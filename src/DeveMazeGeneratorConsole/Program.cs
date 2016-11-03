using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Helpers;
using DeveMazeGenerator.InnerMaps;
using System;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGeneratorConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Test4();

            Console.ReadKey();
        }

        public static void Test4()
        {
            int size = 16384;

            var alg = new AlgorithmBacktrack();
            var w = Stopwatch.StartNew();
            var maze = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(size, size, 1337, null);
            Console.WriteLine($"Generation time: {w.Elapsed}");

            w.Restart();
            var result = MazeVerifier.IsPerfectMaze(maze);
            Console.WriteLine($"Perfect maze verification time: {w.Elapsed}");
            Console.WriteLine($"Is our maze perfect?: {result}");
        }

        public static void Test3()
        {
            int size = 128;

            var alg = new AlgorithmDivisionDynamic();
            var maze = alg.Generate<UndefinedInnerMap, NetRandom>(size, size, 1337, null);

            MazeImager.SaveMaze(Path.Combine($"dinges.png"), maze);


            var otherThing = new AlgorithmDivisionDynamicOldTestingThing(size, size, 1337);

            int b = size / 2;
            var part1 = otherThing.GenerateMapPart(0, 0, b, b);
            var part2 = otherThing.GenerateMapPart(b, 0, b, b);
            var part3 = otherThing.GenerateMapPart(0, b, b, b);
            var part4 = otherThing.GenerateMapPart(b, b, b, b);

            MazeImager.SaveMaze("part1.png", part1);
            MazeImager.SaveMaze("part2.png", part2);
            MazeImager.SaveMaze("part3.png", part3);
            MazeImager.SaveMaze("part4.png", part4);
        }

        public static void Test2()
        {
            var netRandom = new NetRandom(1337);
            var a0 = netRandom.Next();
            var b0 = netRandom.Next();

            netRandom.Reinitialise(100);
            var a1 = netRandom.Next();
            var b1 = netRandom.Next();

            var netRandomNew = new NetRandom(100);
            var a2 = netRandomNew.Next();
            var b2 = netRandomNew.Next();



            int totSize = 128;

            Console.WriteLine($"Tot size: {totSize}");

            var alg = new AlgorithmDivisionDynamicOldTestingThing(totSize, totSize, 1337);


            for (int y = 0; y < 8; y++)
            {
                var ww = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    var a = alg.GenerateMapPart(0, 0, 256, 256);
                }
                Console.WriteLine(ww.Elapsed);
            }

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
                    MazeImager.SaveMaze(Path.Combine("Images", $"{x}_{y}.png"), part);
                    Console.WriteLine($"{x}_{y}.png : {w.Elapsed}");
                }
            }


            var part1 = alg.GenerateMapPart(0, 0, b, b);
            //var part2 = alg.GenerateMapPart(b, 0, b, b);
            //var part3 = alg.GenerateMapPart(0, b, b, b);
            //var part4 = alg.GenerateMapPart(b, b, b, b);

            MazeImager.SaveMaze("part1.png", part1);
            //SaveMaze("part2.png", part2);
            //SaveMaze("part3.png", part3);
            //SaveMaze("part4.png", part4);
        }

        public static void Test1()
        {
            var alg = new AlgorithmBacktrack();

            int size = 16384;

            var w = Stopwatch.StartNew();
            var map = alg.Generate<BitArreintjeFastInnerMap, XorShiftRandom>(size, size, null);

            Console.WriteLine($"Generated maze in {w.Elapsed}");
            Console.WriteLine("Saving maze...");

            w.Restart();
            MazeImager.SaveMaze("output.png", map);

            Console.WriteLine($"Saved maze in: {w.Elapsed}");


            //Console.WriteLine(map.GenerateMapAsString());

            Console.WriteLine("Written file");
        }
    }
}
