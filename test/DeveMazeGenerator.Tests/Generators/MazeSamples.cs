using DeveMazeGenerator.Factories;
using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Imageification;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.PathFinders;
using System.IO;
using Xunit;

namespace DeveMazeGenerator.Tests.Generators
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
            var randomFactory = new RandomFactory<NetRandom>(1337);

            var algorithm = new AlgorithmBacktrack();
            var generatedMap = algorithm.GoGenerate(mapFactory, randomFactory, null);

            var path = PathFinderDepthFirstSmartWithPos.GoFind(map, null);

            using (var fs = new FileStream("GeneratingAMazeWithABlockInTheMiddleWorks.png", FileMode.Create))
            {
                WithPath.SaveMazeAsImageDeluxePng(map, path, fs);
            }
        }
    }
}
