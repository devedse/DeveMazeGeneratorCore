using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    /// <summary>
    /// A struct wrapper around InnerMap that enables better JIT inlining by avoiding
    /// virtual calls. This is used as a generic constraint (where M : struct, IInnerMapAccessor)
    /// to allow the JIT to generate optimized code with inlined method calls.
    /// </summary>
    /// <typeparam name="T">The concrete InnerMap type being wrapped</typeparam>
    public readonly struct InnerMapAccessor<T> : IInnerMapAccessor where T : InnerMap
    {
        private readonly T _innerMap;

        public InnerMapAccessor(T innerMap)
        {
            _innerMap = innerMap;
        }

        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _innerMap.Width;
        }

        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _innerMap.Height;
        }

        public bool this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _innerMap[x, y];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _innerMap[x, y] = value;
        }

        /// <summary>
        /// Gets the underlying InnerMap instance for creating the final Maze.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InnerMap GetInnerMap() => _innerMap;
    }
}
