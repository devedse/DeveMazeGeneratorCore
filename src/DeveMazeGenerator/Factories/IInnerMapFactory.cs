using DeveMazeGenerator.InnerMaps;

namespace DeveMazeGenerator.Factories
{
    public interface IInnerMapFactory<T> where T : InnerMap
    {
        int Width { get; }
        int Height { get; }

        T Create();
        T Create(int width, int height);
        T Create(int width, int height, int startX, int startY);
    }
}