using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using System;
using System.Diagnostics;

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


            //Console.WriteLine(map.GenerateMapAsString());


            Console.ReadKey();
        }
    }
}
