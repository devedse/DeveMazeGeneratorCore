using DeveMazeGeneratorCore.Helpers;
using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    /// <summary>
    /// This is a test that's actually slower then the normal InnerMap. So don't use it :).
    /// </summary>
    public class BitArreintjeFastHilbertInnerMap : InnerMap
    {
        internal long[] _innerData;

        private int _hilCurveSize;

        public BitArreintjeFastHilbertInnerMap(int width, int height) : base(width, height)
        {
            _innerData = new long[(width * height) / 64 + 1];

            var max = Math.Max(width, height);
            _hilCurveSize = CeilPower2(max);
        }


        public static int CeilPower2(int x)
        {
            if (x < 2)
            {
                return 1;
            }
            return (int)Math.Pow(2, (int)Math.Log(x - 1, 2) + 1);
        }


        public override void FillMap(bool state)
        {
            throw new NotImplementedException();
            //for (int i = 0; i < _innerData.Length; i++)
            //{
            //}
        }

        public override InnerMap Clone()
        {
            throw new NotImplementedException();
            //var innerMapTarget = new BitArreintjeFastInnerMap(Width, Height);
            //for (int i = 0; i < _innerData.Length; i++)
            //{
            //    innerMapTarget._innerData[i] = _innerData[i].Clone();
            //}
            //return innerMapTarget;
        }

        public override bool this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var pos = HilbertCurve.Xy2d(_hilCurveSize, x, y);

                if (value)
                {
                    long a = 1L << pos;
                    _innerData[pos / 64] |= a;
                }
                else
                {
                    long a = ~(1L << pos);
                    _innerData[pos / 64] &= a;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var pos = HilbertCurve.Xy2d(_hilCurveSize, x, y);

                return (_innerData[pos / 64] & 1L << pos) != 0;
            }
        }
    }
}
