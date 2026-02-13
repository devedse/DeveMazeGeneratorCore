using DeveMazeGeneratorCore.InnerMaps;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.Generators.SpeedOptimization
{
    public interface IMapAccessor<TMap>
    {
        bool Get(TMap map, int x, int y);
        void Set(TMap map, int x, int y, bool val);
    }


    public struct FastV2Accessor : IMapAccessor<BitArreintjeFastInnerMapUnsafeV2>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(BitArreintjeFastInnerMapUnsafeV2 map, int x, int y) 
            => map._innerData[x][y];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(BitArreintjeFastInnerMapUnsafeV2 map, int x, int y, bool val) 
            => map._innerData[x][y] = val;
    }

    public struct GenericAccessor<TMap> : IMapAccessor<TMap> where TMap : InnerMap
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(TMap map, int x, int y) => map[x, y];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TMap map, int x, int y, bool val) => map[x, y] = val;
    }
}
