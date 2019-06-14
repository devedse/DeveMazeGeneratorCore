using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public class InnerMapFactory<T> : IInnerMapFactory<T> where T : InnerMap
    {
        public int Width { get; }
        public int Height { get; }

        public InnerMapFactory(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public T Create()
        {
            return Create(Width, Height);
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
