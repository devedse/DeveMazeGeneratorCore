using System;
using System.Collections.Generic;

namespace DeveMazeGenerator.Generators
{
    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Seed { get; set; }

        public Rectangle(int x, int y, int width, int height, int seed)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Seed = seed;
        }

        public bool IntersectsWith(Rectangle rect)
        {
            return (rect.X < this.X + this.Width) &&
            (this.X < (rect.X + rect.Width)) &&
            (rect.Y < this.Y + this.Height) &&
            (this.Y < rect.Y + rect.Height);
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y} Width: {Width} Height: {Height} Seed: {Seed}";
        }
    }
}
