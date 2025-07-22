namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public record FastQuadIdentifier(Vertex V1, Vertex V2, Vertex V3, Vertex V4);

    public record Quad(Vertex V1, Vertex V2, Vertex V3, Vertex V4, string PaintColor, FaceDirection FaceDirection)
    {
        public Vertex[] Vertices => [V1, V2, V3, V4];

        public QuadDirection QuadDirection => Vertices.Select(t => t.X).Distinct().Count() == 1 ? QuadDirection.Vertical :
                                              Vertices.Select(t => t.Y).Distinct().Count() == 1 ? QuadDirection.Horizontal :
                                              QuadDirection.Flat;

        public FastQuadIdentifier FastQuadIdentifier()
        {
            var vertices = GetCanonicallyOrderedVertices();
            return new FastQuadIdentifier(vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        /// <summary>
        /// Orders the 4 vertices in a canonical order for cuboids based on face direction:
        /// For each face, finds the 2 dominant axes and orders vertices as:
        /// [0] = min axis1, min axis2  (0,0)
        /// [1] = min axis1, max axis2  (0,1) 
        /// [2] = max axis1, min axis2  (1,0)
        /// [3] = max axis1, max axis2  (1,1)
        /// This creates a consistent 2x2 grid pattern regardless of input order
        /// </summary>
        public Vertex[] GetCanonicallyOrderedVertices()
        {
            var vertices = Vertices;
            
            return FaceDirection switch
            {
                // Top/Bottom faces: use X and Y coordinates (Z is constant)
                FaceDirection.Top or FaceDirection.Bottom => GetOrderedByXY(vertices),
                
                // Front/Back faces: use X and Z coordinates (Y is constant)  
                FaceDirection.Front or FaceDirection.Back => GetOrderedByXZ(vertices),
                
                // Left/Right faces: use Y and Z coordinates (X is constant)
                FaceDirection.Left or FaceDirection.Right => GetOrderedByYZ(vertices),
                
                // Default: use X and Y
                _ => GetOrderedByXY(vertices)
            };
        }
        
        private Vertex[] GetOrderedByXY(Vertex[] vertices)
        {
            var minX = vertices.Min(v => v.X);
            var maxX = vertices.Max(v => v.X);
            var minY = vertices.Min(v => v.Y);
            var maxY = vertices.Max(v => v.Y);
            
            var v00 = vertices.First(v => Math.Abs(v.X - minX) < 0.001f && Math.Abs(v.Y - minY) < 0.001f); // (minX, minY)
            var v01 = vertices.First(v => Math.Abs(v.X - minX) < 0.001f && Math.Abs(v.Y - maxY) < 0.001f); // (minX, maxY)
            var v10 = vertices.First(v => Math.Abs(v.X - maxX) < 0.001f && Math.Abs(v.Y - minY) < 0.001f); // (maxX, minY)
            var v11 = vertices.First(v => Math.Abs(v.X - maxX) < 0.001f && Math.Abs(v.Y - maxY) < 0.001f); // (maxX, maxY)
            
            return [v00, v01, v10, v11];
        }
        
        private Vertex[] GetOrderedByXZ(Vertex[] vertices)
        {
            var minX = vertices.Min(v => v.X);
            var maxX = vertices.Max(v => v.X);
            var minZ = vertices.Min(v => v.Z);
            var maxZ = vertices.Max(v => v.Z);
            
            var v00 = vertices.First(v => Math.Abs(v.X - minX) < 0.001f && Math.Abs(v.Z - minZ) < 0.001f); // (minX, minZ)
            var v01 = vertices.First(v => Math.Abs(v.X - minX) < 0.001f && Math.Abs(v.Z - maxZ) < 0.001f); // (minX, maxZ)
            var v10 = vertices.First(v => Math.Abs(v.X - maxX) < 0.001f && Math.Abs(v.Z - minZ) < 0.001f); // (maxX, minZ)
            var v11 = vertices.First(v => Math.Abs(v.X - maxX) < 0.001f && Math.Abs(v.Z - maxZ) < 0.001f); // (maxX, maxZ)
            
            return [v00, v01, v10, v11];
        }
        
        private Vertex[] GetOrderedByYZ(Vertex[] vertices)
        {
            var minY = vertices.Min(v => v.Y);
            var maxY = vertices.Max(v => v.Y);
            var minZ = vertices.Min(v => v.Z);
            var maxZ = vertices.Max(v => v.Z);
            
            var v00 = vertices.First(v => Math.Abs(v.Y - minY) < 0.001f && Math.Abs(v.Z - minZ) < 0.001f); // (minY, minZ)
            var v01 = vertices.First(v => Math.Abs(v.Y - minY) < 0.001f && Math.Abs(v.Z - maxZ) < 0.001f); // (minY, maxZ)
            var v10 = vertices.First(v => Math.Abs(v.Y - maxY) < 0.001f && Math.Abs(v.Z - minZ) < 0.001f); // (maxY, minZ)
            var v11 = vertices.First(v => Math.Abs(v.Y - maxY) < 0.001f && Math.Abs(v.Z - maxZ) < 0.001f); // (maxY, maxZ)
            
            return [v00, v01, v10, v11];
        }

        /// <summary>
        /// Returns vertices in correct winding order for outward-facing normal (counter-clockwise when viewed from outside)
        /// Uses canonical vertex ordering and applies face-specific transformations
        /// </summary>
        public Vertex[] GetOrderedVertices()
        {
            var canonical = GetCanonicallyOrderedVertices();
            // canonical[0] = (0,0), canonical[1] = (0,1), canonical[2] = (1,0), canonical[3] = (1,1)
            
            return FaceDirection switch
            {
                // Top face: counter-clockwise when viewed from above (outside)
                // (0,0) -> (1,0) -> (1,1) -> (0,1)
                FaceDirection.Top => [canonical[0], canonical[2], canonical[3], canonical[1]],
                
                // Bottom face: counter-clockwise when viewed from below (outside) 
                // (0,0) -> (0,1) -> (1,1) -> (1,0)
                FaceDirection.Bottom => [canonical[0], canonical[1], canonical[3], canonical[2]],
                
                // Front face (-Y): counter-clockwise when viewed from front
                // For front face, Y is constant, so we use X and Z coordinates
                // This needs to be adjusted based on how front faces are actually created
                FaceDirection.Front => [canonical[0], canonical[2], canonical[3], canonical[1]],
                
                // Back face (+Y): counter-clockwise when viewed from back  
                FaceDirection.Back => [canonical[0], canonical[1], canonical[3], canonical[2]],
                
                // Left face (-X): counter-clockwise when viewed from left
                FaceDirection.Left => [canonical[0], canonical[1], canonical[3], canonical[2]],
                
                // Right face (+X): counter-clockwise when viewed from right
                FaceDirection.Right => [canonical[0], canonical[2], canonical[3], canonical[1]],
                
                // Default: use canonical order
                _ => canonical
            };
        }

        /// <summary>
        /// Checks if this quad is adjacent to another quad (sharing exactly 2 vertices - an edge).
        /// This is more precise than SharesEdgeWith which checks for overlapping edges.
        /// </summary>
        public bool IsAdjacentTo(Quad other, float tolerance = 0.001f)
        {
            var vertices1 = Vertices;
            var vertices2 = other.Vertices;
            
            // Count shared vertices (adjacent quads should share exactly 2 vertices - an edge)
            int sharedVertices = 0;
            foreach (var v1 in vertices1)
            {
                foreach (var v2 in vertices2)
                {
                    if (Math.Abs(v1.X - v2.X) < tolerance &&
                        Math.Abs(v1.Y - v2.Y) < tolerance &&
                        Math.Abs(v1.Z - v2.Z) < tolerance)
                    {
                        sharedVertices++;
                        break;
                    }
                }
            }
            
            // Adjacent quads share exactly 2 vertices (an edge)
            return sharedVertices == 2;
        }



        /// <summary>
        /// Checks if this quad is facing another quad (same position, opposite directions).
        /// Used for face culling optimization.
        /// </summary>
        public bool IsFacing(Quad other)
        {
            // Must be opposite face directions
            if (!AreOppositeFaceDirections(FaceDirection, other.FaceDirection)) 
                return false;

            // Check if quads are coplanar and overlapping
            return IsCoplanarAndOverlappingWith(other);
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
        /// Checks if this quad is coplanar and overlapping with another quad (occupy the same space).
        /// </summary>
        private bool IsCoplanarAndOverlappingWith(Quad other, float tolerance = 0.001f)
        {
            var vertices1 = Vertices;
            var vertices2 = other.Vertices;

            // For each vertex in this quad, check if there's a matching vertex in the other quad
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

        public override string ToString()
        {
            return $"{QuadDirection} {FaceDirection} Quad: {V1}, {V2}, {V3}, {V4}, Color: {PaintColor}";
        }
    }

    // Helper struct for vector operations
    public struct Vector3
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}