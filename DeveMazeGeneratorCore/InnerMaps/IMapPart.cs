namespace DeveMazeGeneratorCore.InnerMaps
{
    public interface IMapPart
    {
        public int Width { get; }
        public int Height { get; }

        public int StartX { get; }
        public int StartY { get; }
    }
}
