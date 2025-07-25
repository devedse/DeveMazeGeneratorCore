namespace DeveMazeGeneratorCore.Coaster3MF.Tests
{
    public class NonManifoldEdgeDetectorTests
    {
        [Fact]
        public void AnalyzeMesh_ValidCube_ReturnsManifoldResult()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = NonManifoldEdgeDetector.CreateTestMesh("validcube");

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.True(result.IsManifold, "Valid cube should be manifold");
            Assert.False(result.HasNonManifoldEdges, "Valid cube should not have non-manifold edges");
            Assert.False(result.HasInconsistentWinding, "Valid cube should have consistent winding");
            Assert.False(result.HasDuplicateVertices, "Valid cube should not have duplicate vertices");
            Assert.Empty(result.NonManifoldEdges);
            Assert.Empty(result.InconsistentEdges);
            Assert.Empty(result.DuplicateVertices);
        }

        [Fact]
        public void AnalyzeMesh_NonManifoldYMesh_DetectsNonManifoldEdges()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = NonManifoldEdgeDetector.CreateTestMesh("nonmanifoldy");

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.False(result.IsManifold, "Y-shaped mesh should not be manifold");
            Assert.True(result.HasNonManifoldEdges, "Y-shaped mesh should have non-manifold edges");
            Assert.NotEmpty(result.NonManifoldEdges);

            // Verify at least one edge is shared by more than 2 triangles
            Assert.Contains(result.EdgeTriangleCounts, kvp => kvp.Value > 2);
        }

        [Fact]
        public void AnalyzeMesh_MeshWithHole_DetectsBorderEdges()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = NonManifoldEdgeDetector.CreateTestMesh("borderhole");

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.NotEmpty(result.BorderEdges);

            // Verify at least one edge is shared by only 1 triangle (border edge)
            Assert.Contains(result.EdgeTriangleCounts, kvp => kvp.Value == 1);
        }

        [Fact]
        public void AnalyzeMesh_InconsistentWindingMesh_DetectsWindingIssues()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = NonManifoldEdgeDetector.CreateTestMesh("inconsistentwinding");

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.False(result.IsManifold, "Mesh with inconsistent winding should not be manifold");
            Assert.True(result.HasInconsistentWinding, "Should detect inconsistent winding");
            Assert.NotEmpty(result.InconsistentEdges);
        }

        [Fact]
        public void AnalyzeMesh_DuplicateVerticesMesh_DetectsDuplicateVertices()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = NonManifoldEdgeDetector.CreateTestMesh("duplicatevertices");

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.False(result.IsManifold, "Mesh with duplicate vertices should not be manifold");
            Assert.True(result.HasDuplicateVertices, "Should detect duplicate vertices");
            Assert.NotEmpty(result.DuplicateVertices);

            // Should find vertices 0 and 3 as duplicates
            Assert.Contains(result.DuplicateVertices, pair =>
                (pair.Index1 == 0 && pair.Index2 == 3) || (pair.Index1 == 3 && pair.Index2 == 0));
        }

        [Fact]
        public void AnalyzeMesh_EmptyMesh_HandlesGracefully()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = new Coaster3MF.Models.MeshData();

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.True(result.IsManifold, "Empty mesh should be considered manifold");
            Assert.False(result.HasNonManifoldEdges);
            Assert.False(result.HasInconsistentWinding);
            Assert.False(result.HasDuplicateVertices);
        }

        [Fact]
        public void AnalyzeMesh_SingleTriangle_IsManifold()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = new Coaster3MF.Models.MeshData();

            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(0, 0, 0));
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(1, 0, 0));
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(0.5f, 1, 0));
            mesh.Triangles.Add(new Coaster3MF.Models.Triangle(0, 1, 2, ""));

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.True(result.IsManifold, "Single triangle should be manifold");
            Assert.Equal(3, result.BorderEdges.Count); // All 3 edges are border edges
            Assert.Empty(result.NonManifoldEdges);
            Assert.False(result.HasInconsistentWinding); // No adjacent triangles to have inconsistent winding
        }

        [Fact]
        public void AnalyzeMesh_TwoAdjacentTrianglesCorrectWinding_IsManifold()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();
            var mesh = new Coaster3MF.Models.MeshData();

            // Create two triangles sharing an edge with correct winding
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(0, 0, 0)); // 0
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(1, 0, 0)); // 1
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(0.5f, 1, 0)); // 2
            mesh.Vertices.Add(new Coaster3MF.Models.Vertex(1.5f, 1, 0)); // 3

            mesh.Triangles.Add(new Coaster3MF.Models.Triangle(0, 1, 2, "")); // Counter-clockwise
            mesh.Triangles.Add(new Coaster3MF.Models.Triangle(1, 3, 2, "")); // Counter-clockwise, opposite winding on shared edge 1-2

            // Act
            var result = detector.AnalyzeMesh(mesh);

            // Assert
            Assert.True(result.IsManifold, "Two properly connected triangles should be manifold");
            Assert.False(result.HasNonManifoldEdges);
            Assert.False(result.HasInconsistentWinding);
            Assert.Equal(4, result.BorderEdges.Count); // Outer perimeter edges
        }

        [Fact]
        public void AnalyzeMesh_ActualGeneratedMazeGeometry_AnalyzesSuccessfully()
        {
            // Arrange
            var detector = new NonManifoldEdgeDetector();

            // Generate a small maze
            var alg = new Generators.AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new Factories.InnerMapFactory<InnerMaps.BitArreintjeFastInnerMap>();
            var randomFactory = new Factories.RandomFactory<Generators.Helpers.XorShiftRandom>();
            var actionThing = new Generators.SpeedOptimization.NoAction();

            var maze = alg.GoGenerate(5, 5, 1337, innerMapFactory, randomFactory, actionThing);
            var path = PathFinders.PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            var generator = new MazeGeometryGenerator();
            var meshData = generator.GenerateMazeGeometry(maze.InnerMap, path);

            // Act
            var result = detector.AnalyzeMesh(meshData);

            // Assert
            Assert.True(meshData.Vertices.Count > 0, "Generated maze should have vertices");
            Assert.True(meshData.Triangles.Count > 0, "Generated maze should have triangles");

            Assert.Empty(result.InconsistentEdges);

            // The test is primarily to ensure the detector doesn't crash on real data
            // and provides meaningful results
            Assert.NotNull(result);
            Assert.NotNull(result.EdgeTriangleCounts);

            // Log the results for manual inspection during development
            System.Console.WriteLine($"Maze analysis: Manifold={result.IsManifold}, " +
                                   $"NonManifoldEdges={result.NonManifoldEdges.Count}, " +
                                   $"BorderEdges={result.BorderEdges.Count}, " +
                                   $"InconsistentEdges={result.InconsistentEdges.Count}, " +
                                   $"DuplicateVertices={result.DuplicateVertices.Count}");
        }

        [Fact]
        public void CreateTestMesh_AllTestMeshTypes_CreateSuccessfully()
        {
            // Arrange & Act & Assert
            var testMeshTypes = new[] { "validcube", "nonmanifoldy", "borderhole", "inconsistentwinding", "duplicatevertices" };

            foreach (var meshType in testMeshTypes)
            {
                var mesh = NonManifoldEdgeDetector.CreateTestMesh(meshType);

                Assert.NotNull(mesh);
                Assert.True(mesh.Vertices.Count > 0, $"Test mesh '{meshType}' should have vertices");
                Assert.True(mesh.Triangles.Count > 0, $"Test mesh '{meshType}' should have triangles");
            }
        }

        [Fact]
        public void CreateTestMesh_InvalidType_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                NonManifoldEdgeDetector.CreateTestMesh("invalidtype"));
        }

        [Fact]
        public void EdgeCreateNormalized_DifferentOrders_ProducesSameEdge()
        {
            // Arrange & Act
            var edge1 = NonManifoldEdgeDetector.Edge.CreateNormalized(1, 3);
            var edge2 = NonManifoldEdgeDetector.Edge.CreateNormalized(3, 1);

            // Assert
            Assert.Equal(edge1, edge2);
            Assert.True(edge1.V1 <= edge1.V2, "Edge should be normalized with V1 <= V2");
            Assert.True(edge2.V1 <= edge2.V2, "Edge should be normalized with V1 <= V2");
        }
    }
}
