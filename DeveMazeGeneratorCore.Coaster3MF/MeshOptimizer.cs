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

                    // Check if these two quads are facing each other and can be culled
                    if (AreQuadsFacingEachOther(quad1, quad2))
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
        /// </summary>
        public static void OptimizeQuads(List<Quad> quads)
        {
            Console.WriteLine($"Found {quads.Count} wall quads before optimization.");

            bool quit = false;

            int cur = 0;
            while (cur < quads.Count)
            {
                var currentQuad = quads[cur];
                var quadsThatTouchThisQuad = quads.Where(t => t != currentQuad && t.IsMergableWith(currentQuad)).ToList();

                if (currentQuad.QuadDirection == QuadDirection.Flat)
                {
                    cur++;
                    continue;
                }

                if (quadsThatTouchThisQuad.Any())
                {
                    var allQuads = new List<Quad> { currentQuad }.Concat(quadsThatTouchThisQuad).ToList();

                    Vertex mergedV1, mergedV2, mergedV3, mergedV4;

                    var allVertices = allQuads.SelectMany(t => t.Vertices).ToList();

                    if (currentQuad.QuadDirection == QuadDirection.Vertical)
                    {
                        // Merge vertically aligned quads
                        mergedV1 = new Vertex(
                            currentQuad.V1.X,
                            allVertices.Min(t => t.Y),
                            allVertices.Min(t => t.Z));
                        mergedV2 = new Vertex(
                            currentQuad.V2.X,
                            allVertices.Max(t => t.Y),
                            allVertices.Min(t => t.Z));
                        mergedV3 = new Vertex(
                            currentQuad.V3.X,
                            allVertices.Max(t => t.Y),
                            allVertices.Max(t => t.Z));
                        mergedV4 = new Vertex(
                            currentQuad.V4.X,
                            allVertices.Min(t => t.Y),
                            allVertices.Max(t => t.Z));
                    }
                    else if (currentQuad.QuadDirection == QuadDirection.Horizontal)
                    {
                        // Merge horizontally aligned quads
                        mergedV1 = new Vertex(
                            allVertices.Min(t => t.X),
                            currentQuad.V1.Y,
                            allVertices.Min(t => t.Z));
                        mergedV2 = new Vertex(
                            allVertices.Max(t => t.X),
                            currentQuad.V2.Y,
                            allVertices.Min(t => t.Z));
                        mergedV3 = new Vertex(
                            allVertices.Max(t => t.X),
                            currentQuad.V3.Y,
                            allVertices.Max(t => t.Z));
                        mergedV4 = new Vertex(
                            allVertices.Min(t => t.X),
                            currentQuad.V4.Y,
                            allVertices.Max(t => t.Z));
                    }
                    else //flat
                    {
                        // Merge flat quads
                        mergedV1 = new Vertex(
                            allVertices.Min(t => t.X),
                            allVertices.Min(t => t.Y),
                            allVertices.Min(t => t.Z));
                        mergedV2 = new Vertex(
                            allVertices.Max(t => t.X),
                            allVertices.Min(t => t.Y),
                            allVertices.Min(t => t.Z));
                        mergedV3 = new Vertex(
                            allVertices.Max(t => t.X),
                            allVertices.Max(t => t.Y),
                            allVertices.Max(t => t.Z));
                        mergedV4 = new Vertex(
                            allVertices.Min(t => t.X),
                            allVertices.Max(t => t.Y),
                            allVertices.Max(t => t.Z));
                    }

                    //Merge overlapping quads
                    var mergedQuad = new Quad(
                        mergedV1,
                        mergedV2,
                        mergedV3,
                        mergedV4,
                        currentQuad.PaintColor,
                        currentQuad.FaceDirection
                        );

                    //Remove all quads that were merged
                    quads.RemoveAll(t => t == currentQuad || quadsThatTouchThisQuad.Contains(t));
                    //Add the merged quad
                    quads.Add(mergedQuad);

                }
                else
                {
                    cur++;
                }

                if (quit)
                {
                    break;
                }
            }

            Console.WriteLine($"Found {quads.Count} wall quads after optimization.");
        }

        /// <summary>
        /// Checks if two quads are facing each other (same position, opposite directions).
        /// </summary>
        private static bool AreQuadsFacingEachOther(Quad quad1, Quad quad2)
        {
            // Must be opposite face directions
            if (!AreOppositeFaceDirections(quad1.FaceDirection, quad2.FaceDirection)) return false;

            // Check if quads are coplanar and overlapping
            return AreQuadsCoplanarAndOverlapping(quad1, quad2);
        }

        /// <summary>
        /// Checks if two face directions are opposite.
        /// </summary>
        private static bool AreOppositeFaceDirections(FaceDirection dir1, FaceDirection dir2)
        {
            return (dir1 == FaceDirection.Front && dir2 == FaceDirection.Back) ||
                   (dir1 == FaceDirection.Back && dir2 == FaceDirection.Front) ||
                   (dir1 == FaceDirection.Left && dir2 == FaceDirection.Right) ||
                   (dir1 == FaceDirection.Right && dir2 == FaceDirection.Left) ||
                   (dir1 == FaceDirection.Top && dir2 == FaceDirection.Bottom) ||
                   (dir1 == FaceDirection.Bottom && dir2 == FaceDirection.Top);
        }

        /// <summary>
        /// Checks if two quads are coplanar and overlapping (occupy the same space).
        /// </summary>
        private static bool AreQuadsCoplanarAndOverlapping(Quad quad1, Quad quad2)
        {
            const float tolerance = 0.001f;

            // Get all vertices from both quads
            var vertices1 = new[] { quad1.V1, quad1.V2, quad1.V3, quad1.V4 };
            var vertices2 = new[] { quad2.V1, quad2.V2, quad2.V3, quad2.V4 };

            // For each vertex in quad1, check if there's a matching vertex in quad2
            int matchingVertices = 0;
            foreach (var v1 in vertices1)
            {
                foreach (var v2 in vertices2)
                {
                    if (Math.Abs(v1.X - v2.X) < tolerance &&
                        Math.Abs(v1.Y - v2.Y) < tolerance &&
                        Math.Abs(v1.Z - v2.Z) < tolerance)
                    {
                        matchingVertices++;
                        break;
                    }
                }
            }

            // If all 4 vertices match, the quads are coplanar and overlapping
            return matchingVertices == 4;
        }
    }
}
