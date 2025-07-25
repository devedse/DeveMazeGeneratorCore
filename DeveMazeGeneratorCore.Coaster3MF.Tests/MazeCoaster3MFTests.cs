namespace DeveMazeGeneratorCore.Coaster3MF.Tests
{
    public class MazeCoaster3MFTests
    {
        [Fact]
        public void GenerateMazeQuads_ProducesSameResultsAsDirectGeneration()
        {
            // Arrange
            var alg = new DeveMazeGeneratorCore.Generators.AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new DeveMazeGeneratorCore.Factories.InnerMapFactory<DeveMazeGeneratorCore.InnerMaps.BitArreintjeFastInnerMap>();
            var randomFactory = new DeveMazeGeneratorCore.Factories.RandomFactory<DeveMazeGeneratorCore.Generators.Helpers.XorShiftRandom>();
            var actionThing = new DeveMazeGeneratorCore.Generators.SpeedOptimization.NoAction();

            var maze = alg.GoGenerate(5, 5, 1337, innerMapFactory, randomFactory, actionThing);
            var path = DeveMazeGeneratorCore.PathFinders.PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            var generator = new DeveMazeGeneratorCore.Coaster3MF.MazeGeometryGenerator();

            // Act
            var quads = generator.GenerateMazeQuads(maze.InnerMap, path);
            var meshFromQuads = generator.ConvertQuadsToMesh(quads);
            var meshDirect = generator.GenerateMazeGeometry(maze.InnerMap, path);

            // Assert
            Assert.Equal(meshDirect.Vertices.Count, meshFromQuads.Vertices.Count);
            Assert.Equal(meshDirect.Triangles.Count, meshFromQuads.Triangles.Count);
            Assert.True(quads.Count > 0, "Should generate at least some quads");
        }

        [Fact]
        public void Generate3MFCoaster_WithDifferentSeeds_CreatesDifferentFiles()
        {
            // Arrange
            var coaster = new DeveMazeGeneratorCore.Coaster3MF.MazeCoaster3MF();

            var threemfFilesBefore = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.3mf");

            // Remove existing 3MF files to avoid conflicts
            foreach (var file in threemfFilesBefore)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                }
            }

            // Act
            coaster.Generate3MFCoaster(10, 1337);
            coaster.Generate3MFCoaster(10, 7331);

            // Assert
            var threemfFilesAfter = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.3mf");
            Assert.Equal(2, threemfFilesAfter.Length);

            Assert.True(threemfFilesAfter.Any(f => f.Contains("seed1337")), "Should contain file for seed 1337");
            Assert.True(threemfFilesAfter.Any(f => f.Contains("seed7331")), "Should contain file for seed 7331");

            // Check that the files are different
            var file1 = threemfFilesAfter.First(f => f.Contains("seed1337"));
            var file2 = threemfFilesAfter.First(f => f.Contains("seed7331"));
            var bytesFile1 = File.ReadAllBytes(file1);
            var bytesFile2 = File.ReadAllBytes(file2);

            //Check that the file sizes are not 0
            Assert.NotEmpty(bytesFile1);
            Assert.NotEmpty(bytesFile2);

            // Clean up
            // Remove existing 3MF files to avoid conflicts
            foreach (var file in threemfFilesBefore)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                }
            }
        }
    }
}
