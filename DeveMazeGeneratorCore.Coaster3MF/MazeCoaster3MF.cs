using DeveMazeGeneratorCore.Coaster3MF.Models;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class MazeCoaster3MF
    {
        private readonly MazeGeometryGenerator _geometryGenerator;
        private readonly ThreeMFPackageGenerator _packageGenerator;


        /// PartId 1, 3, 5, 7
        /// ObjectId 2, 4, 6, 8
        /// ModelId1, 2, 3, 4
        private int _partId = 1;
        private int _modelId = 1;
        private int _objectId = 2;

        private ThreeMFModel GenerateModel(MeshData meshData)
        {
            var retval = new ThreeMFModel(_partId, _objectId, _modelId, meshData);
            _partId += 2;
            _objectId += 2;
            _modelId++;
            return retval;
        }


        public MazeCoaster3MF()
        {
            _geometryGenerator = new MazeGeometryGenerator();
            _packageGenerator = new ThreeMFPackageGenerator();
        }

        public void Generate3MFCoaster(int mazeSize, int? seed = null, int? chunkItUp = null)
        {
            var mazes = new List<(Maze maze, List<MazePointPos> path, int seed)>();
            mazes.Add(GenerateMazeWithPath(mazeSize, seed));

            Generate3MFCoasterFromMazes(mazes, mazeSize, chunkItUp);
        }

        public void Generate3MFCoasterMultiple(int mazeSize, int count, int? startSeed = null)
        {
            Console.WriteLine($"Generating {count} mazes of size {mazeSize}x{mazeSize}...");

            var mazes = new List<(Maze maze, List<MazePointPos> path, int seed)>();
            var usedStartSeed = startSeed ?? 1337;

            for (int i = 0; i < count; i++)
            {
                var currentSeed = usedStartSeed + i;
                mazes.Add(GenerateMazeWithPath(mazeSize, currentSeed));
            }

            Generate3MFCoasterFromMazes(mazes, mazeSize, chunkItUp: null);
        }

        private (Maze maze, List<MazePointPos> path, int seed) GenerateMazeWithPath(int mazeSize, int? seed)
        {
            var usedSeed = seed ?? 1337;
            Console.WriteLine($"Generating {mazeSize}x{mazeSize} maze with seed {usedSeed}...");

            // Generate maze using AlgorithmBacktrack2Deluxe2_AsByte
            var maze = GenerateMaze(mazeSize, usedSeed);

            Console.WriteLine("Finding path through maze...");

            // Find the path with position information
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

            Console.WriteLine($"Path found with {path.Count} points (seed: {usedSeed})");

            return (maze, path, usedSeed);
        }

        private void Generate3MFCoasterFromMazes(List<(Maze maze, List<MazePointPos> path, int seed)> mazes, int mazeSize, int? chunkItUp)
        {
            Console.WriteLine("Generating 3MF file...");

            var mazesToCoasterUp = new List<(MazeWithPath mazeWithPath, int seed)>();

            foreach (var (maze, path, seed) in mazes)
            {
                var fullMazeWithPath = new MazeWithPath(maze.InnerMap, path);

                if (chunkItUp.HasValue && chunkItUp.Value > 0)
                {
                    Console.WriteLine($"Chunking up maze (seed {seed}) into {chunkItUp.Value} parts...");
                    var splittedMaze = MazeSplitter.SplitUpMazeIntoChunks(fullMazeWithPath, chunkItUp.Value).ToList();
                    foreach (var chunk in splittedMaze)
                    {
                        mazesToCoasterUp.Add((chunk, seed));
                    }
                }
                else
                {
                    mazesToCoasterUp.Add((fullMazeWithPath, seed));
                }
            }

            int plateNumber = 0;
            // Bunch up the mesh into plates (4 coasters per plate)
            var plates = mazesToCoasterUp.Chunk(4).Select(t =>
            {
                var plateModels = t.Select(item =>
                {
                    // Generate the geometry data for each maze chunk
                    var meshData = _geometryGenerator.GenerateMazeGeometry(item.mazeWithPath.InnerMap, item.mazeWithPath.Path, seed: item.seed);
                    Validate(mazeSize, meshData);
                    return GenerateModel(meshData);
                }).ToList();
                return new ThreeMFPlate(Interlocked.Increment(ref plateNumber), plateModels);
            }).ToList();


            // Generate filename with triangle and vertex counts
            var totalTriangles = plates.Sum(p => p.Models.Sum(m => m.MeshData.Triangles.Count));
            var totalVertices = plates.Sum(p => p.Models.Sum(m => m.MeshData.Vertices.Count));

            var firstMaze = mazes.First();
            var seedDescription = mazes.Count == 1
                ? $"seed{firstMaze.seed}"
                : $"seeds{mazes.First().seed}-{mazes.Last().seed}_count{mazes.Count}";

            var filename = $"maze_coaster_{mazeSize}x{mazeSize}_{seedDescription}_{totalTriangles}tri_{totalVertices}vert.3mf";

            Console.WriteLine($"Creating file: {filename}");

            // Generate the 3MF file
            _packageGenerator.Create3MFFile(filename, plates, firstMaze.maze.InnerMap, firstMaze.path);

            // Generate preview image
            // using (var fs = new FileStream($"{filename}.png", FileMode.Create))
            // {
            //     WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, new List<MazePointPos>(), fs);
            // }
        }

        private static void Validate(int mazeSize, Models.MeshData meshData)
        {
            var nonManifoldEdgeDetector = new NonManifoldEdgeDetector();
            if (mazeSize < 50)
            {
                var meshAnalyzeResult = nonManifoldEdgeDetector.AnalyzeMesh(meshData);
                Console.WriteLine($"Non-manifold edges detected:{Environment.NewLine}{meshAnalyzeResult.ToString("\t")}");
            }
            else if (mazeSize < 100)
            {
                var meshAnalyzeResult = nonManifoldEdgeDetector.AnalyzeMeshOnlyBorderEdges(meshData);
                Console.WriteLine($"Border edges: {meshAnalyzeResult.Count}");
            }
            else
            {
                Console.WriteLine("Skipping non-manifold edge detection for large maze size.");
            }
        }

        private Maze GenerateMaze(int mazeSize, int? seed)
        {
            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();
            var actionThing = new NoAction();

            var usedSeed = seed ?? 1337;
            return alg.GoGenerate(mazeSize, mazeSize, usedSeed, innerMapFactory, randomFactory, actionThing);
        }
    }
}