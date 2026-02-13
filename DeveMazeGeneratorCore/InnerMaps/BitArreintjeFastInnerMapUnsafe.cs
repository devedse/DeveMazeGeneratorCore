using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    [SkipLocalsInit]
    public sealed class BitArreintjeFastInnerMapUnsafe : InnerMap
    {
        // One big array for the whole maze
        // Layout: x0_col, x1_col, x2_col...
        // x0_col = [long0_for_y0-63, long1_for_y64-127...]
        internal long[] _innerData;
        internal int _heightLongs;

        public BitArreintjeFastInnerMapUnsafe(int width, int height)
            : base(width, height)
        {
            // Use bitwise operations for size calculation ((height + 63) / 64)
            _heightLongs = (height + 63) >> 6;
            _innerData = new long[width * _heightLongs];
        }

        public override void FillMap(bool state)
        {
            long fillVal = state ? -1L : 0L;
            Array.Fill(_innerData, fillVal);
        }

        public override InnerMap Clone()
        {
            var innerMapTarget = new BitArreintjeFastInnerMapUnsafe(Width, Height);
            Array.Copy(_innerData, innerMapTarget._innerData, _innerData.Length);
            return innerMapTarget;
        }

        public override bool this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Obtain a reference to the 0th element of the array (skipping bounds check)
                ref long searchSpace = ref MemoryMarshal.GetArrayDataReference(_innerData);
                
                // Calculate index: x * STRIDE + (y / 64)
                // Use bit shift for division by 64
                int index = x * _heightLongs + (y >> 6);
                
                // Fetch value using Unsafe pointer arithmetic
                long val = Unsafe.Add(ref searchSpace, index);
                
                // Check bit
                return (val & (1L << y)) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref long searchSpace = ref MemoryMarshal.GetArrayDataReference(_innerData);
                int index = x * _heightLongs + (y >> 6);
                long mask = 1L << y;
                
                // Get reference to the specific long to modify
                ref long valRef = ref Unsafe.Add(ref searchSpace, index);
                
                // Branchless set operation logic (often compiled to Conditional Move or similar)
                // If value is true, we OR the mask. If false, we AND NOT the mask.
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

