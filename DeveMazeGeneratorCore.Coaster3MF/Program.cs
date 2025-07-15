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
            
            // Parse command line arguments
            int mazeSize = 20; // default
            int? seed = null;
            
            if (args.Length > 0 && int.TryParse(args[0], out int size))
            {
                mazeSize = size;
            }
            
            if (args.Length > 1 && int.TryParse(args[1], out int seedValue))
            {
                seed = seedValue;
            }
            
            // Generate a maze using AlgorithmBacktrack2Deluxe2_AsByte
            var mazeCoaster = new MazeCoaster3MF();
            
            if (args.Length > 0)
            {
                // Single maze with specified parameters
                var filename = $"maze_coaster_{mazeSize}x{mazeSize}.3mf";
                Console.WriteLine($"Creating {filename}...");
                mazeCoaster.Generate3MFCoaster(filename, mazeSize, seed);
                
                if (File.Exists(filename))
                {
                    var fileInfo = new FileInfo(filename);
                    Console.WriteLine($"File created: {filename} ({fileInfo.Length:N0} bytes)");
                }
            }
            else
            {
                // Generate with different seeds for variety
                Console.WriteLine("Generating 3 different coaster designs...");
                
                for (int i = 1; i <= 3; i++)
                {
                    var filename = $"maze_coaster_{i}.3mf";
                    Console.WriteLine($"Creating {filename}...");
                    mazeCoaster.Generate3MFCoaster(filename, 20, 1337 + i);
                }
                
                Console.WriteLine("All 3MF coasters generated successfully!");
                Console.WriteLine("Files created:");
                
                for (int i = 1; i <= 3; i++)
                {
                    var filename = $"maze_coaster_{i}.3mf";
                    if (File.Exists(filename))
                    {
                        var fileInfo = new FileInfo(filename);
                        Console.WriteLine($"  {filename} ({fileInfo.Length:N0} bytes)");
                    }
                }
            }
        }
    }
}
