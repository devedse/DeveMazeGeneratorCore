using DeveMazeGeneratorCore.MonoGame.Core;
using DeveMazeGeneratorCore.Structures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeveMazeGeneratorMonoGame
{
    class WallModel
    {
        private MazeWall mazeWall;


        public WallModel(MazeWall mazeWall)
        {
            this.mazeWall = mazeWall;


            //GoGenerateVertices(TexturePosInfoGenerator.FullImage);
        }


        public void GoGenerateVerticesv2(Drawable3DObject<VertexPositionNormalTexture> drawable3DObject)
        {
            TexturePosInfo texturePosInfo = TexturePosInfoGenerator.FullImage;

            float height = 4f / 3f;



            float hhh = height;
            float www = mazeWall.Yend - mazeWall.Ystart + mazeWall.Xend - mazeWall.Xstart;


            drawable3DObject.AddObject(
                //Front
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, height, mazeWall.Ystart), new Vector3(0, 0, 1), texturePosInfo.front.First()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, height, mazeWall.Yend), new Vector3(0, 0, 1), texturePosInfo.front.Second()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, 0, mazeWall.Ystart), new Vector3(0, 0, 1), texturePosInfo.front.Third()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, 0, mazeWall.Yend), new Vector3(0, 0, 1), texturePosInfo.front.Fourth()),

                //Rear
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, height, mazeWall.Ystart), new Vector3(0, 0, -1), texturePosInfo.rear.Second()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, 0, mazeWall.Ystart), new Vector3(0, 0, -1), texturePosInfo.rear.Fourth()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, height, mazeWall.Yend), new Vector3(0, 0, -1), texturePosInfo.rear.First()),
                new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, 0, mazeWall.Yend), new Vector3(0, 0, -1), texturePosInfo.rear.Third())
            );




            //////This stuff is for repeating the texture
            //for (int i = curVertice; i < curVertice + howmuchvertices; i++)
            //{
            //    var vert = vertices[i];
            //    vert.TextureCoordinate.X *= (www / 2f);
            //    vert.TextureCoordinate.Y *= (hhh / height);
            //    vertices[i] = vert;
            //}

            //curVertice += howmuchvertices;


        }






        public void GoGenerateVertices(VertexPositionNormalTexture[] vertices, int[] indices, ref int curVertice, ref int curIndice)
        {
            TexturePosInfo texturePosInfo = TexturePosInfoGenerator.FullImage;

            float height = 4f / 3f;

            int howmuchvertices = 8;

            //Front
            vertices[curVertice + 0] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, height, mazeWall.Ystart), new Vector3(0, 0, 1), texturePosInfo.front.First());
            vertices[curVertice + 1] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, height, mazeWall.Yend), new Vector3(0, 0, 1), texturePosInfo.front.Second());
            vertices[curVertice + 2] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, 0, mazeWall.Ystart), new Vector3(0, 0, 1), texturePosInfo.front.Third());
            vertices[curVertice + 3] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, 0, mazeWall.Yend), new Vector3(0, 0, 1), texturePosInfo.front.Fourth());

            //Rear
            vertices[curVertice + 4] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, height, mazeWall.Ystart), new Vector3(0, 0, -1), texturePosInfo.rear.Second());
            vertices[curVertice + 5] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xstart, 0, mazeWall.Ystart), new Vector3(0, 0, -1), texturePosInfo.rear.Fourth());
            vertices[curVertice + 6] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, height, mazeWall.Yend), new Vector3(0, 0, -1), texturePosInfo.rear.First());
            vertices[curVertice + 7] = new VertexPositionNormalTexture(new Vector3(mazeWall.Xend, 0, mazeWall.Yend), new Vector3(0, 0, -1), texturePosInfo.rear.Third());



            for (int i = 0; i < howmuchvertices; i += 4)
            {
                indices[curIndice + 0] = curVertice + 0 + i;
                indices[curIndice + 1] = curVertice + 1 + i;
                indices[curIndice + 2] = curVertice + 2 + i;
                indices[curIndice + 3] = curVertice + 1 + i;
                indices[curIndice + 4] = curVertice + 3 + i;
                indices[curIndice + 5] = curVertice + 2 + i;
                curIndice += 6;
            }



            float hhh = height;
            float www = mazeWall.Yend - mazeWall.Ystart + mazeWall.Xend - mazeWall.Xstart;

            ////This stuff is for repeating the texture
            for (int i = curVertice; i < curVertice + howmuchvertices; i++)
            {
                var vert = vertices[i];
                vert.TextureCoordinate.X *= (www / 2f);
                vert.TextureCoordinate.Y *= (hhh / height);
                vertices[i] = vert;
            }

            curVertice += howmuchvertices;


        }

        //public void Update(GameTime gameTime)
        //{

        //}

        //public void Draw(Matrix parentMatrix, BasicEffect effect)
        //{
        //    effect.World = parentMatrix * myMatrix;

        //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Apply();
        //        game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, 8, indices, 0, 4);
        //    }
        //}
    }
}
