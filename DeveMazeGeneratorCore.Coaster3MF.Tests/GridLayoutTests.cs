namespace DeveMazeGeneratorCore.Coaster3MF.Tests
{
    [TestFixture]
    public class GridLayoutTests
    {
        // Helper method to access the private CalculateGridDimensions method via reflection
        private (int width, int height) CalculateGridDimensions(int plateCount)
        {
            // Since CalculateGridDimensions is private static, we can test it via reflection
            var method = typeof(MazeCoaster3MF).GetMethod(
                "CalculateGridDimensions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = method!.Invoke(null, new object[] { plateCount });
            return ((int, int))result!;
        }

        [Test]
        public void CalculateGridDimensions_1Plate_Returns1x1()
        {
            var (width, height) = CalculateGridDimensions(1);
            Assert.That(width, Is.EqualTo(1));
            Assert.That(height, Is.EqualTo(1));
        }

        [Test]
        public void CalculateGridDimensions_4Plates_Returns2x2()
        {
            var (width, height) = CalculateGridDimensions(4);
            Assert.That(width, Is.EqualTo(2));
            Assert.That(height, Is.EqualTo(2));
        }

        [Test]
        public void CalculateGridDimensions_5Plates_Returns3x2()
        {
            var (width, height) = CalculateGridDimensions(5);
            Assert.That(width, Is.EqualTo(3));
            Assert.That(height, Is.EqualTo(2));
        }

        [Test]
        public void CalculateGridDimensions_9Plates_Returns3x3()
        {
            var (width, height) = CalculateGridDimensions(9);
            Assert.That(width, Is.EqualTo(3));
            Assert.That(height, Is.EqualTo(3));
        }

        [Test]
        public void CalculateGridDimensions_10Plates_Returns4x3()
        {
            var (width, height) = CalculateGridDimensions(10);
            Assert.That(width, Is.EqualTo(4));
            Assert.That(height, Is.EqualTo(3));
        }

        [Test]
        public void CalculateGridDimensions_17Plates_Returns5x4()
        {
            var (width, height) = CalculateGridDimensions(17);
            Assert.That(width, Is.EqualTo(5));
            Assert.That(height, Is.EqualTo(4));
        }

        [Test]
        public void CalculateGridDimensions_20Plates_Returns5x4()
        {
            var (width, height) = CalculateGridDimensions(20);
            Assert.That(width, Is.EqualTo(5));
            Assert.That(height, Is.EqualTo(4));
        }

        [Test]
        public void CalculateGridDimensions_25Plates_Returns5x5()
        {
            var (width, height) = CalculateGridDimensions(25);
            Assert.That(width, Is.EqualTo(5));
            Assert.That(height, Is.EqualTo(5));
        }

        [Test]
        public void CalculateGridDimensions_100Plates_Returns10x10()
        {
            var (width, height) = CalculateGridDimensions(100);
            Assert.That(width, Is.EqualTo(10));
            Assert.That(height, Is.EqualTo(10));
        }
    }
}
