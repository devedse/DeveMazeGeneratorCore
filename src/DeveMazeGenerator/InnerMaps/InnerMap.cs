using System;
using System.Text;

namespace DeveMazeGenerator.InnerMaps
{
    public abstract class InnerMap
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public InnerMap(int width, int height)
        {
            Width = width;
            Height = height;
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

        /// <summary>
        /// Info about mazes:
        /// 0 = False = Wall = Black
        /// 1 = True = Empty = White
        /// </summary>

        //abstract must be overidden
        public abstract Boolean this[int x, int y] { get; set; }
    }
}
