using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public class InnerMapFactoryCustom<T> : IInnerMapFactory<T> where T : InnerMap
    {
        public int Width => map.Width;
        public int Height => map.Height;

        private T map;

        public InnerMapFactoryCustom(T map)
        {
            this.map = map;
        }

        public T Create()
        {
            return map;
        }

        public T Create(int width, int height, int startX, int startY)
        {
            return map;
        }

        public T Create(int startX, int startY)
        {
            return map;
        }
    }
}
