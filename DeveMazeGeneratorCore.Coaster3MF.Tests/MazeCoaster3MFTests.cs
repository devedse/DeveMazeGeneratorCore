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
        public void Generate3MFCoaster_CreatesValidFile()
        {
            // Arrange
            var coaster = new DeveMazeGeneratorCore.Coaster3MF.MazeCoaster3MF();
            var filename = "test_coaster.3mf";

            // Clean up any existing file
            if (File.Exists(filename))
                File.Delete(filename);

            // Act
            coaster.Generate3MFCoaster(filename, 10, 1337);

            // Assert
            Assert.True(File.Exists(filename), "3MF file should be created");
            Assert.True(new FileInfo(filename).Length > 0, "3MF file should not be empty");

            // Verify it's a valid ZIP file (3MF files are ZIP archives)
            using (var archive = System.IO.Compression.ZipFile.OpenRead(filename))
            {
                Assert.Contains(archive.Entries, e => e.FullName == "[Content_Types].xml");
                Assert.Contains(archive.Entries, e => e.FullName == "_rels/.rels");
                Assert.Contains(archive.Entries, e => e.FullName == "3D/3dmodel.model");
            }

            // Clean up
            File.Delete(filename);
        }

        [Fact]
        public void Generate3MFCoaster_WithDifferentSeeds_CreatesDifferentFiles()
        {
            // Arrange
            var coaster = new DeveMazeGeneratorCore.Coaster3MF.MazeCoaster3MF();
            var filename1 = "test_coaster_1.3mf";
            var filename2 = "test_coaster_2.3mf";

            // Clean up any existing files
            if (File.Exists(filename1)) File.Delete(filename1);
            if (File.Exists(filename2)) File.Delete(filename2);

            // Act
            coaster.Generate3MFCoaster(filename1, 10, 1337);
            coaster.Generate3MFCoaster(filename2, 10, 7331);

            // Assert
            Assert.True(File.Exists(filename1), "First 3MF file should be created");
            Assert.True(File.Exists(filename2), "Second 3MF file should be created");

            var file1Size = new FileInfo(filename1).Length;
            var file2Size = new FileInfo(filename2).Length;

            Assert.True(file1Size > 0, "First file should not be empty");
            Assert.True(file2Size > 0, "Second file should not be empty");

            // Files should be different (different mazes)
            var file1Bytes = File.ReadAllBytes(filename1);
            var file2Bytes = File.ReadAllBytes(filename2);
            Assert.False(file1Bytes.SequenceEqual(file2Bytes), "Files with different seeds should be different");

            // Clean up
            File.Delete(filename1);
            File.Delete(filename2);
        }
    }
}
