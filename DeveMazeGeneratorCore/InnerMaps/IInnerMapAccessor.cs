namespace DeveMazeGeneratorCore.InnerMaps
{
    /// <summary>
    /// Interface for struct-based map accessors that enable better JIT inlining
    /// by avoiding virtual calls through the class-based InnerMap hierarchy.
    /// </summary>
    public interface IInnerMapAccessor
    {
        int Width { get; }
        int Height { get; }
        bool this[int x, int y] { get; set; }
        
        /// <summary>
        /// Gets the underlying InnerMap instance for creating the final Maze.
        /// </summary>
        InnerMap GetInnerMap();
    }
}
