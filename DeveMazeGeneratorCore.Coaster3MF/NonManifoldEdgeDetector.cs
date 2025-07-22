using DeveMazeGeneratorCore.Coaster3MF.Models;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    /// <summary>
    /// Detects non-manifold edges and other mesh topology issues in 3D meshes.
    /// Non-manifold edges are edges that are shared by more than 2 triangles or have inconsistent winding.
    /// </summary>
    public class NonManifoldEdgeDetector
    {
        /// <summary>
        /// Represents an edge between two vertices.
        /// </summary>
        public record Edge(int V1, int V2)
        {
            /// <summary>
            /// Creates a normalized edge where V1 <= V2 for consistent comparison.
            /// </summary>
            public static Edge CreateNormalized(int v1, int v2)
            {
                return v1 <= v2 ? new Edge(v1, v2) : new Edge(v2, v1);
            }
        }

        /// <summary>
        /// Represents an edge with direction (for winding order checks).
        /// </summary>
        public record DirectedEdge(int From, int To);

        /// <summary>
        /// Results of non-manifold edge detection.
        /// </summary>
        public class DetectionResult
        {
            public bool IsManifold => !HasNonManifoldEdges && !HasInconsistentWinding && !HasDuplicateVertices;

            public bool HasNonManifoldEdges { get; set; }
            public bool HasInconsistentWinding { get; set; }
            public bool HasDuplicateVertices { get; set; }

            public List<Edge> NonManifoldEdges { get; } = new();
            public List<Edge> BorderEdges { get; } = new();
            public List<DirectedEdge> InconsistentEdges { get; } = new();
            public List<(int Index1, int Index2)> DuplicateVertices { get; } = new();

            public Dictionary<Edge, int> EdgeTriangleCounts { get; } = new();

            public string ToString(string prefix)
            {
                return $"{prefix}Manifold: {IsManifold}{Environment.NewLine}" +
                       $"{prefix}Non-manifold edges: {NonManifoldEdges.Count}{Environment.NewLine}" +
                       $"{prefix}Border edges: {BorderEdges.Count}{Environment.NewLine}" +
                       $"{prefix}Inconsistent winding edges: {InconsistentEdges.Count}{Environment.NewLine}" +
                       $"{prefix}Duplicate vertices: {DuplicateVertices.Count}{Environment.NewLine}" +
                       $"{prefix}Edge triangle counts: {EdgeTriangleCounts.Count}";
            }

            public override string ToString() => ToString(string.Empty);
        }

        /// <summary>
        /// Analyzes a mesh for non-manifold edges and other topology issues.
        /// </summary>
        /// <param name="meshData">The mesh to analyze</param>
        /// <returns>Detection results containing all found issues</returns>
        public DetectionResult AnalyzeMesh(MeshData meshData)
        {
            var result = new DetectionResult();

            // Detect duplicate vertices
            DetectDuplicateVertices(meshData, result);

            // Analyze edge topology
            AnalyzeEdgeTopology(meshData, result);

            // Check winding consistency
            CheckWindingConsistency(meshData, result);

            return result;
        }

        public List<Edge> AnalyzeMeshOnlyBorderEdges(MeshData meshData)
        {
            var result = new DetectionResult();
            // Analyze edge topology
            AnalyzeEdgeTopology(meshData, result);
            // No winding consistency check for border edges only
            return result.BorderEdges;
        }

        /// <summary>
        /// Detects vertices that are at the same position (within tolerance).
        /// </summary>
        private void DetectDuplicateVertices(MeshData meshData, DetectionResult result)
        {
            const float tolerance = 1e-6f;

            for (int i = 0; i < meshData.Vertices.Count; i++)
            {
                for (int j = i + 1; j < meshData.Vertices.Count; j++)
                {
                    var v1 = meshData.Vertices[i];
                    var v2 = meshData.Vertices[j];

                    var dx = v1.X - v2.X;
                    var dy = v1.Y - v2.Y;
                    var dz = v1.Z - v2.Z;
                    var distanceSquared = dx * dx + dy * dy + dz * dz;

                    if (distanceSquared < tolerance * tolerance)
                    {
                        result.DuplicateVertices.Add((i, j));
                        result.HasDuplicateVertices = true;
                    }
                }
            }
        }

        /// <summary>
        /// Analyzes edge topology to find non-manifold edges and border edges.
        /// </summary>
        private void AnalyzeEdgeTopology(MeshData meshData, DetectionResult result)
        {
            var edgeCounts = new Dictionary<Edge, int>();

            // Count how many triangles share each edge
            foreach (var triangle in meshData.Triangles)
            {
                var edges = new[]
                {
                    Edge.CreateNormalized(triangle.V1, triangle.V2),
                    Edge.CreateNormalized(triangle.V2, triangle.V3),
                    Edge.CreateNormalized(triangle.V3, triangle.V1)
                };

                foreach (var edge in edges)
                {
                    edgeCounts[edge] = edgeCounts.GetValueOrDefault(edge, 0) + 1;
                }
            }

            // Classify edges based on triangle count
            foreach (var (edge, count) in edgeCounts)
            {
                result.EdgeTriangleCounts[edge] = count;

                if (count == 1)
                {
                    // Border edge (shared by only 1 triangle)
                    result.BorderEdges.Add(edge);
                }
                else if (count > 2)
                {
                    // Non-manifold edge (shared by more than 2 triangles)
                    result.NonManifoldEdges.Add(edge);
                    result.HasNonManifoldEdges = true;
                }
            }
        }

        /// <summary>
        /// Checks for consistent winding order across adjacent triangles.
        /// </summary>
        private void CheckWindingConsistency(MeshData meshData, DetectionResult result)
        {
            var edgeToTriangles = new Dictionary<Edge, List<(Triangle Triangle, bool IsForward)>>();

            // Map each edge to the triangles that use it and track winding direction
            foreach (var triangle in meshData.Triangles)
            {
                var edges = new[]
                {
                    (Edge.CreateNormalized(triangle.V1, triangle.V2), triangle.V1 < triangle.V2),
                    (Edge.CreateNormalized(triangle.V2, triangle.V3), triangle.V2 < triangle.V3),
                    (Edge.CreateNormalized(triangle.V3, triangle.V1), triangle.V3 < triangle.V1)
                };

                foreach (var (edge, isForward) in edges)
                {
                    if (!edgeToTriangles.ContainsKey(edge))
                        edgeToTriangles[edge] = new List<(Triangle, bool)>();

                    edgeToTriangles[edge].Add((triangle, isForward));
                }
            }

            // Check for inconsistent winding on edges shared by exactly 2 triangles
            foreach (var (edge, triangleInfos) in edgeToTriangles)
            {
                if (triangleInfos.Count == 2)
                {
                    var first = triangleInfos[0];
                    var second = triangleInfos[1];

                    // Adjacent triangles should have opposite winding on shared edge
                    if (first.IsForward == second.IsForward)
                    {
                        result.InconsistentEdges.Add(new DirectedEdge(edge.V1, edge.V2));
                        result.HasInconsistentWinding = true;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a simple test mesh with the specified topology issues for testing.
        /// </summary>
        public static MeshData CreateTestMesh(string meshType)
        {
            var meshData = new MeshData();

            switch (meshType.ToLowerInvariant())
            {
                case "validcube":
                    CreateValidCube(meshData);
                    break;

                case "nonmanifoldy":
                    CreateNonManifoldYMesh(meshData);
                    break;

                case "borderhole":
                    CreateMeshWithHole(meshData);
                    break;

                case "inconsistentwinding":
                    CreateInconsistentWindingMesh(meshData);
                    break;

                case "duplicatevertices":
                    CreateDuplicateVerticesMesh(meshData);
                    break;

                default:
                    throw new ArgumentException($"Unknown test mesh type: {meshType}");
            }

            return meshData;
        }

        private static void CreateValidCube(MeshData meshData)
        {
            // Create a simple valid cube (manifold mesh)
            var vertices = new[]
            {
                new Vertex(0, 0, 0), // 0
                new Vertex(1, 0, 0), // 1
                new Vertex(1, 1, 0), // 2
                new Vertex(0, 1, 0), // 3
                new Vertex(0, 0, 1), // 4
                new Vertex(1, 0, 1), // 5
                new Vertex(1, 1, 1), // 6
                new Vertex(0, 1, 1)  // 7
            };

            meshData.Vertices.AddRange(vertices);

            // Define triangles with consistent winding (counter-clockwise when viewed from outside)
            var triangles = new[]
            {
                // Bottom face (z=0)
                new Triangle(0, 2, 1, ""),
                new Triangle(0, 3, 2, ""),
                
                // Top face (z=1)
                new Triangle(4, 5, 6, ""),
                new Triangle(4, 6, 7, ""),
                
                // Front face (y=0)
                new Triangle(0, 1, 5, ""),
                new Triangle(0, 5, 4, ""),
                
                // Right face (x=1)
                new Triangle(1, 2, 6, ""),
                new Triangle(1, 6, 5, ""),
                
                // Back face (y=1)
                new Triangle(2, 3, 7, ""),
                new Triangle(2, 7, 6, ""),
                
                // Left face (x=0)
                new Triangle(3, 0, 4, ""),
                new Triangle(3, 4, 7, "")
            };

            meshData.Triangles.AddRange(triangles);
        }

        private static void CreateNonManifoldYMesh(MeshData meshData)
        {
            // Create a "Y" shaped mesh where 3 triangles meet at a central edge (non-manifold)
            var vertices = new[]
            {
                new Vertex(0, 0, 0), // 0
                new Vertex(1, 0, 0), // 1 - central vertex
                new Vertex(2, 0, 0), // 2
                new Vertex(0.5f, 1, 0), // 3
                new Vertex(1.5f, 1, 0), // 4
                new Vertex(1, -1, 0), // 5
            };

            meshData.Vertices.AddRange(vertices);

            // Three triangles sharing the edge (0,1) - this creates a non-manifold edge
            var triangles = new[]
            {
                new Triangle(0, 1, 3, ""), // First triangle
                new Triangle(0, 1, 5, ""), // Second triangle (shares edge 0-1)
                new Triangle(1, 0, 2, ""), // Third triangle (shares edge 0-1, but with opposite direction)
            };

            meshData.Triangles.AddRange(triangles);
        }

        private static void CreateMeshWithHole(MeshData meshData)
        {
            // Create a square with a triangular hole (border edges)
            var vertices = new[]
            {
                // Outer square
                new Vertex(0, 0, 0), // 0
                new Vertex(2, 0, 0), // 1
                new Vertex(2, 2, 0), // 2
                new Vertex(0, 2, 0), // 3
                
                // Inner triangle (hole)
                new Vertex(0.5f, 0.5f, 0), // 4
                new Vertex(1.5f, 0.5f, 0), // 5
                new Vertex(1, 1.5f, 0), // 6
            };

            meshData.Vertices.AddRange(vertices);

            var triangles = new[]
            {
                // Outer triangles (with hole)
                new Triangle(0, 1, 4, ""),
                new Triangle(1, 5, 4, ""),
                new Triangle(1, 2, 5, ""),
                new Triangle(2, 6, 5, ""),
                new Triangle(2, 3, 6, ""),
                new Triangle(3, 4, 6, ""),
                new Triangle(3, 0, 4, ""),
                
                // The hole creates border edges around triangle 4-5-6
            };

            meshData.Triangles.AddRange(triangles);
        }

        private static void CreateInconsistentWindingMesh(MeshData meshData)
        {
            // Create two adjacent triangles with inconsistent winding
            var vertices = new[]
            {
                new Vertex(0, 0, 0), // 0
                new Vertex(1, 0, 0), // 1
                new Vertex(0.5f, 1, 0), // 2
                new Vertex(1.5f, 1, 0), // 3
            };

            meshData.Vertices.AddRange(vertices);

            var triangles = new[]
            {
                new Triangle(0, 1, 2, ""), // Counter-clockwise: edges (0,1), (1,2), (2,0)
                new Triangle(1, 2, 3, ""), // Counter-clockwise but same direction on shared edge (1,2) - this is inconsistent
            };

            meshData.Triangles.AddRange(triangles);
        }

        private static void CreateDuplicateVerticesMesh(MeshData meshData)
        {
            // Create a mesh with duplicate vertices at the same position
            var vertices = new[]
            {
                new Vertex(0, 0, 0), // 0
                new Vertex(1, 0, 0), // 1
                new Vertex(0.5f, 1, 0), // 2
                new Vertex(0, 0, 0), // 3 - duplicate of vertex 0
            };

            meshData.Vertices.AddRange(vertices);

            var triangles = new[]
            {
                new Triangle(0, 1, 2, ""),
                new Triangle(3, 1, 2, ""), // Uses duplicate vertex
            };

            meshData.Triangles.AddRange(triangles);
        }
    }
}