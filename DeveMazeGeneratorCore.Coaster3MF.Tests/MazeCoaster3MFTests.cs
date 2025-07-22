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

    public class NonManifoldEdgeDetectorTests
    {
        [Fact]
        public void AnalyzeMesh_ValidCube_ReturnsManifoldResult()
        {
            // Arrange
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("validcube");

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("nonmanifoldy");

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("borderhole");

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("inconsistentwinding");

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("duplicatevertices");

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = new DeveMazeGeneratorCore.Coaster3MF.Models.MeshData();

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = new DeveMazeGeneratorCore.Coaster3MF.Models.MeshData();
            
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(0, 0, 0));
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(1, 0, 0));
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(0.5f, 1, 0));
            mesh.Triangles.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Triangle(0, 1, 2, ""));

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            var mesh = new DeveMazeGeneratorCore.Coaster3MF.Models.MeshData();
            
            // Create two triangles sharing an edge with correct winding
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(0, 0, 0)); // 0
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(1, 0, 0)); // 1
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(0.5f, 1, 0)); // 2
            mesh.Vertices.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Vertex(1.5f, 1, 0)); // 3
            
            mesh.Triangles.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Triangle(0, 1, 2, "")); // Counter-clockwise
            mesh.Triangles.Add(new DeveMazeGeneratorCore.Coaster3MF.Models.Triangle(1, 3, 2, "")); // Counter-clockwise, opposite winding on shared edge 1-2

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
            var detector = new DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector();
            
            // Generate a small maze
            var alg = new DeveMazeGeneratorCore.Generators.AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new DeveMazeGeneratorCore.Factories.InnerMapFactory<DeveMazeGeneratorCore.InnerMaps.BitArreintjeFastInnerMap>();
            var randomFactory = new DeveMazeGeneratorCore.Factories.RandomFactory<DeveMazeGeneratorCore.Generators.Helpers.XorShiftRandom>();
            var actionThing = new DeveMazeGeneratorCore.Generators.SpeedOptimization.NoAction();
            
            var maze = alg.GoGenerate(5, 5, 1337, innerMapFactory, randomFactory, actionThing);
            var path = DeveMazeGeneratorCore.PathFinders.PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            var generator = new DeveMazeGeneratorCore.Coaster3MF.MazeGeometryGenerator();
            var meshData = generator.GenerateMazeGeometry(maze.InnerMap, path);

            // Act
            var result = detector.AnalyzeMesh(meshData);

            // Assert
            Assert.True(meshData.Vertices.Count > 0, "Generated maze should have vertices");
            Assert.True(meshData.Triangles.Count > 0, "Generated maze should have triangles");
            
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
                var mesh = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh(meshType);
                
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
                DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.CreateTestMesh("invalidtype"));
        }

        [Fact]
        public void EdgeCreateNormalized_DifferentOrders_ProducesSameEdge()
        {
            // Arrange & Act
            var edge1 = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.Edge.CreateNormalized(1, 3);
            var edge2 = DeveMazeGeneratorCore.Coaster3MF.NonManifoldEdgeDetector.Edge.CreateNormalized(3, 1);

            // Assert
            Assert.Equal(edge1, edge2);
            Assert.True(edge1.V1 <= edge1.V2, "Edge should be normalized with V1 <= V2");
            Assert.True(edge2.V1 <= edge2.V2, "Edge should be normalized with V1 <= V2");
        }
    }
}
