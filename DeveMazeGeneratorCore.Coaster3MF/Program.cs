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
            
            // Generate a 20x20 maze using AlgorithmBacktrack2Deluxe2_AsByte
            var mazeCoaster = new MazeCoaster3MF();
            
            // Generate with different seeds for variety
            Console.WriteLine("Generating 3 different coaster designs...");
            
            var generatedFiles = new List<string>();
            
            for (int i = 1; i <= 3; i++)
            {
                var oldFilename = $"maze_coaster_{i}.3mf";
                Console.WriteLine($"Creating coaster {i}...");
                var actualFilename = mazeCoaster.Generate3MFCoaster(oldFilename, 20, 1337 + i);
                generatedFiles.Add(actualFilename);
            }
            
            Console.WriteLine("All 3MF coasters generated successfully!");
            Console.WriteLine("Files created:");
            
            foreach (var filename in generatedFiles)
            {
                if (File.Exists(filename))
                {
                    var fileInfo = new FileInfo(filename);
                    Console.WriteLine($"  {filename} ({fileInfo.Length:N0} bytes)");
                }
            }
        }
    }
}
