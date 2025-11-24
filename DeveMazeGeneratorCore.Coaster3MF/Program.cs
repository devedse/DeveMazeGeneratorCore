using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using System.IO.Compression;
using System.Xml;
using System.Text;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DeveMazeGeneratorCore 3MF Coaster Maker");

            // Check if we should run performance comparison
            if (args.Length > 0 && args[0] == "--perf")
            {
                PerformanceTest.RunComparison();
                return;
            }

            // Generate a 20x20 maze using AlgorithmBacktrack2Deluxe2_AsByte
            var mazeCoaster = new MazeCoaster3MF();

            // Generate with different seeds for variety
            Console.WriteLine("Generating 3 different coaster designs...");


            // Delete all 3mf files in the current directory
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.3mf");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted file: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                }
            }

            //for (int i = 1; i <= 3; i++)
            //{
            //    Console.WriteLine($"Creating coaster {i}...");
            //    mazeCoaster.Generate3MFCoaster(57, 1337 + i, 57 / 3);
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}

            //for (int i = 1; i <= 3; i++)
            //{
            //    Console.WriteLine($"Creating coaster {i}...");
            //    mazeCoaster.Generate3MFCoaster(19, i);
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}

            // Generate 10 small mazes in one file
            mazeCoaster.Generate3MFCoasterMultiple(19, 25, startSeed: 1);

            Console.WriteLine("All 3MF coasters generated successfully!");
        }
    }
}
