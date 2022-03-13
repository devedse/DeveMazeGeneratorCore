using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Mazes
{
    public class Maze
    {
        public InnerMap InnerMap { get; }

        public Maze(InnerMap innerMap)
        {
            InnerMap = innerMap;
        }
    }
}
