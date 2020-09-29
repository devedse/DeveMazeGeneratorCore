using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Structures
{
    public class KruskalCell
    {
        public int Y { get; set; }
        public int X { get; set; }
        public bool Solid { get; set; }
        public List<KruskalCell> CellSet { get; set; } = new List<KruskalCell>();

        public KruskalCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public string xeny()
        {
            return $"{X}-{Y}";
        }
    }
}
