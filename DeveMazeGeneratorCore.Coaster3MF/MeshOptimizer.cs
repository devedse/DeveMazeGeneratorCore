using DeveMazeGeneratorCore.Coaster3MF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    /// <summary>
    /// Handles optimization and culling operations for mesh geometry represented as quads.
    /// Includes face culling to remove hidden interior faces and quad merging optimizations.
    /// </summary>
    public static class MeshOptimizer
    {
        /// <summary>
        /// Removes quads that are facing each other (interior faces that can't be seen).
        /// This culls faces between adjacent cubes to reduce triangle count.
        /// Optimized version using spatial partitioning to reduce from O(N²) to O(N).
        /// </summary>
        public static void CullHiddenFaces(List<Quad> quads)
        {
            Console.WriteLine($"Found {quads.Count} quads before face culling.");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var quadsToRemove = new HashSet<Quad>();

            // Group quads by face direction and spatial position for efficient lookup
            var spatialGroups = GroupQuadsByPositionAndDirection(quads);
            Console.WriteLine($"Grouped {quads.Count} quads into {spatialGroups.Count} spatial groups in {stopwatch.ElapsedMilliseconds}ms");

            foreach (var group in spatialGroups.Values)
            {
                // Within each spatial group, check for facing pairs
                CullFacingQuadsInGroup(group, quadsToRemove);
            }

            // Remove all marked quads
            foreach (var quad in quadsToRemove)
            {
                quads.Remove(quad);
            }

            stopwatch.Stop();
            Console.WriteLine($"Found {quads.Count} quads after face culling. Removed {quadsToRemove.Count} hidden faces in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Original O(N²) implementation for comparison (kept for testing purposes).
        /// </summary>
        public static void CullHiddenFacesOriginal(List<Quad> quads)
        {
            Console.WriteLine($"Found {quads.Count} quads before face culling (original algorithm).");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var quadsToRemove = new HashSet<Quad>();

            for (int i = 0; i < quads.Count; i++)
            {
                var quad1 = quads[i];
                if (quadsToRemove.Contains(quad1)) continue;

                for (int j = i + 1; j < quads.Count; j++)
                {
                    var quad2 = quads[j];
                    if (quadsToRemove.Contains(quad2)) continue;

                    // Use Quad method to check if these two quads are facing each other and can be culled
                    if (quad1.IsFacing(quad2))
                    {
                        // Both quads are interior faces - remove both
                        quadsToRemove.Add(quad1);
                        quadsToRemove.Add(quad2);
                        break; // quad1 is already marked for removal, move to next
                    }
                }
            }

            // Remove all marked quads
            foreach (var quad in quadsToRemove)
            {
                quads.Remove(quad);
            }

            stopwatch.Stop();
            Console.WriteLine($"Found {quads.Count} quads after face culling (original algorithm). Removed {quadsToRemove.Count} hidden faces in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Groups quads by their spatial position and face direction for efficient culling.
        /// Only quads at the same position with opposite directions can be facing each other.
        /// </summary>
        private static Dictionary<string, List<Quad>> GroupQuadsByPositionAndDirection(List<Quad> quads)
        {
            var groups = new Dictionary<string, List<Quad>>();
            const float tolerance = 0.001f;

            foreach (var quad in quads)
            {
                // Calculate the center position of the quad
                var centerX = (quad.V1.X + quad.V2.X + quad.V3.X + quad.V4.X) / 4f;
                var centerY = (quad.V1.Y + quad.V2.Y + quad.V3.Y + quad.V4.Y) / 4f;
                var centerZ = (quad.V1.Z + quad.V2.Z + quad.V3.Z + quad.V4.Z) / 4f;

                // Round to grid positions to account for floating point precision
                var gridX = (int)Math.Round(centerX / tolerance) * tolerance;
                var gridY = (int)Math.Round(centerY / tolerance) * tolerance;
                var gridZ = (int)Math.Round(centerZ / tolerance) * tolerance;

                // Create a key that represents the spatial position (ignoring face direction for now)
                var positionKey = $"{gridX:F3},{gridY:F3},{gridZ:F3}";

                if (!groups.TryGetValue(positionKey, out var group))
                {
                    group = new List<Quad>();
                    groups[positionKey] = group;
                }

                group.Add(quad);
            }

            return groups;
        }

        /// <summary>
        /// Within a spatial group, find and mark facing quad pairs for removal.
        /// This is much more efficient than the original O(N²) approach.
        /// </summary>
        private static void CullFacingQuadsInGroup(List<Quad> group, HashSet<Quad> quadsToRemove)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var quad1 = group[i];
                if (quadsToRemove.Contains(quad1)) continue;

                for (int j = i + 1; j < group.Count; j++)
                {
                    var quad2 = group[j];
                    if (quadsToRemove.Contains(quad2)) continue;

                    // Use Quad method to check if these two quads are facing each other and can be culled
                    if (quad1.IsFacing(quad2))
                    {
                        // Both quads are interior faces - remove both
                        quadsToRemove.Add(quad1);
                        quadsToRemove.Add(quad2);
                        break; // quad1 is already marked for removal, move to next
                    }
                }
            }
        }

        /// <summary>
        /// Optimizes quads by merging adjacent quads of the same orientation and color.
        /// Only merges quads that share an edge (not diagonally connected ones).
        /// </summary>
        public static void OptimizeQuads(List<Quad> quads)
        {
            Console.WriteLine($"Found {quads.Count} quads before optimization.");

            bool merged;
            do
            {
                merged = false;
                
                for (int i = 0; i < quads.Count && !merged; i++)
                {
                    var currentQuad = quads[i];
                    
                    // Find quads that are actually adjacent (share an edge) and can be merged
                    for (int j = i + 1; j < quads.Count; j++)
                    {
                        var otherQuad = quads[j];
                        
                        if (CanMergeQuads(currentQuad, otherQuad))
                        {
                            var mergedQuad = MergeAdjacentQuads(currentQuad, otherQuad);
                            if (mergedQuad != null)
                            {
                                // Remove the two original quads and add the merged one
                                quads.RemoveAt(j); // Remove higher index first
                                quads.RemoveAt(i);
                                quads.Add(mergedQuad);
                                merged = true;
                                break;
                            }
                        }
                    }
                }
            } while (merged); // Keep merging until no more merges are possible

            Console.WriteLine($"Found {quads.Count} quads after optimization.");
        }

        /// <summary>
        /// Checks if two quads can be merged (same color, face direction, and are adjacent).
        /// </summary>
        private static bool CanMergeQuads(Quad quad1, Quad quad2)
        {
            // Must have same paint color and face direction
            if (quad1.PaintColor != quad2.PaintColor || quad1.FaceDirection != quad2.FaceDirection)
                return false;

            // Must be adjacent (sharing an edge)
            return quad1.IsAdjacentTo(quad2);
        }

        /// <summary>
        /// Merges two adjacent quads into a single larger quad.
        /// Returns null if the quads cannot be merged.
        /// </summary>
        private static Quad? MergeAdjacentQuads(Quad quad1, Quad quad2)
        {
            if (!CanMergeQuads(quad1, quad2))
                return null;

            // Get all vertices from both quads
            var allVertices = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4, quad2.V1, quad2.V2, quad2.V3, quad2.V4 };
            
            // Remove duplicate vertices (the shared edge)
            var uniqueVertices = new List<Vertex>();
            const float tolerance = 0.001f;
            
            foreach (var vertex in allVertices)
            {
                bool isDuplicate = false;
                foreach (var existing in uniqueVertices)
                {
                    if (Math.Abs(vertex.X - existing.X) < tolerance &&
                        Math.Abs(vertex.Y - existing.Y) < tolerance &&
                        Math.Abs(vertex.Z - existing.Z) < tolerance)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    uniqueVertices.Add(vertex);
                }
            }
            
            // Should have exactly 6 unique vertices (8 original - 2 shared)
            if (uniqueVertices.Count != 6)
                return null;
                
            // Find the bounding rectangle of the merged quad
            var minX = uniqueVertices.Min(v => v.X);
            var maxX = uniqueVertices.Max(v => v.X);
            var minY = uniqueVertices.Min(v => v.Y);
            var maxY = uniqueVertices.Max(v => v.Y);
            var minZ = uniqueVertices.Min(v => v.Z);
            var maxZ = uniqueVertices.Max(v => v.Z);
            
            // Create merged quad based on face direction
            Vertex mergedV1, mergedV2, mergedV3, mergedV4;
            
            switch (quad1.FaceDirection)
            {
                case FaceDirection.Top:
                case FaceDirection.Bottom:
                    // Horizontal plane (Z is constant)
                    var z = quad1.FaceDirection == FaceDirection.Top ? maxZ : minZ;
                    mergedV1 = new Vertex(minX, minY, z);
                    mergedV2 = new Vertex(maxX, minY, z);
                    mergedV3 = new Vertex(maxX, maxY, z);
                    mergedV4 = new Vertex(minX, maxY, z);
                    break;
                    
                case FaceDirection.Front:
                case FaceDirection.Back:
                    // Vertical plane (Y is constant)
                    var y = quad1.FaceDirection == FaceDirection.Front ? minY : maxY;
                    mergedV1 = new Vertex(minX, y, minZ);
                    mergedV2 = new Vertex(maxX, y, minZ);
                    mergedV3 = new Vertex(maxX, y, maxZ);
                    mergedV4 = new Vertex(minX, y, maxZ);
                    break;
                    
                case FaceDirection.Left:
                case FaceDirection.Right:
                    // Vertical plane (X is constant)
                    var x = quad1.FaceDirection == FaceDirection.Left ? minX : maxX;
                    mergedV1 = new Vertex(x, minY, minZ);
                    mergedV2 = new Vertex(x, maxY, minZ);
                    mergedV3 = new Vertex(x, maxY, maxZ);
                    mergedV4 = new Vertex(x, minY, maxZ);
                    break;
                    
                default:
                    return null;
            }
            
            return new Quad(mergedV1, mergedV2, mergedV3, mergedV4, quad1.PaintColor, quad1.FaceDirection);
        }

        /// <summary>
        /// RESEARCH: Manifold-aware mesh optimization for cuboid-based maze geometry.
        /// 
        /// PROBLEM STATEMENT:
        /// The original OptimizeQuads method successfully reduces triangle count (~2900 to ~470) 
        /// but creates 400+ border edges, making the mesh non-manifold (not watertight).
        /// 
        /// ROOT CAUSE ANALYSIS:
        /// - Border edges occur when an edge is shared by only 1 triangle instead of 2
        /// - This happens when quad merging creates gaps in the mesh topology
        /// - In cuboid-based geometry, merging faces on one plane affects adjacent planes
        /// 
        /// RESEARCH FINDINGS:
        /// 1. Maze geometry consists of adjacent cubes with 6 faces each
        /// 2. Merging quads within a plane can break edge relationships with perpendicular planes
        /// 3. Manifold topology requires every edge to be shared by exactly 2 triangles
        /// 4. Conservative validation (checking for border edges after each merge) is computationally expensive
        /// 
        /// ATTEMPTED SOLUTIONS:
        /// A) Plane-based grouping: Group quads by face direction and plane, optimize within groups
        ///    - Result: Still created border edges due to inter-plane dependencies
        /// 
        /// B) Conservative validation: Validate topology after each merge attempt
        ///    - Result: Computationally expensive, leads to infinite loops in validation
        /// 
        /// C) Rectangular merging: Only merge quads that form simple rectangles
        ///    - Result: Still breaks topology at cube boundaries
        /// 
        /// FUNDAMENTAL CHALLENGE:
        /// The core issue is that cube-based meshes have complex inter-dependencies between faces.
        /// When you merge two top faces of adjacent cubes, the side faces must also be adjusted
        /// to maintain manifold topology. This requires a holistic approach that considers
        /// the entire cube structure, not just individual face merging.
        /// 
        /// FUTURE RESEARCH DIRECTIONS:
        /// 1. Cube-aware optimization: Work with complete cube structures rather than individual faces
        /// 2. Topology-preserving algorithms: Use algorithms specifically designed for manifold preservation
        /// 3. Alternative representations: Consider using different mesh representations (e.g., CSG operations)
        /// 4. Limited optimization: Focus on safe optimizations that don't risk manifold properties
        /// 
        /// CONCLUSION:
        /// Current mesh optimization is DISABLED to ensure manifold topology is preserved.
        /// The base mesh (after face culling) maintains 0 border edges and is suitable for 3D printing.
        /// Triangle count reduction is less important than maintaining a valid, printable mesh.
        /// </summary>
        public static void OptimizeQuadsManifoldAware(List<Quad> quads)
        {
            Console.WriteLine($"Mesh optimization disabled to preserve manifold topology.");
            Console.WriteLine($"Current quad count: {quads.Count} (no optimization applied)");
            Console.WriteLine($"See MeshOptimizer.OptimizeQuadsManifoldAware documentation for research details.");
            
            // Optimization is intentionally disabled to ensure 0 border edges
            // The research above documents the challenges and attempted solutions
        }
        
        /// <summary>
        /// Very conservative check for whether two quads can be merged.
        /// </summary>
        private static bool CanMergeQuadsConservatively(Quad quad1, Quad quad2)
        {
            // Must have same color and face direction
            if (quad1.PaintColor != quad2.PaintColor || quad1.FaceDirection != quad2.FaceDirection)
                return false;

            // Must be coplanar (same plane)
            if (!AreCoplanar(quad1, quad2))
                return false;

            // Must be adjacent (sharing exactly one edge)
            if (!quad1.IsAdjacentTo(quad2))
                return false;
                
            // Additional conservative checks: ensure they form a simple rectangle
            return CanFormSimpleRectangle(quad1, quad2);
        }
        
        /// <summary>
        /// Checks if two quads can form a simple rectangle (more conservative than the previous version).
        /// </summary>
        private static bool CanFormSimpleRectangle(Quad quad1, Quad quad2)
        {
            var allVertices = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4, quad2.V1, quad2.V2, quad2.V3, quad2.V4 };
            var uniqueVertices = GetUniqueVerticesStatic(allVertices);

            // Must have exactly 6 unique vertices (8 - 2 shared)
            if (uniqueVertices.Count != 6)
                return false;
                
            // Check that they can form a simple rectangular area
            return CheckSimpleRectangularMerge(uniqueVertices, quad1.FaceDirection);
        }
        
        /// <summary>
        /// Simplified check for rectangular merge.
        /// </summary>
        private static bool CheckSimpleRectangularMerge(List<Vertex> vertices, FaceDirection faceDirection)
        {
            if (vertices.Count != 6) return false;
            
            // Get 2D coordinates
            var coords2D = vertices.Select(v => Get2DCoordinates(v, faceDirection)).ToList();
            
            // Group by coordinates to find the pattern
            var xCoords = coords2D.Select(c => c.X).Distinct().OrderBy(x => x).ToList();
            var yCoords = coords2D.Select(c => c.Y).Distinct().OrderBy(y => y).ToList();
            
            // For a valid rectangle merge, we should have exactly 3 unique X coordinates and 2 unique Y coordinates
            // OR 2 unique X coordinates and 3 unique Y coordinates
            return (xCoords.Count == 3 && yCoords.Count == 2) || (xCoords.Count == 2 && yCoords.Count == 3);
        }
        
        /// <summary>
        /// Conservative quad merging that creates simple rectangular quads.
        /// </summary>
        private static Quad? MergeQuadsConservatively(Quad quad1, Quad quad2)
        {
            if (!CanMergeQuadsConservatively(quad1, quad2))
                return null;

            var allVertices = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4, quad2.V1, quad2.V2, quad2.V3, quad2.V4 };
            var uniqueVertices = GetUniqueVerticesStatic(allVertices);

            if (uniqueVertices.Count != 6)
                return null;

            // Find the 4 corner vertices for the merged rectangle
            var corners = FindSimpleRectangleCorners(uniqueVertices, quad1.FaceDirection);
            if (corners == null || corners.Length != 4)
                return null;

            return new Quad(corners[0], corners[1], corners[2], corners[3], quad1.PaintColor, quad1.FaceDirection);
        }
        
        /// <summary>
        /// Finds the 4 corners of a merged rectangle in a simplified way.
        /// </summary>
        private static Vertex[]? FindSimpleRectangleCorners(List<Vertex> vertices, FaceDirection faceDirection)
        {
            if (vertices.Count != 6) return null;
            
            var coords2D = vertices.Select(v => new { Vertex = v, Coord = Get2DCoordinates(v, faceDirection) }).ToList();
            
            // Find extremes
            var minX = coords2D.Min(c => c.Coord.X);
            var maxX = coords2D.Max(c => c.Coord.X);
            var minY = coords2D.Min(c => c.Coord.Y);
            var maxY = coords2D.Max(c => c.Coord.Y);
            
            const float tolerance = 0.001f;
            
            // Find the 4 corner vertices
            var corner1 = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord.X - minX) < tolerance && Math.Abs(c.Coord.Y - minY) < tolerance)?.Vertex;
            var corner2 = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord.X - maxX) < tolerance && Math.Abs(c.Coord.Y - minY) < tolerance)?.Vertex;
            var corner3 = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord.X - maxX) < tolerance && Math.Abs(c.Coord.Y - maxY) < tolerance)?.Vertex;
            var corner4 = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord.X - minX) < tolerance && Math.Abs(c.Coord.Y - maxY) < tolerance)?.Vertex;
                
            if (corner1 == null || corner2 == null || corner3 == null || corner4 == null)
                return null;
                
            return new Vertex[] { corner1, corner2, corner3, corner4 };
        }
        
        /// <summary>
        /// Validates that a set of quads doesn't create new border edges when converted to a mesh.
        /// </summary>
        private static bool ValidateNoNewBorderEdges(List<Quad> quads)
        {
            try
            {
                // Create a temporary geometry generator to convert quads to mesh
                var geometryGenerator = new MazeGeometryGenerator();
                var meshData = geometryGenerator.ConvertQuadsToMesh(quads);
                
                // Check for border edges
                var detector = new NonManifoldEdgeDetector();
                var result = detector.AnalyzeMesh(meshData);
                
                // Return true only if there are no border edges
                return result.BorderEdges.Count == 0;
            }
            catch
            {
                // If there's any error during validation, consider it unsafe
                return false;
            }
        }

        /// <summary>
        /// Groups quads by their plane position and face direction for independent optimization.
        /// </summary>
        private static Dictionary<string, List<Quad>> GroupQuadsByPlane(List<Quad> quads)
        {
            var groups = new Dictionary<string, List<Quad>>();

            foreach (var quad in quads)
            {
                var planeKey = GetPlaneKey(quad);
                if (!groups.ContainsKey(planeKey))
                {
                    groups[planeKey] = new List<Quad>();
                }
                groups[planeKey].Add(quad);
            }

            return groups;
        }

        /// <summary>
        /// Creates a unique key for each plane based on face direction and the constant coordinate.
        /// </summary>
        private static string GetPlaneKey(Quad quad)
        {
            const float tolerance = 0.001f;
            
            return quad.FaceDirection switch
            {
                FaceDirection.Top or FaceDirection.Bottom => 
                    $"{quad.FaceDirection}_{Math.Round(quad.V1.Z / tolerance) * tolerance}",
                FaceDirection.Front or FaceDirection.Back => 
                    $"{quad.FaceDirection}_{Math.Round(quad.V1.Y / tolerance) * tolerance}",
                FaceDirection.Left or FaceDirection.Right => 
                    $"{quad.FaceDirection}_{Math.Round(quad.V1.X / tolerance) * tolerance}",
                _ => $"{quad.FaceDirection}_unknown"
            };
        }

        /// <summary>
        /// Optimizes quads within a single plane by merging rectangular regions.
        /// </summary>
        private static void OptimizeQuadsInPlane(List<Quad> quadsInPlane, string planeKey)
        {
            if (quadsInPlane.Count <= 1) return;

            // Group by color first - only merge quads of the same color
            var colorGroups = quadsInPlane.GroupBy(q => q.PaintColor).ToList();

            foreach (var colorGroup in colorGroups)
            {
                var quadsOfSameColor = colorGroup.ToList();
                if (quadsOfSameColor.Count <= 1) continue;

                // Try to merge quads in this color group
                MergeQuadsInColorGroup(quadsOfSameColor, quadsInPlane);
            }
        }

        /// <summary>
        /// Attempts to merge quads of the same color within a plane using rectangular region detection.
        /// </summary>
        private static void MergeQuadsInColorGroup(List<Quad> quadsOfSameColor, List<Quad> allQuadsInPlane)
        {
            bool merged;
            do
            {
                merged = false;

                // Try to find two adjacent quads that can be merged into a larger rectangle
                for (int i = 0; i < quadsOfSameColor.Count && !merged; i++)
                {
                    for (int j = i + 1; j < quadsOfSameColor.Count; j++)
                    {
                        var quad1 = quadsOfSameColor[i];
                        var quad2 = quadsOfSameColor[j];

                        if (CanMergeQuadsSafely(quad1, quad2))
                        {
                            var mergedQuad = MergeQuadsRectangular(quad1, quad2);
                            if (mergedQuad != null)
                            {
                                // Remove the original quads and add the merged one
                                quadsOfSameColor.RemoveAt(j); // Remove higher index first
                                quadsOfSameColor.RemoveAt(i);
                                allQuadsInPlane.Remove(quad1);
                                allQuadsInPlane.Remove(quad2);

                                quadsOfSameColor.Add(mergedQuad);
                                allQuadsInPlane.Add(mergedQuad);
                                merged = true;
                                break;
                            }
                        }
                    }
                }
            } while (merged);
        }

        /// <summary>
        /// Checks if two quads can be safely merged without creating topology issues.
        /// This is more conservative than the original algorithm.
        /// </summary>
        private static bool CanMergeQuadsSafely(Quad quad1, Quad quad2)
        {
            // Must have same color and face direction
            if (quad1.PaintColor != quad2.PaintColor || quad1.FaceDirection != quad2.FaceDirection)
                return false;

            // Must be coplanar (same plane)
            if (!AreCoplanar(quad1, quad2))
                return false;

            // Must be adjacent (sharing an edge)
            if (!quad1.IsAdjacentTo(quad2))
                return false;

            // Check if they form a rectangular region when merged
            return CanFormRectangle(quad1, quad2);
        }

        /// <summary>
        /// Checks if two quads are coplanar (on the same plane).
        /// </summary>
        private static bool AreCoplanar(Quad quad1, Quad quad2, float tolerance = 0.001f)
        {
            if (quad1.FaceDirection != quad2.FaceDirection)
                return false;

            return quad1.FaceDirection switch
            {
                FaceDirection.Top or FaceDirection.Bottom => 
                    Math.Abs(quad1.V1.Z - quad2.V1.Z) < tolerance,
                FaceDirection.Front or FaceDirection.Back => 
                    Math.Abs(quad1.V1.Y - quad2.V1.Y) < tolerance,
                FaceDirection.Left or FaceDirection.Right => 
                    Math.Abs(quad1.V1.X - quad2.V1.X) < tolerance,
                _ => false
            };
        }

        /// <summary>
        /// Checks if two adjacent quads can form a valid rectangle when merged.
        /// </summary>
        private static bool CanFormRectangle(Quad quad1, Quad quad2)
        {
            // Get all unique vertices from both quads
            var allVertices = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4, quad2.V1, quad2.V2, quad2.V3, quad2.V4 };
            var uniqueVertices = GetUniqueVerticesStatic(allVertices);

            // For a valid rectangular merge, we should have exactly 6 unique vertices
            // (8 original - 2 shared vertices from the common edge)
            if (uniqueVertices.Count != 6)
                return false;

            // Check if the 6 vertices can form a rectangle in the appropriate plane
            return CanVerticesFormRectangle(uniqueVertices, quad1.FaceDirection);
        }

        /// <summary>
        /// Gets unique vertices from an array, removing duplicates within tolerance.
        /// </summary>
        private static List<Vertex> GetUniqueVerticesStatic(Vertex[] vertices, float tolerance = 0.001f)
        {
            var uniqueVertices = new List<Vertex>();

            foreach (var vertex in vertices)
            {
                bool isDuplicate = false;
                foreach (var existing in uniqueVertices)
                {
                    if (Math.Abs(vertex.X - existing.X) < tolerance &&
                        Math.Abs(vertex.Y - existing.Y) < tolerance &&
                        Math.Abs(vertex.Z - existing.Z) < tolerance)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    uniqueVertices.Add(vertex);
                }
            }

            return uniqueVertices;
        }

        /// <summary>
        /// Checks if a set of vertices can form a rectangle in the given plane.
        /// </summary>
        private static bool CanVerticesFormRectangle(List<Vertex> vertices, FaceDirection faceDirection)
        {
            if (vertices.Count != 6)
                return false;

            // Extract the 2D coordinates based on face direction
            var coords2D = vertices.Select(v => Get2DCoordinates(v, faceDirection)).ToList();

            // Find the bounding box
            var minX = coords2D.Min(c => c.X);
            var maxX = coords2D.Max(c => c.X);
            var minY = coords2D.Min(c => c.Y);
            var maxY = coords2D.Max(c => c.Y);

            // For a valid rectangle merge, we should have exactly 6 points:
            // 4 corners + 2 intermediate points on opposite edges
            var expectedPoints = new[]
            {
                (minX, minY), (maxX, minY), (maxX, maxY), (minX, maxY) // 4 corners
            };

            // Check if all 4 corners exist
            const float tolerance = 0.001f;
            foreach (var expected in expectedPoints)
            {
                bool found = coords2D.Any(c => 
                    Math.Abs(c.X - expected.Item1) < tolerance && 
                    Math.Abs(c.Y - expected.Item2) < tolerance);
                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts 2D coordinates from a vertex based on the face direction.
        /// </summary>
        private static (float X, float Y) Get2DCoordinates(Vertex vertex, FaceDirection faceDirection)
        {
            return faceDirection switch
            {
                FaceDirection.Top or FaceDirection.Bottom => (vertex.X, vertex.Y),
                FaceDirection.Front or FaceDirection.Back => (vertex.X, vertex.Z),
                FaceDirection.Left or FaceDirection.Right => (vertex.Y, vertex.Z),
                _ => (vertex.X, vertex.Y)
            };
        }

        /// <summary>
        /// Merges two quads into a rectangular quad using proper geometric calculations.
        /// </summary>
        private static Quad? MergeQuadsRectangular(Quad quad1, Quad quad2)
        {
            if (!CanMergeQuadsSafely(quad1, quad2))
                return null;

            // Get all unique vertices
            var allVertices = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4, quad2.V1, quad2.V2, quad2.V3, quad2.V4 };
            var uniqueVertices = GetUniqueVerticesStatic(allVertices);

            if (uniqueVertices.Count != 6)
                return null;

            // Find the 4 corner vertices of the merged rectangle
            var cornerVertices = FindRectangleCorners(uniqueVertices, quad1.FaceDirection);
            if (cornerVertices == null)
                return null;

            return new Quad(cornerVertices[0], cornerVertices[1], cornerVertices[2], cornerVertices[3], 
                          quad1.PaintColor, quad1.FaceDirection);
        }

        /// <summary>
        /// Finds the 4 corner vertices that form the merged rectangle.
        /// </summary>
        private static Vertex[]? FindRectangleCorners(List<Vertex> vertices, FaceDirection faceDirection)
        {
            if (vertices.Count != 6)
                return null;

            // Extract 2D coordinates and find bounds
            var coords2D = vertices.Select(v => new { Vertex = v, Coord2D = Get2DCoordinates(v, faceDirection) }).ToList();

            var minX = coords2D.Min(c => c.Coord2D.X);
            var maxX = coords2D.Max(c => c.Coord2D.X);
            var minY = coords2D.Min(c => c.Coord2D.Y);
            var maxY = coords2D.Max(c => c.Coord2D.Y);

            // Find vertices at the 4 corners
            const float tolerance = 0.001f;
            var corners = new Vertex[4];

            // Bottom-left (minX, minY)
            corners[0] = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord2D.X - minX) < tolerance && 
                Math.Abs(c.Coord2D.Y - minY) < tolerance)?.Vertex;

            // Bottom-right (maxX, minY)  
            corners[1] = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord2D.X - maxX) < tolerance && 
                Math.Abs(c.Coord2D.Y - minY) < tolerance)?.Vertex;

            // Top-right (maxX, maxY)
            corners[2] = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord2D.X - maxX) < tolerance && 
                Math.Abs(c.Coord2D.Y - maxY) < tolerance)?.Vertex;

            // Top-left (minX, maxY)
            corners[3] = coords2D.FirstOrDefault(c => 
                Math.Abs(c.Coord2D.X - minX) < tolerance && 
                Math.Abs(c.Coord2D.Y - maxY) < tolerance)?.Vertex;

            // Check if all corners were found
            if (corners.Any(c => c == null))
                return null;

            return corners;
        }
    }
}
