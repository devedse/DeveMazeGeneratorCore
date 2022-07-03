using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGeneratorCore.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TestHilbert();

            Console.WriteLine(nameof(TestWithGenerics));
            TestWithGenerics();

            Console.WriteLine(nameof(TestWithSave));
            TestWithSave();

            Console.WriteLine(nameof(Test7));
            Test7();

            while (true)
            {
                Console.WriteLine(nameof(ActualBenchmark2));
                ActualBenchmark2();

                Console.ReadKey();
            }
        }

        public static void TestHilbert()
        {
            for (int order = 3; order <= 3; order++)
            {
                var n = 1 << order;
                var points = HilbertCurve.GetPointsForCurve(n);
                Console.WriteLine("Hilbert curve, order={0}", order);
                var lines = HilbertCurve.DrawCurve(points, n);
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine();
            }


            for (int i = 0; i < 1000; i++)
            {
                int xxx = 0;
                int yyy = 0;
                HilbertCurve.D2xy(8, i, ref xxx, ref yyy);
                var ppp = new MazePoint(xxx, yyy);
                var rev = HilbertCurve.Xy2d(8, ppp.X, ppp.Y);

                Console.WriteLine($"i: {i}  -> {ppp} {rev}");
            }

             

            var maze = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2, BitArreintjeFastHilbertInnerMap, XorShiftRandom>(16, 16, null);

            using (var fs = new FileStream($"TestHilbert.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, new List<Structures.MazePointPos>(), fs);
            }
        }

        public static void TestWithGenerics()
        {
            var result = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2, BitArreintjeFastInnerMap, XorShiftRandom>(16384, 16384, null);
        }

        public static void CreateIcons()
        {
            int size = 16;

            for (int i = 0; i < 100; i++)
            {
                var alg = new AlgorithmBacktrack2Deluxe2();

                Console.WriteLine($"Generating maze using {alg.GetType().Name}...");

                int seed = i;

                var w = Stopwatch.StartNew();

                var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
                var randomFactory = new RandomFactory<XorShiftRandom>();

                var actionThing = new NoAction();

                var maze = alg.GoGenerate(size, size, seed, innerMapFactory, randomFactory, actionThing);
                w.Stop();


                var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

                using (var fs = new FileStream($"icon{seed}.png", FileMode.Create))
                {
                    WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, path, fs);
                }
            }
        }

        public static void ActualBenchmark()
        {
            int size = 16384;
            var fastestElapsed = TimeSpan.MaxValue;


            int seed = 1337;
            while (true)
            {
                var w = Stopwatch.StartNew();
                Console.WriteLine($"Generating mazes using {nameof(AlgorithmBacktrack)}...");
                var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, XorShiftRandom>(size, size, null);
                w.Stop();

                bool foundFastest = false;
                if (w.Elapsed < fastestElapsed)
                {
                    foundFastest = true;
                    fastestElapsed = w.Elapsed;
                }

                Console.WriteLine($"Generation time: {w.Elapsed}" + (foundFastest ? " <<<<<<<< new fastest time" : ""));
                seed++;
            }
        }

        public static void ActualBenchmark2()
        {
            int size = 16384;
            var fastestElapsed = TimeSpan.MaxValue;

            var alg = new AlgorithmBacktrack2Deluxe2();

            Console.WriteLine($"Generating mazes using {alg.GetType().Name}...");

            int seed = 1337;
            while (true)
            {
                var w = Stopwatch.StartNew();

                var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
                var randomFactory = new RandomFactory<XorShiftRandom>();

                var actionThing = new NoAction();

                var maze = alg.GoGenerate(size, size, seed, innerMapFactory, randomFactory, actionThing);
                w.Stop();

                bool foundFastest = false;
                if (w.Elapsed < fastestElapsed)
                {
                    foundFastest = true;
                    fastestElapsed = w.Elapsed;
                }

                var strToPrint = $"Generation time: {w.Elapsed}" + (foundFastest ? " <<<<<<<< new fastest time" : "");
                var strToPrint2 = $"{strToPrint.PadRight(68, ' ')} Fastest: {fastestElapsed}";

                Console.WriteLine(strToPrint2);
                seed++;

                //using (var fs = new FileStream($"GeneratedMazeNoPath{alg.GetType().Name}.png", FileMode.Create))
                //{
                //    WithPath.SaveMazeAsImageDeluxePng(maze, new System.Collections.Generic.List<Structures.MazePointPos>(), fs);
                //}

                //Console.WriteLine("Finding path");

                //var path = PathFinderDepthFirstSmartWithPos.GoFind(maze, null);
                //Console.WriteLine("Found path :)");

                //using (var fs = new FileStream($"GeneratedMaze{alg.GetType().Name}.png", FileMode.Create))
                //{
                //    WithPath.SaveMazeAsImageDeluxePng(maze, path, fs);
                //}

                //return;
                //var result = MazeVerifier.IsPerfectMaze(maze);
                //Console.WriteLine($"Perfect maze verification time: {w.Elapsed}");
                //Console.WriteLine($"Is our maze perfect?: {result}");
            }
        }

        public static void TestWithSave()
        {
            int size = 128;
            var fastestElapsed = TimeSpan.MaxValue;

            var alg = new AlgorithmBacktrack2Deluxe2();

            Console.WriteLine($"Generating maze using {alg.GetType().Name}...");

            int seed = 1337;

            var w = Stopwatch.StartNew();

            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();

            var actionThing = new NoAction();

            var maze = alg.GoGenerate(size, size, seed, innerMapFactory, randomFactory, actionThing);
            w.Stop();

            using (var fs = new FileStream($"SmallTestGeneratedMazeNoPath{alg.GetType().Name}.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, new List<Structures.MazePointPos>(), fs);
            }

            Console.WriteLine($"Generation time: {w.Elapsed}");


            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

            using (var fs = new FileStream($"SmallTestGeneratedMaze{alg.GetType().Name}.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, path, fs);
            }


            //var result = MazeVerifier.IsPerfectMaze(maze);
            //Console.WriteLine($"Perfect maze verification time: {w.Elapsed}");
            //Console.WriteLine($"Is our maze perfect?: {result}");


        }

        public static void Test7()
        {
            int size = 1024;

            var maze = (MazeWithPathAsInnerMap)MazeGenerator.Generate<AlgorithmDivisionDynamicWithPath, BitArreintjeFastInnerMap, XorShiftRandom>(size, size, null);

            using (var fs = new FileStream("DivisionDynamicWithPath.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, maze.PathMap, fs);
            }

            //var alg = new AlgorithmDivisionDynamic();
            //var maze = alg.Generate<UndefinedInnerMap, NetRandom>(size, size, null);

            //using (var fs = new FileStream("DivisionDynamicWithPath.png", FileMode.Create))
            //{
            //    WithoutPath.MazeToImage(maze, fs);
            //}
        }

        public static void Test6()
        {
            int size = 4096;

            var w = Stopwatch.StartNew();
            var maze = MazeGenerator.Generate<AlgorithmKruskal, BitArreintjeFastInnerMap, NetRandom>(size, size, null);
            Console.WriteLine($"Generation time: {w.Elapsed}");

            w.Restart();
            var result = MazeVerifier.IsPerfectMaze(maze.InnerMap);
            Console.WriteLine($"Perfect maze verification time: {w.Elapsed}");
            Console.WriteLine($"Is our maze perfect?: {result}");

            using (var fs = new FileStream("KruskalMaze.png", FileMode.Create))
            {
                WithoutPath.MazeToImage(maze.InnerMap, fs);
            }
        }

        public static void Test3()
        {
            int size = 128;

            var maze = MazeGenerator.Generate<AlgorithmDivisionDynamic, BitArreintjeFastInnerMap, XorShiftRandom>(size, size, null);

            WithoutPath.SaveMaze(Path.Combine($"dinges.png"), maze.InnerMap);

            var otherThing = new AlgorithmDivisionDynamicOldTestingThing(size, size, 1337);

            int b = size / 2;
            var part1 = otherThing.GenerateMapPart(0, 0, b, b);
            var part2 = otherThing.GenerateMapPart(b, 0, b, b);
            var part3 = otherThing.GenerateMapPart(0, b, b, b);
            var part4 = otherThing.GenerateMapPart(b, b, b, b);

            WithoutPath.SaveMaze("part1.png", part1);
            WithoutPath.SaveMaze("part2.png", part2);
            WithoutPath.SaveMaze("part3.png", part3);
            WithoutPath.SaveMaze("part4.png", part4);
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
                    WithoutPath.SaveMaze(Path.Combine("Images", $"{x}_{y}.png"), part);
                    Console.WriteLine($"{x}_{y}.png : {w.Elapsed}");
                }
            }


            var part1 = alg.GenerateMapPart(0, 0, b, b);
            //var part2 = alg.GenerateMapPart(b, 0, b, b);
            //var part3 = alg.GenerateMapPart(0, b, b, b);
            //var part4 = alg.GenerateMapPart(b, b, b, b);

            WithoutPath.SaveMaze("part1.png", part1);
            //SaveMaze("part2.png", part2);
            //SaveMaze("part3.png", part3);
            //SaveMaze("part4.png", part4);
        }

        public static void Test1()
        {
            int size = 16384;

            var w = Stopwatch.StartNew();
            var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, XorShiftRandom>(size, size, null);

            Console.WriteLine($"Generated maze in {w.Elapsed}");
            Console.WriteLine("Saving maze...");

            w.Restart();
            WithoutPath.SaveMaze("output.png", maze.InnerMap);

            Console.WriteLine($"Saved maze in: {w.Elapsed}");


            //Console.WriteLine(map.GenerateMapAsString());

            Console.WriteLine("Written file");
        }
    }
}
