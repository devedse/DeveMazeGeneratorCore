using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeveMazeGeneratorCore.MonoGame.Core
{
    public class Drawable3DObject<T> : IDisposable where T : struct
    {
        private List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();
        private List<IndexBuffer> _indexBuffers = new List<IndexBuffer>();

        private int _curVertI;
        private int _curIndexI;

        public T[] _curVertices;
        public int[] _curIndicesInt;
        public short[] _curIndicesShort;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _vertexCount;
        private readonly int _indexCount;
        private readonly int _verticesPerOperation;
        private readonly int[] _relativeIndexPointers;
        private readonly short[] _relativeIndexPointersShort;
        private readonly IndexElementSize _indexElementSize;
        private int _maxCountVerticesPerBuffer = int.MaxValue;

        public Drawable3DObject(GraphicsDevice graphicsDevice, int vertexCount, int indexCount, int verticesPerOperation, int[] relativeIndexPointers, IndexElementSize indexElementSize)
        {
            _graphicsDevice = graphicsDevice;

            _vertexCount = vertexCount;
            _indexCount = indexCount;

            _verticesPerOperation = verticesPerOperation;
            _relativeIndexPointers = relativeIndexPointers;

            _relativeIndexPointersShort = relativeIndexPointers.Select(t => (short)t).ToArray();

            _indexElementSize = indexElementSize;

            _maxCountVerticesPerBuffer = _indexElementSize == IndexElementSize.ThirtyTwoBits ? int.MaxValue : short.MaxValue;

            InitializeArrays();
        }

        private void StoreArraysInBuffers()
        {
            var vertexBuffer = new VertexBuffer(_graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, _curVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(_curVertices);
            _vertexBuffers.Add(vertexBuffer);

            var indexBuffer = new IndexBuffer(_graphicsDevice, _indexElementSize, _indexElementSize == IndexElementSize.ThirtyTwoBits ? _curIndicesInt.Length : _curIndicesShort.Length, BufferUsage.WriteOnly);
            if (_indexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                indexBuffer.SetData(_curIndicesInt);
            }
            else
            {
                indexBuffer.SetData(_curIndicesShort);
            }
            _indexBuffers.Add(indexBuffer);
        }

        private void InitializeArrays()
        {
            int maxVertexOperations = _maxCountVerticesPerBuffer / _verticesPerOperation;
            int curVertexOperations = Math.Min((_vertexCount - (_vertexBuffers.Count * maxVertexOperations)) / _verticesPerOperation, maxVertexOperations);

            _curVertices = new T[curVertexOperations * _verticesPerOperation];

            var indicesArrayLength = _relativeIndexPointers.Length * curVertexOperations;
            if (_indexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                _curIndicesInt = new int[indicesArrayLength];
            }
            else
            {
                _curIndicesShort = new short[indicesArrayLength];
            }

            _curVertI = 0;
            _curIndexI = 0;
        }


        public void AddObject(params T[] vertices)
        {
            if (vertices.Length != _verticesPerOperation)
            {
                throw new InvalidOperationException("Mag niet");
            }

            if (_curVertI + vertices.Length > _maxCountVerticesPerBuffer)
            {
                StoreArraysInBuffers();
                InitializeArrays();
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                _curVertices[_curVertI++] = vertices[i];
            }


            int vertexIBefore = _curVertI - vertices.Length;
            if (_indexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                for (int i = 0; i < _relativeIndexPointers.Length; i++)
                {
                    _curIndicesInt[_curIndexI++] = _relativeIndexPointers[i];
                }
            }
            else
            {
                for (int i = 0; i < _relativeIndexPointers.Length; i++)
                {
                    _curIndicesShort[_curIndexI++] = (short)(_relativeIndexPointersShort[i] + vertexIBefore);
                }
            }

            if (_curVertI + (_vertexBuffers.Count * _maxCountVerticesPerBuffer) >= _vertexCount)
            {
                StoreArraysInBuffers();
            }
        }

        public void Draw(BasicEffect effect)
        {
            if (_vertexBuffers.Count != _indexBuffers.Count)
            {
                throw new InvalidOperationException("Dit kan nooit gebeuren");
            }

            for (int i = 0; i < _vertexBuffers.Count; i++)
            {
                var vertexBuffer = _vertexBuffers[i];
                var indexBuffer = _indexBuffers[i];
                _graphicsDevice.Indices = indexBuffer;
                _graphicsDevice.SetVertexBuffer(vertexBuffer);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
                }
            }
        }

        public void Dispose()
        {
            foreach (var vertexBuffer in _vertexBuffers)
            {
                vertexBuffer.Dispose();
            }
            foreach (var indexBuffer in _indexBuffers)
            {
                indexBuffer.Dispose();
            }
        }
    }
}
