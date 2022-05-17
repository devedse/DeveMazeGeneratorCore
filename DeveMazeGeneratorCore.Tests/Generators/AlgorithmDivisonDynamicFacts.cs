using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using System;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
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
                var maze = MazeGenerator.Generate<AlgorithmDivisionDynamic, BitArreintjeFastInnerMap, NetRandom>(128, 128, mazeAction);

                Assert.True(MazeVerifier.IsPerfectMaze(maze.InnerMap));
            }
        }
    }
}