using DeveMazeGenerator.Generators.Helpers;

namespace DeveMazeGenerator.Factories
{
    public class RandomFactory<T> : IRandomFactory where T : IRandom
    {
        private readonly int seed;

        public RandomFactory(int seed)
        {
            this.seed = seed;
        }

        public IRandom Create()
        {
            var typeSwitcher = new TypeSwitch<IRandom>()
                .Case(() => new NetRandom(seed))
                .Case(() => new XorShiftRandom(seed));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return createdObject;
        }
    }
}
