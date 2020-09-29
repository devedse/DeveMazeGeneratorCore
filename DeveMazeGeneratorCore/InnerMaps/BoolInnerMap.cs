using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class BoolInnerMap : InnerMap
    {
        private bool[][] _innerData;

        public BoolInnerMap(int width, int height)
            : base(width, height)
        {
            _innerData = new bool[width][];
            for (int i = 0; i < width; i++)
            {
                _innerData[i] = new bool[height];
            }
        }

        public override void FillMap(bool state)
        {
            for (int i = 0; i < _innerData.Length; i++)
            {
                var arrayHere = _innerData[i];
                for (int y = 0; y < arrayHere.Length; y++)
                {
                    arrayHere[y] = state;
                }
            }
        }

        public override InnerMap Clone()
        {
            throw new NotImplementedException("Clone is not yet implemented for BoolInnerMap");
            //var innerMapTarget = new BitArreintjeFastInnerMap(Width, Height);
            //for (int i = 0; i < innerData.Length; i++)
            //{
            //    innerMapTarget.innerData[i] = innerData[i].Clone();
            //}
            //return innerMapTarget;
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
