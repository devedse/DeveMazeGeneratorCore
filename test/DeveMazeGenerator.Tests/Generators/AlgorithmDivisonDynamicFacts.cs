using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Helpers;
using DeveMazeGenerator.InnerMaps;
using System;
using Xunit;

namespace DeveMazeGenerator.Tests.Generators
{
    namespace AlgorithmDivisionDynamicFacts
    {
        public class TheGenerateMethod
        {
            [Fact]
            public void GeneratesAPerfectMaze()
            {
                //Arrange
                var generator = new AlgorithmDivisionDynamic();

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