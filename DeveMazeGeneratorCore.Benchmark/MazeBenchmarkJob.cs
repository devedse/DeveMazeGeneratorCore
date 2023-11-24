using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Benchmark
{
    [MemoryDiagnoser]
    //[InliningDiagnoser]
    //[TailCallDiagnoser]
    //[EtwProfiler]
    //[ConcurrencyVisualizerProfiler]
    //[NativeMemoryProfiler]
    //[ThreadingDiagnoser]
    [JsonExporterAttribute.Full]
    [JsonExporterAttribute.FullCompressed]
    [
        //DeveJob(RuntimeMoniker.Net60, launchCount: 1, warmupCount: 4, targetCount: 50, invocationCount: 1),
        DeveJob(RuntimeMoniker.Net70, launchCount: 1, warmupCount: 4, targetCount: 50, invocationCount: 1),
    ]
    [AsciiDocExporter]
    [HtmlExporter]
    [MarkdownExporterAttribute.GitHub]
    [MinColumn, MaxColumn]
    [Config(typeof(Config))]
    public class MazeBenchmarkJob
    {
        private const int SIZE = 4096 * 2 * 2;
        private const int SEED = 1337;

        private InnerMapFactory<BitArreintjeFastInnerMap> _innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
        private RandomFactory<XorShiftRandom> _randomFactory = new RandomFactory<XorShiftRandom>();
        private NoAction _action = new NoAction();

        public IEnumerable<object> Algorithms()
        {
            yield return new AlgorithmBacktrack();
            yield return new AlgorithmBacktrack2();
            yield return new AlgorithmBacktrack2Deluxe_AsByte();
            yield return new AlgorithmBacktrack2Deluxe2_AsByte();
            yield return new AlgorithmBacktrack2Deluxe2WithBorder_AsByte();
            yield return new AlgorithmBacktrack3();
            yield return new AlgorithmBacktrack4();
            //yield return new AlgorithmKruskal();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Algorithms))]
        public void Simple(IAlgorithm<Maze> algorithm)
        {
            algorithm.GoGenerate(SIZE, SIZE, SEED, _innerMapFactory, _randomFactory, _action);
        }

        private class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = SummaryStyle.Default.WithMaxParameterColumnWidth(200);
            }
        }
    }
}
