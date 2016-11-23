using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public class InnerMapFactoryCustom<T> : IInnerMapFactory<T> where T : InnerMap
    {
        private T map;

        public InnerMapFactoryCustom(T map)
        {
            this.map = map;
        }

        public T Create()
        {
            return map;
        }
    }
}
