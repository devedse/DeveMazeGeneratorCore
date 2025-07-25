using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Mazes
{
    public record MazeWithPath(InnerMap InnerMap, List<MazePointPos> Path);
}
