namespace DeveMazeGeneratorCore.InnerMaps
{
    public interface IMapPart
    {
        int Width { get; }
        int Height { get; }

        int StartX { get; }
        int StartY { get; }
    }
}
