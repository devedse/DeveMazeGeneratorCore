using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public class InnerMapFactory
    {
        public static T Create<T>(int width, int height) where T : InnerMap
        {
            var typeSwitcher = new TypeSwitch<InnerMap>()
                .Case(() => new BitArreintjeFastInnerMap(width, height));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return (T)createdObject;
        }
    }
}
