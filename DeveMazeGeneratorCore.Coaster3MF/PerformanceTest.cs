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
            int[] mazeSizes = { 100, 150 };
            
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
            
            // Generate quads WITHOUT face culling for proper performance testing
            var geometryGenerator = new MazeGeometryGenerator();
            var unculledQuads = geometryGenerator.GenerateMazeQuads(maze.InnerMap, path, true, false);
            var quads1 = new List<Quad>(unculledQuads); // Copy for vertex-based test  
            var quads2 = new List<Quad>(unculledQuads); // Copy for spatial comparison
            var quads3 = new List<Quad>(unculledQuads); // Copy for original comparison
            
            Console.WriteLine($"Generated {unculledQuads.Count} quads");
            
            // Test optimized vertex-based version (new)
            var sw1 = Stopwatch.StartNew();
            MeshOptimizer.CullHiddenFaces(quads1);
            sw1.Stop();
            
            // Test spatial version (previous optimization)
            var sw2 = Stopwatch.StartNew();
            MeshOptimizer.CullHiddenFacesSpatial(quads2);
            sw2.Stop();
            
            // Test original version (skip for large mazes to avoid timeout)
            long originalTime = 0;
            if (mazeSize <= 50)
            {
                var sw3 = Stopwatch.StartNew();
                MeshOptimizer.CullHiddenFacesOriginal(quads3);
                sw3.Stop();
                originalTime = sw3.ElapsedMilliseconds;
            }
            
            Console.WriteLine($"Vertex-based: {sw1.ElapsedMilliseconds}ms, Spatial: {sw2.ElapsedMilliseconds}ms");
            if (originalTime > 0)
            {
                Console.WriteLine($"Original: {originalTime}ms");
                Console.WriteLine($"Vertex vs Original speedup: {(double)originalTime / sw1.ElapsedMilliseconds:F1}x");
            }
            Console.WriteLine($"Vertex vs Spatial speedup: {(double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds:F1}x");
            
            // Verify results are identical
            if (quads1.Count != quads2.Count || (originalTime > 0 && quads1.Count != quads3.Count))
            {
                Console.WriteLine($"ERROR: Result mismatch! Vertex: {quads1.Count}, Spatial: {quads2.Count}");
                if (originalTime > 0) Console.WriteLine($"Original: {quads3.Count}");
            }
            else
            {
                Console.WriteLine("✓ All results are identical");
            }
        }
    }
}