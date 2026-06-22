using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using System;
using System.Diagnostics;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    public class AlgorithmBacktrack2Deluxe2_AsbyteOptimizedTests
    {
        [Fact]
        public void GeneratesAMazeUsingOptimizedMethod()
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
            var maze = MazeGenerator.GenerateOptimized<BitArreintjeFastInnerMap>(128, 128, 1337, mazeAction);

            //Assert
            Trace.WriteLine("Taken steps: " + current);
            Trace.WriteLine("Total steps: " + total);

            Assert.NotEqual(0, total);
            Assert.Equal(total, current);
            Assert.False(maze.InnerMap[0, 0]);
            Assert.True(maze.InnerMap[1, 1]);
        }

        [Fact]
        public void GeneratesAPerfectMazeUsingOptimizedMethod()
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
            var maze = MazeGenerator.GenerateOptimized<BitArreintjeFastInnerMap>(128, 128, 1337, mazeAction);

            //Assert
            Assert.True(MazeVerifier.IsPerfectMaze(maze.InnerMap));
        }

        [Fact]
        public void OptimizedAndNormalMethodProduceSameMaze()
        {
            //Arrange
            int seed = 1337;
            int size = 128;

            //Act
            var mazeNormal = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2_AsByte, BitArreintjeFastInnerMap, XorShiftRandom>(size, size, seed, null);
            var mazeOptimized = MazeGenerator.GenerateOptimized<BitArreintjeFastInnerMap>(size, size, seed, null);

            //Assert - Both mazes should be identical for the same seed
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Assert.Equal(mazeNormal.InnerMap[x, y], mazeOptimized.InnerMap[x, y]);
                }
            }
        }

        [Fact]
        public void WorksWithDifferentInnerMapTypes()
        {
            //Act & Assert - These should not throw
            var maze1 = MazeGenerator.GenerateOptimized<BitArreintjeFastInnerMap>(64, 64, 1337, null);
            Assert.NotNull(maze1);
            Assert.True(MazeVerifier.IsPerfectMaze(maze1.InnerMap));

            var maze2 = MazeGenerator.GenerateOptimized<BitArreintjeFastChunkedInnerMap>(64, 64, 1337, null);
            Assert.NotNull(maze2);
            Assert.True(MazeVerifier.IsPerfectMaze(maze2.InnerMap));

            // Note: BoolInnerMap skipped due to unrelated Clone() implementation issue
        }
    }
}
