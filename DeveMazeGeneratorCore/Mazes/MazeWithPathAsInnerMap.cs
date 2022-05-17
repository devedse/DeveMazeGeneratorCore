using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Mazes
{
    public class MazeWithPathAsInnerMap : Maze
    {
        public InnerMap PathMap { get; }

        public MazeWithPathAsInnerMap(InnerMap innerMap, InnerMap pathMap) : base(innerMap)
        {
            PathMap = pathMap;
        }
    }
}
