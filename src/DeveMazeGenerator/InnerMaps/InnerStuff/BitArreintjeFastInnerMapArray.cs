using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.InnerMaps.InnerStuff
{
    public class BitArreintjeFastInnerMapArray
    {
        //Internal since it's used by some other classes (because this is so awesome)
        internal int[] innerData;

        public BitArreintjeFastInnerMapArray(int height)
        {
            innerData = new int[height / 32 + 1];
        }

        public void FillMap(bool state)
        {
            if (state)
            {
                for (int i = 0; i < innerData.Length; i++)
                {
                    //-1 is all 1's
                    innerData[i] = -1;
                }
            }
            else
            {
                for (int i = 0; i < innerData.Length; i++)
                {
                    //0 is all 0's
                    innerData[i] = 0;
                }
            }
        }

        public bool this[int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value)
                {
                    int a = 1 << y;
                    innerData[y / 32] |= a;
                }
                else
                {
                    int a = ~(1 << y);
                    innerData[y / 32] &= a;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (innerData[y / 32] & (1 << y)) != 0;
            }
        }
    }
}
