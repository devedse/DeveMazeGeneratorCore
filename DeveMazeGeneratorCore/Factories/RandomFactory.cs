using DeveMazeGeneratorCore.Generators.Helpers;

namespace DeveMazeGeneratorCore.Factories
{
    public class RandomFactory<T> : IRandomFactory where T : IRandom
    {
        public IRandom Create(int seed)
        {
            var typeSwitcher = new TypeSwitch<IRandom>()
                .Case(() => new NetRandom(seed))
                .Case(() => new XorShiftRandom(seed));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return createdObject;
        }
    }
}
