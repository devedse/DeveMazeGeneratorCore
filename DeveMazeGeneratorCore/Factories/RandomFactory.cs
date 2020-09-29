using DeveMazeGeneratorCore.Generators.Helpers;

namespace DeveMazeGeneratorCore.Factories
{
    public class RandomFactory<T> : IRandomFactory where T : IRandom
    {
        private readonly int _seed;

        public RandomFactory(int seed)
        {
            this._seed = seed;
        }

        public IRandom Create()
        {
            var typeSwitcher = new TypeSwitch<IRandom>()
                .Case(() => new NetRandom(_seed))
                .Case(() => new XorShiftRandom(_seed));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return createdObject;
        }
    }
}
