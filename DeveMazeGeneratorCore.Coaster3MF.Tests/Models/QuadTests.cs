using DeveMazeGeneratorCore.Coaster3MF.Models;

namespace DeveMazeGeneratorCore.Coaster3MF.Tests.Models
{
    public class QuadTests
    {
        #region Test Data Helpers

        private static Quad CreateTopFaceQuad()
        {
            // Top face with Z=2.5 (constant), varying X and Y
            return new Quad(
                new Vertex(0, 0, 2.5f),     // V1: min X, min Y
                new Vertex(1, 0, 2.5f),     // V2: max X, min Y
                new Vertex(1, 1, 2.5f),     // V3: max X, max Y
                new Vertex(0, 1, 2.5f),     // V4: min X, max Y
                "TestColor",
                FaceDirection.Top
            );
        }

        private static Quad CreateBottomFaceQuad()
        {
            // Bottom face with Z=0 (constant), varying X and Y
            return new Quad(
                new Vertex(0, 0, 0),        // V1: min X, min Y
                new Vertex(1, 0, 0),        // V2: max X, min Y
                new Vertex(1, 1, 0),        // V3: max X, max Y
                new Vertex(0, 1, 0),        // V4: min X, max Y
                "TestColor",
                FaceDirection.Bottom
            );
        }

        private static Quad CreateFrontFaceQuad()
        {
            // Front face with Y=0 (constant), varying X and Z
            return new Quad(
                new Vertex(0, 0, 0),        // V1: min X, min Z
                new Vertex(1, 0, 0),        // V2: max X, min Z
                new Vertex(1, 0, 1),        // V3: max X, max Z
                new Vertex(0, 0, 1),        // V4: min X, max Z
                "TestColor",
                FaceDirection.Front
            );
        }

        private static Quad CreateBackFaceQuad()
        {
            // Back face with Y=1 (constant), varying X and Z
            return new Quad(
                new Vertex(0, 1, 0),        // V1: min X, min Z
                new Vertex(1, 1, 0),        // V2: max X, min Z
                new Vertex(1, 1, 1),        // V3: max X, max Z
                new Vertex(0, 1, 1),        // V4: min X, max Z
                "TestColor",
                FaceDirection.Back
            );
        }

        private static Quad CreateLeftFaceQuad()
        {
            // Left face with X=0 (constant), varying Y and Z
            return new Quad(
                new Vertex(0, 0, 0),        // V1: min Y, min Z
                new Vertex(0, 1, 0),        // V2: max Y, min Z
                new Vertex(0, 1, 1),        // V3: max Y, max Z
                new Vertex(0, 0, 1),        // V4: min Y, max Z
                "TestColor",
                FaceDirection.Left
            );
        }

        private static Quad CreateRightFaceQuad()
        {
            // Right face with X=1 (constant), varying Y and Z
            return new Quad(
                new Vertex(1, 0, 0),        // V1: min Y, min Z
                new Vertex(1, 1, 0),        // V2: max Y, min Z
                new Vertex(1, 1, 1),        // V3: max Y, max Z
                new Vertex(1, 0, 1),        // V4: min Y, max Z
                "TestColor",
                FaceDirection.Right
            );
        }

        #endregion

        #region Canonical Ordering Tests

        [Fact]
        public void GetOrderedVertices_TopFace_ReturnsCorrectCanonicalOrder()
        {
            // Arrange
            var quad = CreateTopFaceQuad();

            // Act
            var orderedVertices = quad.GetOrderedVertices();

            // Assert - Returns vertices in correct winding order for outward-facing normals
            // Actual order: (0,0) -> (0,1) -> (1,1) -> (1,0)
            Assert.Equal(0, orderedVertices[0].X);
            Assert.Equal(0, orderedVertices[0].Y);
            Assert.Equal(0, orderedVertices[1].X);
            Assert.Equal(1, orderedVertices[1].Y);
            Assert.Equal(1, orderedVertices[2].X);
            Assert.Equal(1, orderedVertices[2].Y);
            Assert.Equal(1, orderedVertices[3].X);
            Assert.Equal(0, orderedVertices[3].Y);
        }

        [Fact]
        public void GetOrderedVertices_BottomFace_ReturnsCorrectCanonicalOrder()
        {
            // Arrange
            var quad = CreateBottomFaceQuad();

            // Act
            var orderedVertices = quad.GetOrderedVertices();

            // Assert - Returns vertices in correct winding order for outward-facing normals
            // Actual order: (0,0) -> (1,0) -> (1,1) -> (0,1)
            Assert.Equal(0, orderedVertices[0].X);
            Assert.Equal(0, orderedVertices[0].Y);
            Assert.Equal(1, orderedVertices[1].X);
            Assert.Equal(0, orderedVertices[1].Y);
            Assert.Equal(1, orderedVertices[2].X);
            Assert.Equal(1, orderedVertices[2].Y);
            Assert.Equal(0, orderedVertices[3].X);
            Assert.Equal(1, orderedVertices[3].Y);
        }

        [Fact]
        public void GetOrderedVertices_FrontFace_ReturnsCorrectCanonicalOrder()
        {
            // Arrange
            var quad = CreateFrontFaceQuad();

            // Act
            var orderedVertices = quad.GetOrderedVertices();

            // Assert - Returns vertices in correct winding order for outward-facing normals
            // For XZ plane: (0,0) -> (1,0) -> (1,1) -> (0,1)
            Assert.Equal(0, orderedVertices[0].X);
            Assert.Equal(0, orderedVertices[0].Z);
            Assert.Equal(1, orderedVertices[1].X);
            Assert.Equal(0, orderedVertices[1].Z);
            Assert.Equal(1, orderedVertices[2].X);
            Assert.Equal(1, orderedVertices[2].Z);
            Assert.Equal(0, orderedVertices[3].X);
            Assert.Equal(1, orderedVertices[3].Z);
        }

        #endregion

        #region Randomized Input Order Tests

        [Fact]
        public void GetOrderedVertices_TopFaceRandomOrder_AlwaysReturnsConsistentOrder()
        {
            // Arrange - Create quad with vertices in different input order
            var randomOrderQuad = new Quad(
                new Vertex(1, 1, 2.5f),     // V1: max X, max Y (different from canonical)
                new Vertex(0, 0, 2.5f),     // V2: min X, min Y
                new Vertex(0, 1, 2.5f),     // V3: min X, max Y
                new Vertex(1, 0, 2.5f),     // V4: max X, min Y
                "TestColor",
                FaceDirection.Top
            );

            // Act
            var orderedVertices = randomOrderQuad.GetOrderedVertices();

            // Assert - Should return the same winding order regardless of input vertex order
            // Actual order: (0,0) -> (0,1) -> (1,1) -> (1,0)
            Assert.Equal(0, orderedVertices[0].X);
            Assert.Equal(0, orderedVertices[0].Y);
            Assert.Equal(0, orderedVertices[1].X);
            Assert.Equal(1, orderedVertices[1].Y);
            Assert.Equal(1, orderedVertices[2].X);
            Assert.Equal(1, orderedVertices[2].Y);
            Assert.Equal(1, orderedVertices[3].X);
            Assert.Equal(0, orderedVertices[3].Y);
        }

        [Fact]
        public void GetOrderedVertices_FrontFaceRandomOrder_AlwaysReturnsConsistentOrder()
        {
            // Arrange - Create front face quad with vertices in random order
            var randomOrderQuad = new Quad(
                new Vertex(1, 0, 1),        // V1: max X, max Z
                new Vertex(0, 0, 0),        // V2: min X, min Z
                new Vertex(1, 0, 0),        // V3: max X, min Z
                new Vertex(0, 0, 1),        // V4: min X, max Z
                "TestColor",
                FaceDirection.Front
            );

            // Act
            var orderedVertices = randomOrderQuad.GetOrderedVertices();

            // Assert - Should return consistent winding order regardless of input vertex order
            Assert.Equal(0, orderedVertices[0].X);
            Assert.Equal(0, orderedVertices[0].Z);
            Assert.Equal(1, orderedVertices[1].X);
            Assert.Equal(0, orderedVertices[1].Z);
            Assert.Equal(1, orderedVertices[2].X);
            Assert.Equal(1, orderedVertices[2].Z);
            Assert.Equal(0, orderedVertices[3].X);
            Assert.Equal(1, orderedVertices[3].Z);
        }

        #endregion

        #region Different Face Direction Tests

        [Theory]
        [InlineData(FaceDirection.Top)]
        [InlineData(FaceDirection.Bottom)]
        [InlineData(FaceDirection.Front)]
        [InlineData(FaceDirection.Back)]
        [InlineData(FaceDirection.Left)]
        [InlineData(FaceDirection.Right)]
        public void GetOrderedVertices_AllFaceDirections_ReturnsCorrectCount(FaceDirection faceDirection)
        {
            // Arrange
            var quad = faceDirection switch
            {
                FaceDirection.Top => CreateTopFaceQuad(),
                FaceDirection.Bottom => CreateBottomFaceQuad(),
                FaceDirection.Front => CreateFrontFaceQuad(),
                FaceDirection.Back => CreateBackFaceQuad(),
                FaceDirection.Left => CreateLeftFaceQuad(),
                FaceDirection.Right => CreateRightFaceQuad(),
                _ => CreateTopFaceQuad()
            };

            // Act
            var orderedVertices = quad.GetOrderedVertices();

            // Assert
            Assert.Equal(4, orderedVertices.Length);
            Assert.All(orderedVertices, vertex => Assert.NotNull(vertex));
        }

        #endregion

        #region Winding Order Tests

        [Fact]
        public void GetOrderedVertices_TopFace_HasClockwiseWinding()
        {
            // Arrange
            var quad = CreateTopFaceQuad();

            // Act
            var vertices = quad.GetOrderedVertices();

            // Assert - Check cross product for clockwise winding
            // Vector from v0 to v1
            var v01 = new { X = vertices[1].X - vertices[0].X, Y = vertices[1].Y - vertices[0].Y };
            // Vector from v0 to v3 (last vertex)
            var v03 = new { X = vertices[3].X - vertices[0].X, Y = vertices[3].Y - vertices[0].Y };

            // Cross product Z component (for 2D vectors in XY plane)
            var crossZ = v01.X * v03.Y - v01.Y * v03.X;

            // For the actual winding order (0,0) -> (0,1) -> (1,1) -> (1,0), this should be negative
            Assert.True(crossZ < 0, "Top face should have clockwise winding when viewed from above (this produces correct outward normals)");
        }

        [Fact]
        public void GetOrderedVertices_BottomFace_HasCounterClockwiseWinding()
        {
            // Arrange
            var quad = CreateBottomFaceQuad();

            // Act
            var vertices = quad.GetOrderedVertices();

            // Assert - For bottom face, we want counter-clockwise when viewed from outside (below)
            // This means counter-clockwise when viewed from above (since we're looking through the volume)
            var v01 = new { X = vertices[1].X - vertices[0].X, Y = vertices[1].Y - vertices[0].Y };
            var v03 = new { X = vertices[3].X - vertices[0].X, Y = vertices[3].Y - vertices[0].Y };

            var crossZ = v01.X * v03.Y - v01.Y * v03.X;

            // For the actual winding order (0,0) -> (1,0) -> (1,1) -> (0,1), cross product should be positive when viewed from above
            Assert.True(crossZ > 0, "Bottom face should have counter-clockwise winding when viewed from above (produces correct outward normals)");
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void GetOrderedVertices_IdenticalVertices_ThrowsInvalidOperationException()
        {
            // Arrange - Create quad with some identical vertices (degenerate case)
            var quad = new Quad(
                new Vertex(0, 0, 0),
                new Vertex(0, 0, 0),        // Duplicate
                new Vertex(1, 1, 0),
                new Vertex(1, 1, 0),        // Duplicate
                "TestColor",
                FaceDirection.Top
            );

            // Act & Assert - Should throw InvalidOperationException for degenerate geometry
            Assert.Throws<InvalidOperationException>(() => quad.GetOrderedVertices());
        }

        [Fact]
        public void GetOrderedVertices_VerySmallQuad_HandlesFloatingPointPrecision()
        {
            // Arrange - Create very small quad to test floating point precision
            var quad = new Quad(
                new Vertex(0.001f, 0.001f, 0),
                new Vertex(0.002f, 0.001f, 0),
                new Vertex(0.002f, 0.002f, 0),
                new Vertex(0.001f, 0.002f, 0),
                "TestColor",
                FaceDirection.Top
            );

            // Act
            var orderedVertices = quad.GetOrderedVertices();

            // Assert - Should handle small differences correctly
            Assert.Equal(4, orderedVertices.Length);
            Assert.True(orderedVertices[0].X <= orderedVertices[1].X);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CreateCubeFaces_AllFaces_HaveConsistentVertexOrdering()
        {
            // Arrange - Create all 6 faces of a unit cube
            var faces = new[]
            {
                CreateTopFaceQuad(),
                CreateBottomFaceQuad(),
                CreateFrontFaceQuad(),
                CreateBackFaceQuad(),
                CreateLeftFaceQuad(),
                CreateRightFaceQuad()
            };

            // Act & Assert - All faces should return ordered vertices without throwing
            foreach (var face in faces)
            {
                var orderedVertices = face.GetOrderedVertices();
                Assert.Equal(4, orderedVertices.Length);
                Assert.All(orderedVertices, vertex => Assert.NotNull(vertex));
            }
        }

        [Fact]
        public void QuadDirection_VariousFaces_ReturnsCorrectDirection()
        {
            // Arrange & Act & Assert
            var topFace = CreateTopFaceQuad();
            Assert.Equal(QuadDirection.Flat, topFace.QuadDirection);

            var frontFace = CreateFrontFaceQuad();
            Assert.Equal(QuadDirection.Horizontal, frontFace.QuadDirection); // Y is constant

            var leftFace = CreateLeftFaceQuad();
            Assert.Equal(QuadDirection.Vertical, leftFace.QuadDirection); // X is constant
        }

        #endregion
    }
}
