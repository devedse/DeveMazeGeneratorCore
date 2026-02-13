using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    namespace AlgorithmBacktrackFacts
    {
        public class TheGenerateMethodWithUnsafeMap
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
                var maze = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2_AsByte, BitArreintjeFastInnerMapUnsafe, XorShiftRandom>(128, 128, mazeAction);

                var map = maze.InnerMap.GenerateMapAsString();

                //Save map as image
                var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

                using (var fs = new FileStream($"{nameof(TheGenerateMethodWithUnsafeMap)}_{nameof(GeneratesAMaze)}.png", FileMode.Create))
                {
                    WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, path, fs);
                }

                Console.WriteLine(map);

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
                var maze = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2_AsByte, BitArreintjeFastInnerMapUnsafe, XorShiftRandom>(128, 128, mazeAction);

                Assert.True(MazeVerifier.IsPerfectMaze(maze.InnerMap));
            }
        }
    }
}

