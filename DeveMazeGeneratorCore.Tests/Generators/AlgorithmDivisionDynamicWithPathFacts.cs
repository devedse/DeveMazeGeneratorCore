using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using System;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    namespace AlgorithmDivisionDynamicWithPathFacts
    {
        public class TheGenerateMethod
        {
            [Fact]
            public void GeneratesAPerfectMaze()
            {
                //Arrange
                var generator = new AlgorithmDivisionDynamicWithPath();

                long current = 0;
                long total = 0;
                var mazeAction = new Action<int, int, long, long>((x, y, cur, tot) =>
                {
                    current = cur;
                    total = tot;
                });

                //Act
                var map = generator.Generate<BitArreintjeFastInnerMap, NetRandom>(128, 128, mazeAction);

                Assert.True(MazeVerifier.IsPerfectMaze(map));
            }
        }
    }
}
