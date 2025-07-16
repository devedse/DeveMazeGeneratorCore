namespace DeveMazeGeneratorCore.Structures
{
    public class MazeWall
    {
        public int Xstart { get; }
        public int Ystart { get; }
        public int Xend { get; }
        public int Yend { get; }

        public MazeWall(int xstart, int ystart, int xend, int yend)
        {
            Xstart = xstart;
            Ystart = ystart;
            Xend = xend;
            Yend = yend;
        }

        public override string ToString()
        {
            return $"Wall from ({Xstart}, {Ystart}) to ({Xend}, {Yend})";
        }
    }
}
