using DeveMazeGeneratorCore.MonoGame.Core.ExtensionMethods;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.MonoGame.Core
{
    public class Drawable3DObject<T> : IDisposable where T : struct
    {
        private List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();
        private List<IndexBuffer> _indexBuffers = new List<IndexBuffer>();

        private int _curVertI;
        private int _curIndexI;

        private T[] _curVertices;
        private int[] _curIndicesInt;
        private short[] _curIndicesShort;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _vertexCount;
        private readonly int _indexCount;
        private readonly int _verticesPerOperation;
        private readonly int _indicesPerOperation;
        private readonly int[] _relativeIndexPointers;
        private readonly short[] _relativeIndexPointersShort;
        private readonly IndexElementSize _indexElementSize;

        //The actual number if we look at operations (e.g. 6 indices per operation)
        private int _maxCountPrimitivesPerBuffer;
        private readonly int _maxCountIndicesPerBuffer;
        private readonly int _maxOperationsPerBuffer;
        private readonly int _maxCountVerticesPerBuffer;

        /// <summary>
        /// Create a new Drawable3DOBject
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice from game.GraphicsDevice</param>
        /// <param name="vertexCount">The total amount of vertices of the object</param>
        /// <param name="indexCount">The total amount of indices of the object</param>
        /// <param name="verticesPerOperation">The amount of vertices to add per "AddObject" call</param>
        /// <param name="relativeIndexPointers"></param>
        /// <param name="indexElementSize"></param>
        public Drawable3DObject(GraphicsDevice graphicsDevice, int vertexCount, int indexCount, int verticesPerOperation, int[] relativeIndexPointers, IndexElementSize? indexElementSize = null)
        {
            _graphicsDevice = graphicsDevice;

            _vertexCount = vertexCount;
            _indexCount = indexCount; //This field is actually not used but we keep it here to verify incoming data

            _verticesPerOperation = verticesPerOperation;
            _relativeIndexPointers = relativeIndexPointers;
            _indicesPerOperation = relativeIndexPointers.Length;

            if (_vertexCount % _verticesPerOperation != 0)
            {
                throw new ArgumentException($"`_vertexCount % _verticesPerOperation == 0` needs to be true. It's not :(");
            }

            if (_indexCount % _relativeIndexPointers.Length != 0)
            {
                throw new ArgumentException($"`_indexCount % _relativeIndexPointers.Length != 0` needs to be true. It's not :(");
            }

            if (_vertexCount / _verticesPerOperation != _indexCount / _relativeIndexPointers.Length)
            {
                throw new ArgumentException($"`_vertexCount / _verticesPerOperation != _indexCount / _relativeIndexPointers.Length` needs to be true. It's not :(");
            }

            _relativeIndexPointersShort = relativeIndexPointers.Select(t => (short)t).ToArray();

            _indexElementSize = indexElementSize ?? graphicsDevice.GetPreferedIndexElementSize();

            //Workaround for: https://github.com/MonoGame/MonoGame/pull/8112
            var vertexSize = Marshal.SizeOf<T>();

            _maxCountPrimitivesPerBuffer = _indexElementSize == IndexElementSize.ThirtyTwoBits ? 1048575 : ushort.MaxValue;
            _maxCountIndicesPerBuffer = _maxCountPrimitivesPerBuffer * 3;
            _maxOperationsPerBuffer = _maxCountIndicesPerBuffer / _indicesPerOperation;
            _maxCountVerticesPerBuffer = _maxOperationsPerBuffer * verticesPerOperation;
            

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
            int curVertexOperations = Math.Min((_vertexCount - (_vertexBuffers.Count * _maxCountVerticesPerBuffer)), _maxCountVerticesPerBuffer);

            _curVertices = new T[curVertexOperations];

            var indicesArrayLength = curVertexOperations / _verticesPerOperation * _indicesPerOperation;
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
                throw new InvalidOperationException($"This {this.GetType().Name} is configured to be used with sets of {_verticesPerOperation} vertices per operation. But you are passing in: {vertices.Length}");
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
                    _curIndicesInt[_curIndexI++] = _relativeIndexPointers[i] + vertexIBefore;
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

                Console.WriteLine($"Created a total of {_vertexBuffers.Count} vertex buffers and {_indexBuffers.Count} indexbuffers for this {this.GetType().Name}");
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

                    var primitiveCount = indexBuffer.IndexCount / 3;

                    if (_graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                        throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
                    if (_graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                        throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
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
