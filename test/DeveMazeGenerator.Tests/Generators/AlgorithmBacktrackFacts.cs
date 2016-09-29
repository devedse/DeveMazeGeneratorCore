using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using System;
using Xunit;

namespace DeveMazeGenerator.Tests.Generators
{
    namespace AlgorithmBacktrackFacts
    {
        public class TheGenerateMethod
        {
            [Fact]
            public void GeneratesAMaze()
            {
                //Arrange
                var generator = new AlgorithmBacktrack();
                var innerMap = new BitArreintjeFastInnerMap(128, 128);

                long current = 0;
                long total = 0;
                var mazeAction = new Action<int, int, long, long>((x, y, cur, tot) =>
                {
                    current = cur;
                    total = tot;
                });

                //Act
                generator.Generate(innerMap, mazeAction);

                //Assert
                Assert.NotEqual(0, total);
                Assert.Equal(total, current);
            }
        }
    }
}
