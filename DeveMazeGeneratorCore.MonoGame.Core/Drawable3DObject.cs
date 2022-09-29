using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.MonoGame.Core
{
    public class Drawable3DObject<T> : IDisposable where T : struct
    {
        private List<VertexBuffer> _vertexBuffer = new List<VertexBuffer>();
        private List<IndexBuffer> _indexBuffer = new List<IndexBuffer>();

        private T[] _curVertices;
        private int[] _curIndicesInt;
        private short[] _curIndicesShort;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _vertexCount;
        private readonly int _indexCount;
        private readonly int _verticesPerOperation;
        private readonly int[] _relativeIndexPointers;
        private readonly short[] _relativeIndexPointersShort;
        private readonly IndexElementSize _indexElementSize;
        private int _maxCountPerBuffer = int.MaxValue;


        public Drawable3DObject(GraphicsDevice graphicsDevice, int vertexCount, int indexCount, int verticesPerOperation, int[] relativeIndexPointers, IndexElementSize indexElementSize)
        {
            _graphicsDevice = graphicsDevice;

            _vertexCount = vertexCount;
            _indexCount = indexCount;

            _verticesPerOperation = verticesPerOperation;
            _relativeIndexPointers = relativeIndexPointers;

            _relativeIndexPointersShort = relativeIndexPointers.Select(t => (short)t).ToArray();

            _indexElementSize = indexElementSize;

            _maxCountPerBuffer = indexElementSize == IndexElementSize.ThirtyTwoBits ? int.MaxValue : short.MaxValue;
        
            _curVertices = new T[Math.Min(vertexCount, _maxCountPerBuffer)];
            if (indexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                var aaa = indexCount / _maxCountPerBuffer;

            }
            else
            {

            }
        }

        public void AddObject(params T[] vertices)
        {
            if (vertices.Length != _relativeIndexPointers.Length)
            {
                throw new InvalidOperationException("Mag niet");
            }

            if (_curVertices.Count + vertices.Length > _maxCountPerBuffer)
            {
                var vertexBuffer = new VertexBuffer(_graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
                var indexBuffer = new IndexBuffer(_graphicsDevice, _indexElementSize, _indexElementSize == IndexElementSize.ThirtyTwoBits ? _curIndicesInt.Count : _curIndicesShort.Count, BufferUsage.WriteOnly);

                var spanneke = CollectionsMarshal.AsSpan(_curVertices);

                vertexBuffer.SetData<T>();
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
