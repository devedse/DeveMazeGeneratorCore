using DeveMazeGeneratorCore.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeveMazeGeneratorCore.InnerMaps
{
    /// <summary>
    /// Info about mazes:
    /// 0 = False = Wall = Black
    /// 1 = True = Empty = White
    /// </summary>
    public abstract class InnerMap : IMapPart
    {
        ///// <summary>
        ///// This data can be used by some algorithms to also generate path data
        ///// </summary>
        //public InnerMap PathData { get; set; }

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

        public List<MazeWall> GenerateListOfMazeWalls()
        {
            List<MazeWall> walls = new List<MazeWall>();
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    //Horizontal
                    if (this[x, y] == false)
                    {
                        Boolean done = false;
                        int xx = x;
                        while (!done)
                        {
                            if (xx >= Width - 1 || this[xx, y] == true)
                            {
                                AddToWallList(walls, x, y, xx - 1, y);
                                done = true;
                            }
                            xx++;
                        }
                        x = xx - 1;
                    }
                }
            }


            for (int x = 0; x < Width - 1; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    //Vertical
                    if (this[x, y] == false)
                    {

                        Boolean done = false;
                        int yy = y;
                        while (!done)
                        {
                            if (yy >= Height - 1 || this[x, yy] == true)
                            {
                                AddToWallList(walls, x, y, x, yy - 1);
                                done = true;
                            }
                            yy++;
                        }
                        y = yy - 1;
                    }
                }
            }

            return walls;
        }

        private void AddToWallList(List<MazeWall> walls, int xstart, int ystart, int xend, int yend)
        {
            if (xend - xstart <= 1 && yend - ystart <= 1)
            {
                return;
            }

            MazeWall wall = new MazeWall(xstart, ystart, xend, yend);
            walls.Add(wall);

            //Console.WriteLine("New wall found: " + xstart + ":" + ystart + "  " + xend + ":" + yend);
        }

        public void MarkBorderInaccessible()
        {
            int borderWidth = 2; // Hardcoded border width

            // Check if the height and width are even. If so, adjust the border painting accordingly
            bool isHeightEven = Height % 2 == 0;
            bool isWidthEven = Width % 2 == 0;

            int adjustedHeight = isHeightEven ? Height - 2 : Height - 1;
            int adjustedWidth = isWidthEven ? Width - 2 : Width - 1;

            // Mark top and bottom borders
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < borderWidth; y++)
                {
                    this[x, y] = true;                   // Top border
                    this[x, adjustedHeight - y] = true;   // Bottom border
                }
            }

            // Mark left and right borders
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < borderWidth; x++)
                {
                    this[x, y] = true;                  // Left border
                    this[adjustedWidth - x, y] = true;  // Right border
                }
            }
        }

        //abstract must be overidden
        public abstract bool this[int x, int y] { get; set; }
    }
}
