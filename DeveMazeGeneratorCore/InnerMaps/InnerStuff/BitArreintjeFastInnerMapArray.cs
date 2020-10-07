using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps.InnerStuff
{
    public class BitArreintjeFastInnerMapArray
    {
        //Internal since it's used by some other classes (because this is so awesome)
        internal long[] _innerData;

        public BitArreintjeFastInnerMapArray(int height)
        {
            _innerData = new long[height / 64 + 1];
        }

        public BitArreintjeFastInnerMapArray Clone()
        {
            var cloned = new BitArreintjeFastInnerMapArray(0);
            cloned._innerData = new long[_innerData.Length];
            Array.Copy(_innerData, cloned._innerData, _innerData.Length);
            return cloned;
        }

        public void FillMap(bool state)
        {
            if (state)
            {
                for (int i = 0; i < _innerData.Length; i++)
                {
                    //-1 is all 1's
                    _innerData[i] = -1;
                }
            }
            else
            {
                for (int i = 0; i < _innerData.Length; i++)
                {
                    //0 is all 0's
                    _innerData[i] = 0;
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
                    long a = 1 << y;
                    _innerData[y / 64] |= a;
                }
                else
                {
                    long a = ~(1 << y);
                    _innerData[y / 64] &= a;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (_innerData[y / 64] & 1 << y) != 0;
            }
        }
    }
}
