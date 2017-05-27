namespace DeveMazeGenerator.Structures
{
    /// <summary>
    /// Contains a position.
    /// Note: Struct really is faster then class so use struct or class depending on what you need
    /// </summary>
    public class MazePointClass
    {
        public int X { get; set; }
        public int Y { get; set; }

        public MazePointClass(int X, int Y)
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