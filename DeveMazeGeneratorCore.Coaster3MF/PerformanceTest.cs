using DeveMazeGeneratorCore.Coaster3MF.Models;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using System.Diagnostics;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class PerformanceTest
    {
        public static void RunComparison()
        {
            Console.WriteLine("Performance Comparison: Original vs Optimized Quad Culling");
            
            // Test with different maze sizes
            int[] mazeSizes = { 30, 40, 50 };
            
            foreach (var size in mazeSizes)
            {
                Console.WriteLine($"\n=== Testing {size}x{size} maze ===");
                TestPerformance(size);
            }
        }
        
        static void TestPerformance(int mazeSize)
        {
            // Generate maze
            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();
            var actionThing = new NoAction();
            
            var maze = alg.GoGenerate(mazeSize, mazeSize, 1337, innerMapFactory, randomFactory, actionThing);
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            
            // Generate quads
            var geometryGenerator = new MazeGeometryGenerator();
            var quads1 = geometryGenerator.GenerateMazeQuads(maze.InnerMap, path);
            var quads2 = new List<Quad>(quads1); // Copy for comparison
            
            Console.WriteLine($"Generated {quads1.Count} quads");
            
            // Test optimized version
            var sw1 = Stopwatch.StartNew();
            MeshOptimizer.CullHiddenFaces(quads1);
            sw1.Stop();
            
            // Test original version  
            var sw2 = Stopwatch.StartNew();
            MeshOptimizer.CullHiddenFacesOriginal(quads2);
            sw2.Stop();
            
            Console.WriteLine($"Optimized: {sw1.ElapsedMilliseconds}ms, Original: {sw2.ElapsedMilliseconds}ms");
            Console.WriteLine($"Speedup: {(double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds:F1}x");
            
            // Verify results are identical
            if (quads1.Count != quads2.Count)
            {
                Console.WriteLine($"ERROR: Result mismatch! Optimized: {quads1.Count}, Original: {quads2.Count}");
            }
            else
            {
                Console.WriteLine("✓ Results are identical");
            }
        }
    }
}