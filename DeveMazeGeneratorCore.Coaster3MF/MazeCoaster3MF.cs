using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Imageification;
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

        public MazeCoaster3MF()
        {
            _geometryGenerator = new MazeGeometryGenerator();
            _packageGenerator = new ThreeMFPackageGenerator();
        }

        public void Generate3MFCoaster(string filename, int mazeSize, int? seed = null)
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

            // Generate the 3MF file
            _packageGenerator.Create3MFFile(maze.InnerMap, path, meshData, filename);

            // Generate preview image
            // using (var fs = new FileStream($"{filename}.png", FileMode.Create))
            // {
            //     WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, new List<MazePointPos>(), fs);
            // }
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

            // Generate filename with triangle and vertex counts
            var usedSeed = seed ?? 1337;
            var filename = $"maze_coaster_{mazeSize}x{mazeSize}_seed{usedSeed}_{meshData.Triangles.Count}tri_{meshData.Vertices.Count}vert.3mf";

            Console.WriteLine($"Creating file: {filename}");

            // Generate the 3MF file
            _packageGenerator.Create3MFFile(maze.InnerMap, path, meshData, filename);

            // Generate preview image
            // using (var fs = new FileStream($"{filename}.png", FileMode.Create))
            // {
            //     WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, new List<MazePointPos>(), fs);
            // }
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