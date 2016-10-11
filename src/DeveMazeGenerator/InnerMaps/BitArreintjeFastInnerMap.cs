using DeveMazeGenerator.InnerMaps.InnerStuff;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.InnerMaps
{
    public class BitArreintjeFastInnerMap : InnerMap
    {
        private BitArreintjeFastInnerMapArray[] innerData;

        public BitArreintjeFastInnerMap(int width, int height)
            : base(width, height)
        {
            innerData = new BitArreintjeFastInnerMapArray[width];
            for (int i = 0; i < width; i++)
            {
                innerData[i] = new BitArreintjeFastInnerMapArray(height);
            }
        }

        public override void FillMap(bool state)
        {
            for (int i = 0; i < innerData.Length; i++)
            {
                innerData[i].FillMap(state);
            }
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
