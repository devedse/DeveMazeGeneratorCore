using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using ImageProcessorCore;
using System;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGeneratorConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var map = new BitArreintjeFastInnerMap(16384, 16384);
            var alg = new AlgorithmBacktrack();



            var w = Stopwatch.StartNew();
            alg.Generate(map, null);
            var elapsed = w.Elapsed;



            Console.WriteLine($"Done in: {elapsed}");

            using (var fs = new FileStream("output.png", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                MazeToImage(map, fs);
            }
            //Console.WriteLine(map.GenerateMapAsString());

            Console.WriteLine("Written file");

            Console.ReadKey();
        }

        public static void MazeToImage(InnerMap map, Stream stream)
        {
            var image = new Image(map.Width, map.Height);
            using (var pixels = image.Lock())
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        var wall = map[x, y];

                        if (!wall)
                        {
                            pixels[x, y] = Color.Black;
                        }
                        else
                        {
                            pixels[x, y] = Color.White;
                        }
                    }
                }
            }

            image.SaveAsPng(stream);
        }
    }
}
