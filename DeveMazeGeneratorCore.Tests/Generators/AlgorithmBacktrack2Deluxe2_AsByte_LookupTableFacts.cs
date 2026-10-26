using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using Xunit;

namespace DeveMazeGeneratorCore.Tests.Generators
{
    public class AlgorithmBacktrack2Deluxe2_AsByte_LookupTableFacts
    {
        [Theory]
        [InlineData(128, 128, 1337)]
        [InlineData(129, 77, 42)]
        [InlineData(500, 300, 12345)]
        [InlineData(1024, 1024, 7)]
        public void GeneratesTheSameMazeAsAlgorithmBacktrack2Deluxe2_AsByte(int width, int height, int seed)
        {
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();

            var expected = new AlgorithmBacktrack2Deluxe2_AsByte().GoGenerate(width, height, seed, innerMapFactory, randomFactory, new NoAction());
            var actual = new AlgorithmBacktrack2Deluxe2_AsByte_LookupTable().GoGenerate(width, height, seed, innerMapFactory, randomFactory, new NoAction());

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.True(expected.InnerMap[x, y] == actual.InnerMap[x, y], $"Mazes differ at ({x}, {y})");
                }
            }
        }

        [Fact]
        public void GeneratesAPerfectMaze()
        {
            var maze = MazeGenerator.Generate<AlgorithmBacktrack2Deluxe2_AsByte_LookupTable, BitArreintjeFastInnerMap, XorShiftRandom>(128, 128, 1337, null);

            Assert.True(MazeVerifier.IsPerfectMaze(maze.InnerMap));
        }
    }
}
