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
            
            for (int i = 3; i <= 3; i++)
            {
                Console.WriteLine($"Creating coaster {i}...");
                mazeCoaster.Generate3MFCoaster(20, 1337 + i);
            }
            
            Console.WriteLine("All 3MF coasters generated successfully!");
        }
    }
}
