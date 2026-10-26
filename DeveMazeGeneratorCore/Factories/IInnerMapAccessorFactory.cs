using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Factories
{
    /// <summary>
    /// Factory interface for creating struct-based map accessors that enable better JIT inlining.
    /// </summary>
    /// <typeparam name="TAccessor">The struct accessor type</typeparam>
    public interface IInnerMapAccessorFactory<TAccessor> where TAccessor : struct, IInnerMapAccessor
    {
        TAccessor Create(int width, int height);
        TAccessor Create(int width, int height, int startX, int startY);
    }
}
