using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeveMazeGenerator.Structures
{
    public class KruskalCell
    {
        public int Y { get; set; }
        public int X { get; set; }
        public bool Solid { get; set; }
        public List<KruskalCell> CellSet { get; set; } = new List<KruskalCell>();

        public KruskalCell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public String xeny()
        {
            return $"{X}-{Y}";
        }
    }
}
