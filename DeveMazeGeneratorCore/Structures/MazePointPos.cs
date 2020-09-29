using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.Structures
{
    /// <summary>
    /// Contains a position with a byte that describes how far in the maze this point is (to determine the color).
    /// Note: Struct really is faster then class
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)] //This is required so this struct uses 9 bytes instead of 12
    public struct MazePointPos
    {
        public int X, Y;
        public byte RelativePos;

        public MazePointPos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            RelativePos = 0;
        }

        public MazePointPos(int X, int Y, byte RelativePos)
        {
            this.X = X;
            this.Y = Y;
            this.RelativePos = RelativePos;
        }

        public override string ToString()
        {
            return $"MazePointPos(X: {X} Y: {Y} RelativePos: {RelativePos})";
        }
    }
}
