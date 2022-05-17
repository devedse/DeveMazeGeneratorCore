namespace DeveMazeGeneratorCore.InnerMaps
{
    public class InnerMapWithPath<M> : IMapPart where M : InnerMap
    {
        public int Width { get; }
        public int Height { get; }

        public int StartX { get; }
        public int StartY { get; }

        public M Map { get; }
        public M PathMap { get; }

        public InnerMapWithPath(int width, int height, int startX, int startY, M map, M pathMap)
        {
            Width = width;
            Height = height;
            StartX = startX;
            StartY = startY;
            Map = map;
            PathMap = pathMap;
        }
    }
}
