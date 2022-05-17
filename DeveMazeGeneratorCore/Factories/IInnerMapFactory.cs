using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Factories
{
    public interface IInnerMapFactory<T> where T : InnerMap
    {
        T Create(int width, int height);
        T Create(int width, int height, int startX, int startY);
    }
}