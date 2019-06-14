using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.InnerMaps
{
    public class BoolInnerMap : InnerMap
    {
        private bool[][] innerData;

        public BoolInnerMap(int width, int height)
            : base(width, height)
        {
            innerData = new bool[width][];
            for (int i = 0; i < width; i++)
            {
                innerData[i] = new bool[height];
            }
        }

        public override void FillMap(bool state)
        {
            for (int i = 0; i < innerData.Length; i++)
            {
                var arrayHere = innerData[i];
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
                return innerData[x][y];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                innerData[x][y] = value;
            }
        }
    }
}
