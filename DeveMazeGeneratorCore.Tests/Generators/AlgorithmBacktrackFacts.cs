using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using System;
using System.Diagnostics;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    namespace AlgorithmBacktrackFacts
    {
        public class TheGenerateMethod
        {
            [Fact]
            public void GeneratesAMaze()
            {
                //Arrange
                long current = 0;
                long total = 0;
                var mazeAction = new Action<int, int, long, long>((x, y, cur, tot) =>
                {
                    current = cur;
                    total = tot;
                });

                //Act
                var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, NetRandom>(128, 128, mazeAction);

                //Assert
                Trace.WriteLine("Taken steps: " + current);
                Trace.WriteLine("Total steps: " + total);

                Assert.NotEqual(0, total);
                Assert.Equal(total, current);
                Assert.False(maze.InnerMap[0, 0]);
                Assert.True(maze.InnerMap[1, 1]);
            }

            [Fact]
            public void GeneratesAPerfectMaze()
            {
                //Arrange
                var generator = new AlgorithmBacktrack();

                long current = 0;
                long total = 0;
                var mazeAction = new Action<int, int, long, long>((x, y, cur, tot) =>
                {
                    current = cur;
                    total = tot;
                });

                //Act
                var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, NetRandom>(128, 128, mazeAction);

                Assert.True(MazeVerifier.IsPerfectMaze(maze.InnerMap));
            }
        }
    }
}
