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
        /// </summary>
        public static void CullHiddenFaces(List<Quad> quads)
        {
            Console.WriteLine($"Found {quads.Count} quads before face culling.");

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

            Console.WriteLine($"Found {quads.Count} quads after face culling. Removed {quadsToRemove.Count} hidden faces.");
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
    }
}
