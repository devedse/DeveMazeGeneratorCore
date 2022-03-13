using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using System.IO;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    public class MazeSamples
    {
        [Fact]
        public void GeneratingAMazeWithABlockInTheMiddleWorks()
        {
            var map = new BitArreintjeFastInnerMap(128, 128);

            for (int y = 33; y < 96; y++)
            {
                for (int x = 33; x < 96; x++)
                {
                    map[x, y] = true;
                }
            }

            var mapFactory = new InnerMapFactoryCustom<BitArreintjeFastInnerMap>(map);
            var randomFactory = new RandomFactory<NetRandom>();

            var algorithm = new AlgorithmBacktrack();
            var generatedMap = algorithm.GoGenerate(128, 128, 1337, mapFactory, randomFactory, new NoAction());            

            var path = PathFinderDepthFirstSmartWithPos.GoFind(map, null);

            using (var fs = new FileStream("GeneratingAMazeWithABlockInTheMiddleWorks.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(map, path, fs);
            }
        }
    }
}
