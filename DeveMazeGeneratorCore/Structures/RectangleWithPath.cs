namespace DeveMazeGeneratorCore.Structures
{
    public class RectangleWithPath : Rectangle
    {

        public MazePointClassLinkedList MazePointLeft { get; set; }
        public MazePointClassLinkedList MazePointRight { get; set; }
        public bool PathPassesThroughThis { get; set; }

        public RectangleWithPath(int x, int y, int width, int height, int seed)
            : base(x, y, width, height, seed)
        {
        }

        public RectangleWithPath(int x, int y, int width, int height, int seed, MazePointClassLinkedList left, MazePointClassLinkedList right, bool pathPassesThroughThis)
            : base(x, y, width, height, seed)
        {
            MazePointLeft = left;
            MazePointRight = right;
            PathPassesThroughThis = pathPassesThroughThis;

            left.Next = right;
            right.Previous = left;
        }

        public override string ToString()
        {
            return $"RectangleWithPath(X: {X} Y: {Y} Width: {Width} Height: {Height}, MazePointLeft: {MazePointLeft}, MazePointRight: {MazePointRight}, PathPassesThroughThis: {PathPassesThroughThis})";
        }
    }
}
