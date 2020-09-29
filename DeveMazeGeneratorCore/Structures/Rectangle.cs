namespace DeveMazeGeneratorCore.Structures
{
    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Seed { get; set; }

        public Rectangle(int x, int y, int width, int height, int seed)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Seed = seed;
        }

        public bool IntersectsWith(Rectangle rect)
        {
            return rect.X < X + Width &&
            X < rect.X + rect.Width &&
            rect.Y < Y + Height &&
            Y < rect.Y + rect.Height;
        }

        public bool IntersectsWith(MazePoint point)
        {
            if (point.X >= X && point.Y >= Y && point.X < X + Width && point.Y < Y + Height)
            {
                return true;
            }
            return false;
        }

        public bool IntersectsWith(MazePointClass point)
        {
            if (point == null)
            {
                return false;
            }
            if (point.X >= X && point.Y >= Y && point.X < X + Width && point.Y < Y + Height)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"Rectangle(X: {X} Y: {Y} Width: {Width} Height: {Height})";
        }
    }
}
