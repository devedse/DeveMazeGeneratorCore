namespace DeveMazeGeneratorCore.Structures
{
    public class MazeWall
    {
        public int xstart;
        public int ystart;
        public int xend;
        public int yend;

        public MazeWall(int xstart, int ystart, int xend, int yend)
        {
            this.xstart = xstart;
            this.ystart = ystart;
            this.xend = xend;
            this.yend = yend;
        }
    }
}
