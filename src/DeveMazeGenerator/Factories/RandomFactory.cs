using DeveMazeGenerator.Generators.Helpers;

namespace DeveMazeGenerator.Factories
{
    public static class RandomFactory
    {
        public static IRandom Create<T>(int seed) where T : IRandom
        {
            var typeSwitcher = new TypeSwitch<IRandom>()
                .Case(() => new NetRandom(seed))
                .Case(() => new XorShiftRandom(seed));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return createdObject;
        }
    }
}
