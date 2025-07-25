using DeveMazeGeneratorCore.Coaster3MF.Models;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.PathFinders;

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

        public void Generate3MFCoaster(int mazeSize, int? seed = null)
        {
            Console.WriteLine($"Generating {mazeSize}x{mazeSize} maze...");

            // Generate maze using AlgorithmBacktrack2Deluxe2_AsByte
            var maze = GenerateMaze(mazeSize, seed);

            Console.WriteLine("Finding path through maze...");

            // Find the path with position information
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

            Console.WriteLine($"Path found with {path.Count} points (seed: {seed ?? 1337})");
            Console.WriteLine("Generating 3MF file...");

            // Generate the geometry data
            var meshData = _geometryGenerator.GenerateMazeGeometry(maze.InnerMap, path);

            Validate(mazeSize, meshData);



            var plates = new List<ThreeMFPlate>()
            {
                new ThreeMFPlate(1, new List<ThreeMFModel>
                {
                    GenerateModel(meshData),
                    GenerateModel(meshData),
                    GenerateModel(meshData),
                })
            };


            // Generate filename with triangle and vertex counts
            var usedSeed = seed ?? 1337;
            var filename = $"maze_coaster_{mazeSize}x{mazeSize}_seed{usedSeed}_{meshData.Triangles.Count}tri_{meshData.Vertices.Count}vert.3mf";

            Console.WriteLine($"Creating file: {filename}");

            // Generate the 3MF file
            _packageGenerator.Create3MFFile(filename, plates, maze.InnerMap, path);

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