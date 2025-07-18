namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public record Quad(Vertex V1, Vertex V2, Vertex V3, Vertex V4, string PaintColor)
    {
        public Vertex[] Vertices => [V1, V2, V3, V4];

        public QuadDirection QuadDirection => Vertices.Select(t => t.X).Distinct().Count() == 1 ? QuadDirection.Vertical :
                                              Vertices.Select(t => t.Y).Distinct().Count() == 1 ? QuadDirection.Horizontal :
                                              QuadDirection.Flat;

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
                   IsTouchingWith(other, tolerance) &&
                   PaintColor == other.PaintColor;
        }

        public override string ToString()
        {
            return $"{QuadDirection} Quad: {V1}, {V2}, {V3}, {V4}, Color: {PaintColor}";
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