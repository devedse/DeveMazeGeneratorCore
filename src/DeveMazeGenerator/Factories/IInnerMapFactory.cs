using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public interface IInnerMapFactory<T> where T : InnerMap
    {
        T Create();
    }
}