using BenchmarkDotNet.Running;

namespace DeveMazeGeneratorCore.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running the Benchmark job");
            var summary = BenchmarkRunner.Run<MazeBenchmarkJob>();
        }
    }
}