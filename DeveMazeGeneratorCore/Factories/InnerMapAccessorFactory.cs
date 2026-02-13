using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Factories
{
    /// <summary>
    /// Factory for creating InnerMapAccessor structs that wrap InnerMap instances.
    /// This enables better JIT inlining by using struct-based generic constraints.
    /// </summary>
    /// <typeparam name="T">The concrete InnerMap type to wrap</typeparam>
    public class InnerMapAccessorFactory<T> : IInnerMapAccessorFactory<InnerMapAccessor<T>> where T : InnerMap
    {
        private readonly IInnerMapFactory<T> _innerMapFactory;

        public InnerMapAccessorFactory(IInnerMapFactory<T> innerMapFactory)
        {
            _innerMapFactory = innerMapFactory;
        }

        public InnerMapAccessor<T> Create(int width, int height)
        {
            var innerMap = _innerMapFactory.Create(width, height);
            return new InnerMapAccessor<T>(innerMap);
        }

        public InnerMapAccessor<T> Create(int width, int height, int startX, int startY)
        {
            var innerMap = _innerMapFactory.Create(width, height, startX, startY);
            return new InnerMapAccessor<T>(innerMap);
        }
    }
}
