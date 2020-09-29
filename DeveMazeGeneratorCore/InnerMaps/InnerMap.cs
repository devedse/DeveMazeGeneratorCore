using System;
using System.Text;

namespace DeveMazeGeneratorCore.InnerMaps
{
    /// <summary>
    /// Info about mazes:
    /// 0 = False = Wall = Black
    /// 1 = True = Empty = White
    /// </summary>
    public abstract class InnerMap
    {
        /// <summary>
        /// This data can be used by some algorithms to also generate path data
        /// </summary>
        public InnerMap PathData { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int StartX { get; set; }
        public int StartY { get; set; }

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

        /// <summary>
        /// Clones the map into either an instance of BitArreintjeFastInnerMap or 
        /// an exact copy of itself (if this is implemented by the child class)
        /// </summary>
        /// <returns>Cloned inner map</returns>
        public virtual InnerMap Clone()
        {
            var innerMapTarget = new BitArreintjeFastInnerMap(Width, Height);
            CloneInto(innerMapTarget);
            return innerMapTarget;
        }

        /// <summary>
        /// This method makes a copy of the maze.
        /// </summary>
        /// <param name="mapTarget">The map to clone into</param>
        /// <returns>The cloned maze</returns>
        public void CloneInto(InnerMap mapTarget)
        {
            if (Width != mapTarget.Width) throw new ArgumentException($"Width of the target ({mapTarget.Width}) is not equal to that of the source ({Width}).");
            if (Height != mapTarget.Height) throw new ArgumentException($"Height of the target ({mapTarget.Height}) is not equal to that of the source ({Height}).");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    mapTarget[x, y] = this[x, y];
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
        public abstract bool this[int x, int y] { get; set; }
    }
}
