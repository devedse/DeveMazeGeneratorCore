using DeveMazeGeneratorCore.InnerMaps.InnerStuff;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class BitArreintjeFastChunkedInnerMap : InnerMap
    {
        private BitArreintjeFastInnerMapArray[] _innerData;

        public const int ChunkSize = 8;
        private int _chunksPerRow;

        public BitArreintjeFastChunkedInnerMap(int width, int height)
            : base(width, height)
        {
            _chunksPerRow = width / ChunkSize;
            var chunkCount = _chunksPerRow * (height / ChunkSize);

            _innerData = new BitArreintjeFastInnerMapArray[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                _innerData[i] = new BitArreintjeFastInnerMapArray(ChunkSize * ChunkSize);
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
            var innerMapTarget = new BitArreintjeFastChunkedInnerMap(Width, Height);
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
                int chunkX = x / ChunkSize;
                int chunkY = y / ChunkSize;

                var chunkNumber = chunkY * _chunksPerRow + chunkX;

                int localX = x % ChunkSize;
                int localY = y % ChunkSize;

                var positionInChunk = localY * ChunkSize + localX;


                return _innerData[chunkNumber][positionInChunk];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int chunkX = x / ChunkSize;
                int chunkY = y / ChunkSize;

                var chunkNumber = chunkY * _chunksPerRow + chunkX;

                int localX = x % ChunkSize;
                int localY = y % ChunkSize;

                var positionInChunk = localY * ChunkSize + localX;

                _innerData[chunkNumber][positionInChunk] = value;
            }
        }
    }
}
