using DeveMazeGeneratorCore.InnerMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
