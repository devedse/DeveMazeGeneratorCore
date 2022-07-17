using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DeveMazeGeneratorMonoGame
{
    class CubeModel : IDisposable
    {
        public TheGame game;
        public int[] indices = new int[36];
        public VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];

        public float width;
        public float height;
        public float depth;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        public CubeModel(TheGame game, float width, float height, float depth, TexturePosInfo texturePosInfo, float imageSizeFactor)
        {
            this.game = game;
            this.width = width;
            this.height = height;
            this.depth = depth;
            GoGenerateVertices(texturePosInfo, imageSizeFactor);
        }

        public void GoGenerateVertices(TexturePosInfo texturePosInfo, float imageSizeFactor)
        {
            //Front
            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, height, depth), new Vector3(0, 0, 1), texturePosInfo.front.First());
            vertices[1] = new VertexPositionNormalTexture(new Vector3(width, height, depth), new Vector3(0, 0, 1), texturePosInfo.front.Second());
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, 0, depth), new Vector3(0, 0, 1), texturePosInfo.front.Third());
            vertices[3] = new VertexPositionNormalTexture(new Vector3(width, 0, depth), new Vector3(0, 0, 1), texturePosInfo.front.Fourth());

            //Right
            vertices[4] = new VertexPositionNormalTexture(new Vector3(width, height, depth), new Vector3(1, 0, 0), texturePosInfo.right.First());
            vertices[5] = new VertexPositionNormalTexture(new Vector3(width, height, 0), new Vector3(1, 0, 0), texturePosInfo.right.Second());
            vertices[6] = new VertexPositionNormalTexture(new Vector3(width, 0, depth), new Vector3(1, 0, 0), texturePosInfo.right.Third());
            vertices[7] = new VertexPositionNormalTexture(new Vector3(width, 0, 0), new Vector3(1, 0, 0), texturePosInfo.right.Fourth());

            //Rear
            vertices[8] = new VertexPositionNormalTexture(new Vector3(width, height, 0), new Vector3(0, 0, -1), texturePosInfo.rear.First());
            vertices[9] = new VertexPositionNormalTexture(new Vector3(0, height, 0), new Vector3(0, 0, -1), texturePosInfo.rear.Second());
            vertices[10] = new VertexPositionNormalTexture(new Vector3(width, 0, 0), new Vector3(0, 0, -1), texturePosInfo.rear.Third());
            vertices[11] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, 0, -1), texturePosInfo.rear.Fourth());

            //Left
            vertices[12] = new VertexPositionNormalTexture(new Vector3(0, height, 0), new Vector3(-1, 0, 0), texturePosInfo.left.First());
            vertices[13] = new VertexPositionNormalTexture(new Vector3(0, height, depth), new Vector3(-1, 0, 0), texturePosInfo.left.Second());
            vertices[14] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), texturePosInfo.left.Third());
            vertices[15] = new VertexPositionNormalTexture(new Vector3(0, 0, depth), new Vector3(-1, 0, 0), texturePosInfo.left.Fourth());

            //Top
            vertices[16] = new VertexPositionNormalTexture(new Vector3(0, height, 0), new Vector3(0, 1, 0), texturePosInfo.top.First());
            vertices[17] = new VertexPositionNormalTexture(new Vector3(width, height, 0), new Vector3(0, 1, 0), texturePosInfo.top.Second());
            vertices[18] = new VertexPositionNormalTexture(new Vector3(0, height, depth), new Vector3(0, 1, 0), texturePosInfo.top.Third());
            vertices[19] = new VertexPositionNormalTexture(new Vector3(width, height, depth), new Vector3(0, 1, 0), texturePosInfo.top.Fourth());

            //Bottom
            vertices[20] = new VertexPositionNormalTexture(new Vector3(0, 0, depth), new Vector3(0, -1, 0), texturePosInfo.bottom.First());
            vertices[21] = new VertexPositionNormalTexture(new Vector3(width, 0, depth), new Vector3(0, -1, 0), texturePosInfo.bottom.Second());
            vertices[22] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, -1, 0), texturePosInfo.bottom.Third());
            vertices[23] = new VertexPositionNormalTexture(new Vector3(width, 0, 0), new Vector3(0, -1, 0), texturePosInfo.bottom.Fourth());


            int cur = 0;
            for (int i = 0; i < 24; i += 4)
            {
                indices[cur + 0] = 0 + i;
                indices[cur + 1] = 1 + i;
                indices[cur + 2] = 2 + i;
                indices[cur + 3] = 1 + i;
                indices[cur + 4] = 3 + i;
                indices[cur + 5] = 2 + i;
                cur += 6;
            }

            //This stuff is for repeating the texture
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                vert.TextureCoordinate.X *= (width / imageSizeFactor);
                vert.TextureCoordinate.Y *= (depth / imageSizeFactor);
                //vert.TextureCoordinate.X /= 10.0f;
                //vert.TextureCoordinate.Y /= 10.0f;
                vertices[i] = vert;
            }


            vertexBuffer = new VertexBuffer(game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            if (game.Platform == Platform.Blazor)
            {
                indexBuffer = new IndexBuffer(game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                if (indices.Any(t => t > short.MaxValue))
                {
                    throw new InvalidOperationException("Could not use a maze this big due to the indices being too high");
                }
                var indicesConverted = indices.Select(t => (short)t).ToArray();
                indexBuffer.SetData(indicesConverted);
            }
            else
            {
                indexBuffer = new IndexBuffer(game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(Matrix parentMatrix, BasicEffect effect)
        {
            effect.World = parentMatrix;

            game.GraphicsDevice.Indices = indexBuffer;
            game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Length / 3);
            }
        }

        public void Dispose()
        {
            if (game.Platform != Platform.Blazor)
            {
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                }
                if (indexBuffer != null)
                {
                    indexBuffer.Dispose();
                }
            }
        }
    }
}
