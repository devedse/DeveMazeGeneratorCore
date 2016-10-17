using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public class InnerMapFactory<T> : IInnerMapFactory<T> where T : InnerMap
    {
        private readonly int width;
        private readonly int height;

        public InnerMapFactory(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public T Create()
        {
            var typeSwitcher = new TypeSwitch<InnerMap>()
                .Case(() => new BitArreintjeFastInnerMap(width, height))
                .Case(() => new UndefinedInnerMap(width, height));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return (T)createdObject;
        }
    }
}
