using System;

namespace DeveMazeGeneratorCore.MonoGame.Core.HelperObjects
{
    public struct IntSize : IEquatable<IntSize>
    {
        public int Width { get; }
        public int Height { get; }
        public IntSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            return obj is IntSize size && Equals(size);
        }

        public bool Equals(IntSize other)
        {
            return Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hashCode = 859600377;
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(IntSize left, IntSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntSize left, IntSize right)
        {
            return !(left == right);
        }
    }
}
