using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Factories
{
    public class InnerMapFactory<T> : IInnerMapFactory<T> where T : InnerMap
    {
        public InnerMapFactory()
        {
        }

        public T Create(int desiredWidth, int desiredHeight)
        {
            var typeSwitcher = new TypeSwitch<InnerMap>()
              .Case(() => new BitArreintjeFastInnerMap(desiredWidth, desiredHeight))
              .Case(() => new BoolInnerMap(desiredWidth, desiredHeight));

            var createdObject = typeSwitcher.Switch(typeof(T));
            return (T)createdObject;
        }

        public T Create(int desiredWidth, int desiredHeight, int startX, int startY)
        {
            var castedObject = Create(desiredWidth, desiredHeight);
            castedObject.StartX = startX;
            castedObject.StartY = startY;
            return castedObject;
        }
    }
}
