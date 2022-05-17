namespace DeveMazeGeneratorCore.Generators.Helpers
{
    public interface IRandom
    {
        /// <summary>
        /// Returns an integer between 0 and int.MaxValue
        /// </summary>
        /// <returns></returns>
        int Next();

        /// <summary>
        /// Returns an integer between 0 and maxValue
        /// </summary>
        /// <param name="maxValue">MaxValue exclusive</param>
        /// <returns></returns>
        int Next(int maxValue);

        /// <summary>
        /// Returns an integer between minValue and maxValue
        /// </summary>
        /// <param name="minValue">MinValue inclusive</param>
        /// <param name="maxValue">MaxValue exclusive</param>
        /// <returns></returns>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Returns a double in between 0 and 1
        /// </summary>
        /// <returns></returns>
        double NextDouble();

        /// <summary>
        /// Fills the provided byte array with random bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill</param>
        void NextBytes(byte[] buffer);

        ///// <summary>
        ///// Reinitialises the random generator with a new seed
        ///// </summary>
        ///// <param name="seed">The seed to use</param>
        //void Reinitialise(int seed);
    }
}
