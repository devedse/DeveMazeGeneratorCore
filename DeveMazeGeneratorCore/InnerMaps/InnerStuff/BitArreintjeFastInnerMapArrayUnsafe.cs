using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.InnerMaps.InnerStuff
{
    [SkipLocalsInit]
    public sealed class BitArreintjeFastInnerMapArrayUnsafe
    {
        internal long[] _innerData;

        public BitArreintjeFastInnerMapArrayUnsafe(int height)
        {
            _innerData = new long[(height + 63) >> 6];
        }

        public BitArreintjeFastInnerMapArrayUnsafe Clone()
        {
            var cloned = new BitArreintjeFastInnerMapArrayUnsafe(0);
            cloned._innerData = new long[_innerData.Length];
            Array.Copy(_innerData, cloned._innerData, _innerData.Length);
            return cloned;
        }

        public void FillMap(bool state)
        {
            long val = state ? -1L : 0L;
            Array.Fill(_innerData, val);
        }

        public bool this[int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref long searchSpace = ref MemoryMarshal.GetArrayDataReference(_innerData);
                // y / 64
                long val = Unsafe.Add(ref searchSpace, y >> 6);
                return (val & (1L << y)) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref long searchSpace = ref MemoryMarshal.GetArrayDataReference(_innerData);
                int idx = y >> 6;
                ref long valRef = ref Unsafe.Add(ref searchSpace, idx);
                long mask = 1L << y;

                if (value)
                {
                    valRef |= mask;
                }
                else
                {
                    valRef &= ~mask;
                }
            }
        }
    }
}

