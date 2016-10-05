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
            Test2();

            Console.ReadKey();
        }

        public static void Test2()
        {
            int totSize = 1024;

            Console.WriteLine($"Tot size: {totSize}");

            var alg = new AlgorithmDivisionDynamic(totSize, totSize, 1337);

            //var partTot = alg.GenerateMapPart(0, 0, totSize, totSize);
            //SaveMaze("parttot.png", partTot);

            int b = 256;

            var dir = Directory.CreateDirectory("Images");
            var w = new Stopwatch();

            var dinges = totSize / b;
            if (totSize % b != 0)
            {
                dinges += 1;
            }
            for (int y = 0; y < dinges; y++)
            {
                for (int x = 0; x < dinges; x++)
                {
                    w.Restart();
                    var part = alg.GenerateMapPart(x * b, y * b, b, b);
                    w.Stop();
                    SaveMaze(Path.Combine("Images", $"{x}_{y}.png"), part);
                    Console.WriteLine($"{x}_{y}.png : {w.Elapsed}");
                }
            }


            var part1 = alg.GenerateMapPart(768, 0, b, b);
            //var part2 = alg.GenerateMapPart(b, 0, b, b);
            //var part3 = alg.GenerateMapPart(0, b, b, b);
            //var part4 = alg.GenerateMapPart(b, b, b, b);

            SaveMaze("part1.png", part1);
            //SaveMaze("part2.png", part2);
            //SaveMaze("part3.png", part3);
            //SaveMaze("part4.png", part4);
        }

        public static void Test1()
        {
            var map = new BitArreintjeFastInnerMap(16384, 16384);
            var alg = new AlgorithmBacktrack();



            var w = Stopwatch.StartNew();
            alg.Generate(map, null);
            var elapsed = w.Elapsed;

            SaveMaze("output.png", map);

            Console.WriteLine($"Done in: {elapsed}");


            //Console.WriteLine(map.GenerateMapAsString());

            Console.WriteLine("Written file");
        }

        public static void SaveMaze(string fileName, InnerMap maze)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                MazeToImage(maze, fs);
            }
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
