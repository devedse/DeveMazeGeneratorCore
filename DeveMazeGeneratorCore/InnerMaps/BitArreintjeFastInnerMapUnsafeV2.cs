using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps.InnerStuff;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    [SkipLocalsInit]
    public sealed class BitArreintjeFastInnerMapUnsafeV2 : InnerMap
    {
        internal BitArreintjeFastInnerMapArrayUnsafe[] _innerData;

        public BitArreintjeFastInnerMapUnsafeV2(int width, int height)
            : base(width, height)
        {
            _innerData = new BitArreintjeFastInnerMapArrayUnsafe[width];
            for (int i = 0; i < width; i++)
            {
                _innerData[i] = new BitArreintjeFastInnerMapArrayUnsafe(height);
            }
        }

        public override void FillMap(bool state)
        {
            for (int i = 0; i < _innerData.Length; i++)
            {
                _innerData[i].FillMap(state);
            }
        }

        public override InnerMap Clone()
        {
            var innerMapTarget = new BitArreintjeFastInnerMapUnsafeV2(Width, Height);
            for (int i = 0; i < _innerData.Length; i++)
            {
                innerMapTarget._innerData[i] = _innerData[i].Clone();
            }
            return innerMapTarget;
        }

        public override bool this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref BitArreintjeFastInnerMapArrayUnsafe arrayRef = ref MemoryMarshal.GetArrayDataReference(_innerData);
                return Unsafe.Add(ref arrayRef, x)[y];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref BitArreintjeFastInnerMapArrayUnsafe arrayRef = ref MemoryMarshal.GetArrayDataReference(_innerData);
                Unsafe.Add(ref arrayRef, x)[y] = value;
            }
        }
    }
}
