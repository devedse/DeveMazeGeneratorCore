using System;
using System.Text;

namespace DeveMazeGenerator.InnerMaps
{
    /// <summary>
    /// Info about mazes:
    /// 0 = False = Wall = Black
    /// 1 = True = Empty = White
    /// </summary>
    public abstract class InnerMap
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public InnerMap(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Fills the map cell by cell.
        /// This method is really slow and should be overidden where possible
        /// </summary>
        /// <param name="state">false = 0 = Wall, true = 1 = Empty</param>
        public virtual void FillMap(bool state)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    this[x, y] = state;
                }
            }
        }

        //virtual can be overidden
        public virtual string GenerateMapAsString()
        {
            var stringBuilder = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bool b = this[x, y];
                    if (b)
                    {
                        stringBuilder.Append(' ');
                    }
                    else
                    {
                        stringBuilder.Append('0');
                    }
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        //abstract must be overidden
        public abstract Boolean this[int x, int y] { get; set; }
    }
}
