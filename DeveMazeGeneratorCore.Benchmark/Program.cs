using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace DeveMazeGeneratorCore.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running the Benchmark job");

            //var config = DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(200));
            var summary = BenchmarkRunner.Run<MazeBenchmarkJob>();
        }
    }
}