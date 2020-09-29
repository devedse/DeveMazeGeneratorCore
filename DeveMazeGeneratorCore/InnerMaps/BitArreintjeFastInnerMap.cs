using DeveMazeGeneratorCore.InnerMaps.InnerStuff;
using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class BitArreintjeFastInnerMap : InnerMap
    {
        private BitArreintjeFastInnerMapArray[] _innerData;

        public BitArreintjeFastInnerMap(int width, int height)
            : base(width, height)
        {
            _innerData = new BitArreintjeFastInnerMapArray[width];
            for (int i = 0; i < width; i++)
            {
                _innerData[i] = new BitArreintjeFastInnerMapArray(height);
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
            var innerMapTarget = new BitArreintjeFastInnerMap(Width, Height);
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
                return _innerData[x][y];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _innerData[x][y] = value;
            }
        }
    }
}
