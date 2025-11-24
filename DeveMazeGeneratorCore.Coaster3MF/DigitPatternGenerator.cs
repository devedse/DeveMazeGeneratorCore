namespace DeveMazeGeneratorCore.Coaster3MF
{
    /// <summary>
    /// Generates 3x5 block patterns for digits 0-9.
    /// Each pattern is represented as a 2D boolean array where true = block, false = empty.
    /// Patterns are designed to be clear and readable when rendered as blocks.
    /// </summary>
    public static class DigitPatternGenerator
    {
        private const int PatternWidth = 3;
        private const int PatternHeight = 5;

        /// <summary>
        /// Gets the 3x5 pattern for a given digit (0-9).
        /// Returns null if the digit is not in the valid range.
        /// Pattern coordinates: [y, x] where y is row (0=top, 4=bottom) and x is column (0=left, 2=right)
        /// </summary>
        public static bool[,]? GetDigitPattern(int digit)
        {
            return digit switch
            {
                0 => new bool[,]
                {
                    { true, true, true },
                    { true, false, true },
                    { true, false, true },
                    { true, false, true },
                    { true, true, true }
                },
                1 => new bool[,]
                {
                    { false, true, false },
                    { false, true, false },
                    { false, true, false },
                    { false, true, false },
                    { false, true, false }
                },
                2 => new bool[,]
                {
                    { true, true, true },
                    { false, false, true },
                    { true, true, true },
                    { true, false, false },
                    { true, true, true }
                },
                3 => new bool[,]
                {
                    { true, true, true },
                    { false, false, true },
                    { true, true, true },
                    { false, false, true },
                    { true, true, true }
                },
                4 => new bool[,]
                {
                    { true, false, true },
                    { true, false, true },
                    { true, true, true },
                    { false, false, true },
                    { false, false, true }
                },
                5 => new bool[,]
                {
                    { true, true, true },
                    { true, false, false },
                    { true, true, true },
                    { false, false, true },
                    { true, true, true }
                },
                6 => new bool[,]
                {
                    { true, true, true },
                    { true, false, false },
                    { true, true, true },
                    { true, false, true },
                    { true, true, true }
                },
                7 => new bool[,]
                {
                    { true, true, true },
                    { false, false, true },
                    { false, true, false },
                    { false, true, false },
                    { false, true, false }
                },
                8 => new bool[,]
                {
                    { true, true, true },
                    { true, false, true },
                    { true, true, true },
                    { true, false, true },
                    { true, true, true }
                },
                9 => new bool[,]
                {
                    { true, true, true },
                    { true, false, true },
                    { true, true, true },
                    { false, false, true },
                    { true, true, true }
                },
                _ => null
            };
        }

        /// <summary>
        /// Gets the width of digit patterns (always 3).
        /// </summary>
        public static int GetPatternWidth() => PatternWidth;

        /// <summary>
        /// Gets the height of digit patterns (always 5).
        /// </summary>
        public static int GetPatternHeight() => PatternHeight;
    }
}
