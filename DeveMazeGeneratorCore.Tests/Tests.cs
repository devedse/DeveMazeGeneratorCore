using Xunit;
using System.IO;
using System.Linq;

namespace DeveMazeGeneratorCore.Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }

    public class MazeCoaster3MFTests
    {
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
            coaster.Generate3MFCoaster(filename, 1337);
            
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
            
            // Act - Use smaller sizes for the test to avoid long running times
            coaster.Generate3MFCoaster(filename1, 500); // Reduced from 1337
            coaster.Generate3MFCoaster(filename2, 500, 7331); // Reduced from 7331 size
            
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

        [Fact]
        public void Generate3MFCoaster_LargeMaze_CompletesWithoutCrashing()
        {
            // Arrange
            var coaster = new DeveMazeGeneratorCore.Coaster3MF.MazeCoaster3MF();
            var filename = "test_large_coaster.3mf";
            
            // Clean up any existing file
            if (File.Exists(filename))
                File.Delete(filename);
            
            // Act - Test with a larger maze that previously would have caused issues
            coaster.Generate3MFCoaster(filename, 2000, 1337);
            
            // Assert
            Assert.True(File.Exists(filename), "Large 3MF file should be created");
            Assert.True(new FileInfo(filename).Length > 0, "Large 3MF file should not be empty");
            
            // Clean up
            File.Delete(filename);
        }
    }
}
