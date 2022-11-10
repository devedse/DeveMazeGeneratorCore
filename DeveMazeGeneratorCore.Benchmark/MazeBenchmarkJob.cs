using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
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
    [SimpleJob(RuntimeMoniker.Net60), SimpleJob(RuntimeMoniker.Net70)]
    [AsciiDocExporter]
    [HtmlExporter]
    [MarkdownExporterAttribute.GitHub]
    [MinColumn, MaxColumn]
    public class MazeBenchmarkJob
    {
        private const int SIZE = 4096;
        private const int SEED = 1337;

        private InnerMapFactory<BitArreintjeFastInnerMap> _innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
        private RandomFactory<XorShiftRandom> _randomFactory = new RandomFactory<XorShiftRandom>();
        private NoAction _action = new NoAction();

        public IEnumerable<object> Algorithms()
        {
            yield return new AlgorithmBacktrack();
            yield return new AlgorithmBacktrack2();
            yield return new AlgorithmBacktrack2Deluxe();
            yield return new AlgorithmBacktrack2Deluxe_AsByte();
            yield return new AlgorithmBacktrack2Deluxe2();
            yield return new AlgorithmBacktrack2Deluxe2_AsByte();
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
    }
}
