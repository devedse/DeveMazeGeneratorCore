namespace DeveMazeGenerator.Generators.SpeedOptimization
{
    public static class MazeDir
    {
        public const uint L = 0b0001;
        public const uint R = 0b0010;
        public const uint U = 0b0100;
        public const uint D = 0b1000;

        public const uint LU = L | U;
        public const uint RU = R | U;
        public const uint LD = L | D;
        public const uint RD = R | D;

        public const uint LUR = L | U | R;
        public const uint URD = U | R | D;
        public const uint RDL = R | D | L;
        public const uint DLU = D | L | U;
    }
}
