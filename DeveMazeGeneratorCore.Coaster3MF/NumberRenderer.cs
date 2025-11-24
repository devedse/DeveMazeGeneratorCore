namespace DeveMazeGeneratorCore.Coaster3MF
{
    /// <summary>
    /// Renders numbers as a sequence of 3x5 block patterns centered in the maze.
    /// Each digit is separated by a 1-block gap, with an underline below to indicate orientation.
    /// </summary>
    public class NumberRenderer
    {
        private const int DigitWidth = 3;
        private const int DigitHeight = 5;
        private const int DigitSpacing = 1; // Gap between digits
        private const int UnderlineGap = 1; // Gap between number and underline
        private const int UnderlineHeight = 1; // Height of the underline

        /// <summary>
        /// Calculates the total width needed to render a number (including spacing).
        /// </summary>
        public static int GetNumberWidth(int number)
        {
            var digits = GetDigits(number);
            if (digits.Count == 0) return 0;
            
            // Width = (number of digits × digit width) + (gaps between digits)
            return (digits.Count * DigitWidth) + ((digits.Count - 1) * DigitSpacing);
        }

        /// <summary>
        /// Renders a number as blocks centered in the maze.
        /// Returns a 2D array representing the blocks pattern, where true = white block.
        /// The pattern is centered both horizontally and vertically, with an underline below.
        /// </summary>
        /// <param name="number">The number to render (seed value)</param>
        /// <param name="mazeWidth">The width of the maze in blocks</param>
        /// <param name="mazeHeight">The height of the maze in blocks</param>
        /// <returns>A 2D array [y, x] where true indicates a white block should be placed</returns>
        public static bool[,] RenderNumber(int number, int mazeWidth, int mazeHeight)
        {
            var digits = GetDigits(number);
            if (digits.Count == 0)
            {
                return new bool[0, 0];
            }

            var numberWidth = GetNumberWidth(number);
            var totalHeight = DigitHeight + UnderlineGap + UnderlineHeight;

            // Result covers the entire maze
            var result = new bool[mazeHeight, mazeWidth];

            // Calculate starting positions to center both horizontally and vertically
            // The center point should be for the entire content (digits + gap + underline)
            var startX = Math.Max(0, (mazeWidth - numberWidth) / 2);
            var centerY = mazeHeight / 2;
            
            // Position underline above center, then gap, then digits (since it's mirrored)
            var underlineY = centerY - (totalHeight / 2);
            var startY = underlineY + UnderlineHeight + UnderlineGap;

            // Render each digit
            var currentX = startX;
            foreach (var digit in digits)
            {
                var pattern = DigitPatternGenerator.GetDigitPattern(digit);
                if (pattern == null) continue;

                // Copy the digit pattern into the result array (mirrored vertically since it's on the bottom)
                for (int y = 0; y < DigitHeight; y++)
                {
                    for (int x = 0; x < DigitWidth; x++)
                    {
                        var targetX = currentX + x;
                        // Mirror vertically: bottom row of pattern becomes top row
                        var targetY = startY + (DigitHeight - 1 - y);
                        if (targetX >= 0 && targetX < mazeWidth && targetY >= 0 && targetY < mazeHeight)
                        {
                            result[targetY, targetX] = pattern[y, x];
                        }
                    }
                }

                // Move to next digit position
                currentX += DigitWidth + DigitSpacing;
            }

            // Add underline (already calculated position above)
            if (underlineY >= 0 && underlineY < mazeHeight)
            {
                for (int x = startX; x < startX + numberWidth && x < mazeWidth; x++)
                {
                    result[underlineY, x] = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts individual digits from a number in left-to-right order.
        /// </summary>
        private static List<int> GetDigits(int number)
        {
            // Handle negative numbers by taking absolute value
            number = Math.Abs(number);

            var digits = new List<int>();
            if (number == 0)
            {
                digits.Add(0);
                return digits;
            }

            while (number > 0)
            {
                digits.Insert(0, number % 10);
                number /= 10;
            }

            return digits;
        }

        /// <summary>
        /// Gets the total height of the number pattern including underline (5 + 1 gap + 1 underline = 7).
        /// </summary>
        public static int GetNumberHeight() => DigitHeight + UnderlineGap + UnderlineHeight;
    }
}
