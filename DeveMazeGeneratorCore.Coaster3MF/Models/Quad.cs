namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public record Quad(Vertex V1, Vertex V2, Vertex V3, Vertex V4, string PaintColor, FaceDirection FaceDirection)
    {
        public Vertex[] Vertices => [V1, V2, V3, V4];

        public QuadDirection QuadDirection => Vertices.Select(t => t.X).Distinct().Count() == 1 ? QuadDirection.Vertical :
                                              Vertices.Select(t => t.Y).Distinct().Count() == 1 ? QuadDirection.Horizontal :
                                              QuadDirection.Flat;

        /// <summary>
        /// Orders the 4 vertices in a canonical order for cuboids based on face direction:
        /// For each face, finds the 2 dominant axes and orders vertices as:
        /// [0] = min axis1, min axis2  (0,0)
        /// [1] = min axis1, max axis2  (0,1) 
        /// [2] = max axis1, min axis2  (1,0)
        /// [3] = max axis1, max axis2  (1,1)
        /// This creates a consistent 2x2 grid pattern regardless of input order
        /// </summary>
        private Vertex[] GetCanonicallyOrderedVertices()
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

        public bool HasOverlappingVerticesWith(Quad other)
        {
            return Vertices.Any(v => other.Vertices.Contains(v));
        }

        public bool SharesEdgeWith(Quad other, double tolerance = 0.0001)
        {
            var thisEdges = GetEdges();
            var otherEdges = other.GetEdges();

            foreach (var edge1 in thisEdges)
            {
                foreach (var edge2 in otherEdges)
                {
                    if (EdgesOverlap(edge1, edge2, tolerance))
                        return true;
                }
            }

            return false;
        }

        private (Vertex, Vertex)[] GetEdges()
        {
            return new[]
            {
                (V1, V2),
                (V2, V3),
                (V3, V4),
                (V4, V1)
            };
        }

        private bool EdgesOverlap((Vertex, Vertex) edge1, (Vertex, Vertex) edge2, double tolerance)
        {
            // First check if edges are collinear
            if (!AreEdgesCollinear(edge1, edge2, tolerance))
                return false;

            // If collinear, check if they overlap
            return DoCollinearSegmentsOverlap(edge1, edge2, tolerance);
        }

        private bool AreEdgesCollinear((Vertex, Vertex) edge1, (Vertex, Vertex) edge2, double tolerance)
        {
            // Calculate direction vectors
            var dir1 = new Vector3(
                edge1.Item2.X - edge1.Item1.X,
                edge1.Item2.Y - edge1.Item1.Y,
                edge1.Item2.Z - edge1.Item1.Z
            );

            var dir2 = new Vector3(
                edge2.Item2.X - edge2.Item1.X,
                edge2.Item2.Y - edge2.Item1.Y,
                edge2.Item2.Z - edge2.Item1.Z
            );

            // Check if one edge point lies on the line defined by the other edge
            var crossProduct = CrossProduct(dir1,
                new Vector3(
                    edge2.Item1.X - edge1.Item1.X,
                    edge2.Item1.Y - edge1.Item1.Y,
                    edge2.Item1.Z - edge1.Item1.Z
                ));

            return crossProduct.Length() < tolerance;
        }

        private bool DoCollinearSegmentsOverlap((Vertex, Vertex) seg1, (Vertex, Vertex) seg2, double tolerance)
        {
            // For collinear segments, project onto the dominant axis
            var dx1 = Math.Abs(seg1.Item2.X - seg1.Item1.X);
            var dy1 = Math.Abs(seg1.Item2.Y - seg1.Item1.Y);
            var dz1 = Math.Abs(seg1.Item2.Z - seg1.Item1.Z);

            // Determine dominant axis
            if (dx1 >= dy1 && dx1 >= dz1)
            {
                // Project onto X axis
                return SegmentsOverlap1D(
                    seg1.Item1.X, seg1.Item2.X,
                    seg2.Item1.X, seg2.Item2.X,
                    tolerance);
            }
            else if (dy1 >= dx1 && dy1 >= dz1)
            {
                // Project onto Y axis
                return SegmentsOverlap1D(
                    seg1.Item1.Y, seg1.Item2.Y,
                    seg2.Item1.Y, seg2.Item2.Y,
                    tolerance);
            }
            else
            {
                // Project onto Z axis
                return SegmentsOverlap1D(
                    seg1.Item1.Z, seg1.Item2.Z,
                    seg2.Item1.Z, seg2.Item2.Z,
                    tolerance);
            }
        }

        private bool SegmentsOverlap1D(double a1, double a2, double b1, double b2, double tolerance)
        {
            // Ensure segments are ordered
            var min1 = Math.Min(a1, a2);
            var max1 = Math.Max(a1, a2);
            var min2 = Math.Min(b1, b2);
            var max2 = Math.Max(b1, b2);

            // Check if segments overlap
            return max1 + tolerance >= min2 && max2 + tolerance >= min1;
        }

        private Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public bool IsTouchingWith(Quad other, double tolerance = 0.0001)
        {
            return HasOverlappingVerticesWith(other) || SharesEdgeWith(other, tolerance);
        }

        public bool IsMergableWith(Quad other, double tolerance = 0.0001)
        {
            return QuadDirection == other.QuadDirection &&
                   FaceDirection == other.FaceDirection &&
                   IsTouchingWith(other, tolerance) &&
                   PaintColor == other.PaintColor;
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