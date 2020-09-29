namespace DeveMazeGenerator.Structures
{
    /// <summary>
    /// Contains a position.
    /// Note: Struct really is faster then class
    /// </summary>
    public struct MazePoint
    {
        public int X, Y;

        public MazePoint(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public override string ToString()
        {
            return $"MazePoint(X: {X} Y: {Y})";
        }
    }
}
