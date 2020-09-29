namespace DeveMazeGeneratorCore.Helpers
{
    public static class UnevenHelper
    {
        /// <summary>
        /// Returns if a number is even or not
        /// </summary>
        /// <param name="number">The input number</param>
        /// <returns>True if even, false if uneven</returns>
        public static bool NumberIsEven(int number)
        {
            return number % 2 == 0;
        }

        /// <summary>
        /// Makes a number uneven. E.g. if 32 is inputted it will return 31.
        /// If 31 is inputted it will return 31.
        /// </summary>
        /// <param name="number">The input number</param>
        /// <returns>The first uneven number lower then this</returns>
        public static int MakeUneven(int number)
        {
            if (NumberIsEven(number))
            {
                return number - 1;
            }
            return number;
        }
    }
}
