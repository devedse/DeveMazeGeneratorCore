using DeveMazeGeneratorCore.Structures;

namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public class PathData
    {
        public HashSet<MazePointPos> PathSet { get; } = new();
        public Dictionary<MazePointPos, byte> PathPositions { get; } = new();

        public PathData(List<MazePointPos> path)
        {
            foreach (var point in path)
            {
                PathSet.Add(point);
                PathPositions[point] = point.RelativePos;
            }
        }

        public bool Contains(int x, int y)
        {
            return PathSet.Any(p => p.X == x && p.Y == y);
        }

        public byte GetRelativePosition(int x, int y)
        {
            var point = PathSet.FirstOrDefault(p => p.X == x && p.Y == y);
            if (PathPositions.ContainsKey(point))
            {
                return PathPositions[point];
            }
            return 0;
        }
    }
}