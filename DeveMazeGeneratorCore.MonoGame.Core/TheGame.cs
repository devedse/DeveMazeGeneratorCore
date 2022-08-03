﻿#region Using Statements
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.MonoGame.Core;
using DeveMazeGeneratorCore.MonoGame.Core.Data;
using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using DeveMazeGeneratorMonoGame.LineOfSight;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
#endregion

namespace DeveMazeGeneratorMonoGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TheGame : Game
    {
        public Platform Platform { get; }

        private IntSize? _desiredScreenSize = null;
        private IContentManagerExtension _contentManagerExtension = null;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Camera camera;
        private Basic3dExampleCamera newcamera;

        private BasicEffect effect;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private VertexBuffer vertexBufferPath;
        private IndexBuffer indexBufferPath;

        private int curMazeWidth = 32;
        private int curMazeHeight = 32;
        private int wallsCount = 0;
        private int pathCount = 0;

        private Maze currentMaze = null;
        private List<MazePointPos> currentPath = null;


        //Keys.H
        private bool drawRoof = true;

        //Keys.L
        private bool lighting = true;

        //Keys.P
        private bool drawPath = false;

        private float numbertje = -1f;

        private int speedFactor = 2;

        private Random random = new Random();

        private PlayerModel playerModel;

        //Keys.T
        private bool fromAboveCamera = false;

        //Keys.F
        private bool followCamera = true;

        //Keys.C
        private Boolean chaseCamera = false;
        //Keys.B
        private Boolean chaseCameraShowDebugBlocks = false;
        private LineOfSightDeterminer determiner;
        private LineOfSightObject curChaseCameraPoint = null;

        //Keys.O
        private bool UseNewCamera = false;

        private int skyboxSize = 1000000;
        private CubeModelInvertedForSkybox skyboxModel;
        private CubeModel groundModel;
        private CubeModel roofModel;
        private CubeModel startModel;
        private CubeModel finishModel;
        private CubeModel possibleCubejeModel;
        private CubeModel losPointCubeModel;



        public bool AllowMouseResets { get; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        private List<IAlgorithm<Maze>> algorithms = new List<IAlgorithm<Maze>>()
        {
            new AlgorithmBacktrack2Deluxe2(),
            new AlgorithmDivisionDynamic(),
            new AlgorithmKruskal()
        };
        private int currentAlgorithm = 0;

        public TheGame(IContentManagerExtension contentManagerExtension, IntSize? desiredScreenSize, Platform platform) : base()
        {
            _contentManagerExtension = contentManagerExtension;
            _desiredScreenSize = desiredScreenSize;
            Platform = platform;

            AllowMouseResets = Platform != Platform.Blazor;
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferMultiSampling = true;
            //GraphicsDevice.PresentationParameters.MultiSampleCount = 16;

            IsMouseVisible = true;

            //TargetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)500);


            //This is required for Blazor since it loads assets in a custom way
            Content = new ExtendibleContentManager(this.Services, _contentManagerExtension);
            Content.RootDirectory = "Content";
        }

        public TheGame() : this(null, null, Platform.Desktop)
        {

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if !BLAZOR
            // TODO: Add your initialization logic here
            if (true)
            {
                graphics.PreferredBackBufferWidth = 3000;
                graphics.PreferredBackBufferHeight = 1400;
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
            }

            this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000d / 240);

            Window.AllowUserResizing = true;
            graphics.ApplyChanges();
#endif

            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window_ClientSizeChanged(null, null);

            Activated += TheGame_Activated;
            base.Initialize();
        }

        private void TheGame_Activated(object sender, EventArgs e)
        {
            //Console.WriteLine($"{DateTime.Now}: ACTIVATED");
            ResetMouseToCenter();
        }

        public void ResetMouseToCenter()
        {
            if (AllowMouseResets)
            {
                Mouse.SetPosition(ScreenWidth / 2, ScreenHeight / 2);
            }
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            ScreenWidth = Window.ClientBounds.Width;
            ScreenHeight = Window.ClientBounds.Height;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GenerateMaze();
            camera = new Camera(this);
            newcamera = new Basic3dExampleCamera(GraphicsDevice, Window, this);
            newcamera.Position = new Vector3(7.5f, 7.5f, 7.5f);
            newcamera.LookAtDirection = Vector3.Forward;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice);

            ContentDing.GoLoadContent(GraphicsDevice, Content);

            playerModel = new PlayerModel(this);

            skyboxModel = new CubeModelInvertedForSkybox(this, skyboxSize, skyboxSize, skyboxSize, TexturePosInfoGenerator.FullImage);

            startModel = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);
            finishModel = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);

            possibleCubejeModel = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);
            losPointCubeModel = new CubeModel(this, 0.75f, 0.75f, 0.75f, TexturePosInfoGenerator.FullImage, 0.75f);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void GenerateMaze()
        {
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();

            groundModel?.Dispose();
            groundModel = new CubeModel(this, curMazeWidth - 2, 0.1f, curMazeHeight - 2, TexturePosInfoGenerator.FullImage, 2f / 3f);
            roofModel?.Dispose();
            roofModel = new CubeModel(this, curMazeWidth - 2, 0.1f, curMazeHeight - 2, TexturePosInfoGenerator.FullImage, 2f / 3f);


            var alg = algorithms[currentAlgorithm];
            //int randomnumber = curMazeWidth < 2048 ? random.Next(3) : random.Next(2);
            //if (randomnumber == 0)
            //    alg = new AlgorithmBacktrack();
            //else if (randomnumber == 1)
            //    alg = new AlgorithmDivision();
            //else
            //    alg = new AlgorithmKruskal();

            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();

            currentMaze = alg.GoGenerate(curMazeWidth, curMazeHeight, Environment.TickCount, innerMapFactory, randomFactory, new NoAction());
            var walls = currentMaze.InnerMap.GenerateListOfMazeWalls();
            currentPath = PathFinderDepthFirstSmartWithPos.GoFind(currentMaze.InnerMap, null);

            determiner = new LineOfSightDeterminer(currentMaze.InnerMap, currentPath);
            curChaseCameraPoint = null;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[walls.Count * 8];
            int[] indices = new int[walls.Count * 12];

            int curVertice = 0;
            int curIndice = 0;



            foreach (var wall in walls)
            {
                //int factorHeight = 10;
                //int factorWidth = 10;

                WallModel model = new WallModel(wall);

                model.GoGenerateVertices(vertices, indices, ref curVertice, ref curIndice);

            }

            wallsCount = walls.Count;

            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            if (Platform == Platform.Blazor)
            {
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                if (indices.Any(t => t > short.MaxValue))
                {
                    throw new InvalidOperationException("Could not use a maze this big due to the indices being too high");
                }
                var indicesConverted = indices.Select(t => (short)t).ToArray();
                indexBuffer.SetData(indicesConverted);
            }
            else
            {
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);
            }

            GeneratePath(currentPath);
        }

        public void GeneratePath(List<MazePointPos> path)
        {
            vertexBufferPath?.Dispose();
            indexBufferPath?.Dispose();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[path.Count * 4];
            int[] indices = new int[path.Count * 6];

            int curVertice = 0;
            int curIndice = 0;



            foreach (var pathNode in path)
            {
                //int factorHeight = 10;
                //int factorWidth = 10;

                VierkantjeModel model = new VierkantjeModel();

                model.GoGenerateVertices(pathNode.X, pathNode.Y, vertices, indices, ref curVertice, ref curIndice);

            }

            pathCount = path.Count;

            vertexBufferPath = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBufferPath.SetData(vertices);


            if (Platform == Platform.Blazor)
            {
                indexBufferPath = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                if (indices.Any(t => t > short.MaxValue))
                {
                    throw new InvalidOperationException("Could not use a maze this big due to the indices for the path being too high");
                }
                var indicesConverted = indices.Select(t => (short)t).ToArray();
                indexBufferPath.SetData(indicesConverted);
            }
            else
            {
                indexBufferPath = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBufferPath.SetData(indices);
            }
        }

        public Vector2 GetPosAtThisNumer(float number)
        {
            number -= 1.0f;

            number *= (float)speedFactor;

            number = Math.Max(0, number);

            int cur = (int)number;
            int next = cur + 1;

            var curPoint = currentPath[Math.Min(cur, currentPath.Count - 1)];
            var nextPoint = currentPath[Math.Min(next, currentPath.Count - 1)];

            var curPointVector = new Vector2(curPoint.X, curPoint.Y);
            var nextPointVector = new Vector2(nextPoint.X, nextPoint.Y);

            float rest = number - cur;

            var retval = curPointVector + ((nextPointVector - curPointVector) * rest);
            return retval;
        }

        public MazePointPos GetPosAtThisNumerMazePoint(float number)
        {
            number -= 1.0f;

            number *= (float)speedFactor;

            number = Math.Max(0, number);

            int cur = (int)number;

            var curPoint = currentPath[Math.Min(cur, currentPath.Count - 1)];

            return curPoint;
        }


        public static void Meassure(Action a)
        {
            var s = Stopwatch.StartNew();
            a();
            Debug.WriteLine("Elapsed: " + s.Elapsed.TotalSeconds);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputDing.PreUpdate();

            if (InputDing.CurKey.IsKeyDown(Keys.Escape) && Platform != Platform.Blazor)
            {
                Exit();
            }

            //Reset when done
            if ((numbertje * speedFactor) > pathCount + speedFactor)
            {
                numbertje = 0;
                GenerateMaze();
            }


            if (InputDing.KeyDownUp(Keys.Up))
            {
                numbertje = 0;
                curMazeWidth *= 2;
                curMazeHeight *= 2;
                GenerateMaze();
            }

            if (InputDing.KeyDownUp(Keys.Down))
            {
                if (curMazeWidth > 4 && curMazeHeight > 4)
                {
                    numbertje = 0;
                    curMazeWidth /= 2;
                    curMazeHeight /= 2;
                    if (curMazeWidth < 1)
                        curMazeWidth = 1;
                    if (curMazeHeight < 1)
                        curMazeHeight = 1;
                    GenerateMaze();
                }
            }

            if (InputDing.KeyDownUp(Keys.Left))
            {
                currentAlgorithm--;
                if (currentAlgorithm < 0)
                {
                    currentAlgorithm = algorithms.Count - 1;
                }
                numbertje = 0;
                GenerateMaze();
            }
            if (InputDing.KeyDownUp(Keys.Right))
            {
                currentAlgorithm++;
                if (currentAlgorithm >= algorithms.Count)
                {
                    currentAlgorithm = 0;
                }
                numbertje = 0;
                GenerateMaze();
            }

            if (InputDing.CurKey.IsKeyDown(Keys.D0))
            {
                GenerateMaze();
            }

            if (InputDing.KeyDownUp(Keys.H))
            {
                drawRoof = !drawRoof;
            }

            if (InputDing.KeyDownUp(Keys.L))
            {
                lighting = !lighting;
            }

            if (InputDing.KeyDownUp(Keys.P))
            {
                drawPath = !drawPath;
            }


            if (InputDing.CurKey.IsKeyDown(Keys.G))
            {
                numbertje = 0;
            }

            if (InputDing.KeyDownUp(Keys.R))
            {
                numbertje = 0;
                GenerateMaze();
            }

            if (InputDing.KeyDownUp(Keys.OemPlus) || InputDing.KeyDownUp(Keys.Add))
            {
                speedFactor *= 2;
                numbertje = (numbertje - 1f) / 2f + 1f;

                if (speedFactor <= 0)
                {
                    numbertje = 0;
                    speedFactor = 1;
                }
            }

            if (InputDing.KeyDownUp(Keys.OemMinus) || InputDing.KeyDownUp(Keys.Subtract))
            {
                if (speedFactor >= 2)
                {
                    speedFactor /= 2;
                    numbertje = (numbertje - 1f) * 2f + 1f;
                }
            }


            if (InputDing.KeyDownUp(Keys.Enter) && (InputDing.CurKey.IsKeyDown(Keys.LeftAlt) || InputDing.CurKey.IsKeyDown(Keys.RightAlt)))
            {
                graphics.ToggleFullScreen();
            }


            if (InputDing.KeyDownUp(Keys.O))
            {
                UseNewCamera = !UseNewCamera;
            }


            if (UseNewCamera)
            {
                newcamera.Update(gameTime);
            }
            else
            {
                camera.Update(gameTime);
            }



            //Line of sight stuff
            //Should happen when player runs out of range

            if (InputDing.KeyDownUp(Keys.C))
            {
                if (chaseCamera == false)
                {
                    fromAboveCamera = false;
                    followCamera = false;
                }
                chaseCamera = !chaseCamera;
            }

            if (InputDing.KeyDownUp(Keys.B))
            {
                chaseCameraShowDebugBlocks = !chaseCameraShowDebugBlocks;
            }

            if (chaseCamera || chaseCameraShowDebugBlocks)
            {
                if (curChaseCameraPoint == null)
                {
                    curChaseCameraPoint = determiner.GetNextLosObject();
                }

                if (curChaseCameraPoint != null)
                {
                    var curmazepoint = GetPosAtThisNumerMazePoint(numbertje);
                    var curposnumber = currentPath.IndexOf(curmazepoint);

                    while (true)
                    {
                        if (!curChaseCameraPoint.LosPoints.Any(t => currentPath.Skip(curposnumber).Any(z => t.X == z.X && t.Y == z.Y)))
                        {
                            curChaseCameraPoint = determiner.GetNextLosObject();
                            if (curChaseCameraPoint == null)
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


                if (InputDing.KeyDownUp(Keys.Enter))
                {
                    curChaseCameraPoint = determiner.GetNextLosObject();
                }
            }

            if (chaseCamera && curChaseCameraPoint != null)
            {
                camera.cameraPosition = new Vector3(curChaseCameraPoint.CameraPoint.X * 10.0f, 7.5f, curChaseCameraPoint.CameraPoint.Y * 10.0f);

                var playerPos = GetPosAtThisNumer(numbertje);


                var newRot = (float)Math.Atan2(playerPos.Y - curChaseCameraPoint.CameraPoint.Y, playerPos.X - curChaseCameraPoint.CameraPoint.X) * -1f - (MathHelper.Pi / 2.0f);

                camera.updownRot = 0;
                camera.leftrightRot = newRot;
                camera.UpdateViewMatrix();
            }






            if (InputDing.KeyDownUp(Keys.T))
            {
                if (!fromAboveCamera)
                {
                    chaseCamera = false;
                    followCamera = false;
                    drawRoof = false;

                    camera.leftrightRot = 0.15f;
                    camera.updownRot = -0.72f;
                }
                fromAboveCamera = !fromAboveCamera;
            }


            if (fromAboveCamera)
            {
                //float prenumbertje = numbertje - (float)gameTime.ElapsedGameTime.TotalSeconds;

                //var pre = GetPosAtThisNumer(prenumbertje);
                var now = GetPosAtThisNumer(numbertje);

                camera.cameraPosition = new Vector3((now.X + 1.9f) * 10.0f, (7.0f) * 10.0f, (now.Y + 7.0f) * 10.0f);
                camera.UpdateViewMatrix();
            }









            if (InputDing.KeyDownUp(Keys.F))
            {
                if (!followCamera)
                {
                    fromAboveCamera = false;
                    chaseCamera = false;
                }
                followCamera = !followCamera;
            }

            if (followCamera)
            {
                var pospos = GetPosAtThisNumer(numbertje);
                var posposbefore = GetPosAtThisNumer(numbertje - (0.5f / (float)speedFactor));
                var posposnext = GetPosAtThisNumer(Math.Max(numbertje + (0.5f / (float)speedFactor), 1.1f));
                var pospos3d = new Vector3(pospos.X * 10.0f, 7.5f, pospos.Y * 10.0f);

                camera.cameraPosition = pospos3d;

                camera.updownRot = 0;

                var oldRot = camera.leftrightRot;
                var newRot = (float)Math.Atan2(posposnext.Y - posposbefore.Y, posposnext.X - posposbefore.X) * -1f - (MathHelper.Pi / 2.0f);

                //camera.leftrightRot = (9.0f * oldRot + 1.0f * newRot) / 10.0f;
                camera.leftrightRot = newRot;
                camera.UpdateViewMatrix();
            }




            numbertje += (float)gameTime.ElapsedGameTime.TotalSeconds;
            InputDing.AfterUpdate();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SamplerState newSamplerState = new SamplerState()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                Filter = TextureFilter.Point
            };
            GraphicsDevice.SamplerStates[0] = newSamplerState;


            //GraphicsDevice.BlendState = BlendState.Opaque;

            //DepthStencilState d = new DepthStencilState();
            //d.DepthBufferEnable = true;
            //GraphicsDevice.DepthStencilState = d;

            Matrix worldMatrix = Matrix.Identity;

            if (UseNewCamera)
            {
                effect.World = worldMatrix;
                effect.View = newcamera.View;
                effect.Projection = newcamera.Projection;
            }
            else
            {
                effect.World = worldMatrix;
                effect.View = camera.viewMatrix;
                effect.Projection = camera.projectionMatrix;
            }


            //effect.EnableDefaultLighting();
            effect.LightingEnabled = true;
            effect.EmissiveColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 0.1f;

            effect.AmbientLightColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
            effect.DirectionalLight0.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;



            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.CullCounterClockwiseFace;
            state.MultiSampleAntiAlias = true;
            //state.DepthBias = 0.01f;
            GraphicsDevice.RasterizerState = state;



            //Skybox
            effect.LightingEnabled = false;
            effect.Texture = ContentDing.skyTexture1;
            Matrix skyboxMatrix = Matrix.CreateTranslation(camera.cameraPosition) * Matrix.CreateTranslation(new Vector3(-skyboxSize / 2, -skyboxSize / 2, -skyboxSize / 2));
            skyboxModel.Draw(skyboxMatrix, effect);


            effect.LightingEnabled = false;

            //Ground
            int mazeScale = 10;
            Matrix scaleMatrix = Matrix.CreateScale(mazeScale);
            Matrix growingScaleMatrix = scaleMatrix * Matrix.CreateScale(1, (float)Math.Max(Math.Min(numbertje / 1.0f, 1), 0), 1);
            effect.World = scaleMatrix;
            effect.Texture = ContentDing.win98FloorTexture;
            groundModel.Draw(Matrix.CreateTranslation(0, -0.1f, 0) * scaleMatrix, effect);

            //Roof
            if (drawRoof)
            {
                effect.Texture = ContentDing.win98RoofTexture;
                roofModel.Draw(Matrix.CreateTranslation(0, 4f / 3f, 0) * scaleMatrix, effect);
            }

            effect.LightingEnabled = lighting;

            //Start
            effect.Texture = ContentDing.startTexture;
            startModel.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * growingScaleMatrix, effect);


            //Finish
            effect.Texture = ContentDing.endTexture;
            finishModel.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * Matrix.CreateTranslation(curMazeWidth - 4, 0, curMazeHeight - 4) * growingScaleMatrix, effect);


            //Me
            if (!followCamera)
            {

                var vvv = GetPosAtThisNumer(numbertje);

                var vvvv2 = new Vector3(vvv.X * 10f, 0, vvv.Y * 10f);

                //var translationmatrix = Matrix.CreateTranslation(vvvv2);

                //effect.Texture = ContentDing.redTexture;
                //targetCamera.Draw(translationmatrix, effect);


                var pospos = GetPosAtThisNumer(numbertje - (1f / (float)speedFactor));
                var posposnext = GetPosAtThisNumer(Math.Max(numbertje + (1f / (float)speedFactor), 1.1f));

                var newRot = (float)Math.Atan2(posposnext.Y - pospos.Y, posposnext.X - pospos.X) * -1f + (MathHelper.Pi / 2.0f);


                Matrix totMatrix = MatrixExtensions.CreateRotationY(new Vector3(4, 0, 2), newRot);
                totMatrix *= Matrix.CreateTranslation(-4, 12, -2); //Put him in the middle of a tile
                totMatrix *= Matrix.CreateScale(1f / 3f);
                totMatrix *= Matrix.CreateTranslation(vvvv2);


                var posposHeadTurn = GetPosAtThisNumer(numbertje);
                var posposnextHeadTurn = GetPosAtThisNumer(Math.Max(numbertje + (2f / (float)speedFactor), 1.1f));
                var newHeadTurn = (float)Math.Atan2(posposnextHeadTurn.Y - posposHeadTurn.Y, posposnextHeadTurn.X - posposHeadTurn.X) * -1f + (MathHelper.Pi / 2.0f);


                effect.Texture = ContentDing.minecraftTexture;


                playerModel.Draw(totMatrix, effect, numbertje / 2.0f * speedFactor, newHeadTurn - newRot);
            }


            //Maze
            effect.World = growingScaleMatrix;

            if (vertexBuffer != null && indexBuffer != null)
            {
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.SetVertexBuffer(vertexBuffer);

                effect.Texture = ContentDing.win98WallTexture;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
                }

            }

            effect.World = scaleMatrix * Matrix.CreateTranslation(0, 0.5f, 0); //Put it slightly above ground level

            //Path
            if (drawPath && vertexBufferPath != null && vertexBufferPath != null)
            {
                GraphicsDevice.Indices = indexBufferPath;
                GraphicsDevice.SetVertexBuffer(vertexBufferPath);

                effect.Texture = ContentDing.win98LegoTexture;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBufferPath.VertexCount, 0, indexBufferPath.IndexCount / 3);
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBufferPath.IndexCount / 3);
                }

            }



            //Draw line of sight
            if (chaseCameraShowDebugBlocks && curChaseCameraPoint != null)
            {
                effect.Texture = ContentDing.redTexture;
                possibleCubejeModel.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * Matrix.CreateTranslation(curChaseCameraPoint.CameraPoint.X - 1, 0, curChaseCameraPoint.CameraPoint.Y - 1) * growingScaleMatrix, effect);

                if (curChaseCameraPoint.LosPoints != null)
                {
                    effect.Texture = ContentDing.startTexture;
                    foreach (var losPoint in curChaseCameraPoint.LosPoints)
                    {
                        losPointCubeModel.Draw(Matrix.CreateTranslation(0.625f, 0.375f, 0.625f) * Matrix.CreateTranslation(losPoint.X - 1, 0, losPoint.Y - 1) * growingScaleMatrix, effect);
                    }
                }
            }


            spriteBatch.Begin();

            string stringToDraw = $"Size: {curMazeWidth}, Walls: {wallsCount}, Path length: {pathCount}, Speed: {speedFactor}, Current: {(int)Math.Max((numbertje - 1f) * speedFactor, 0)}, Algorithm: ({currentAlgorithm}: {algorithms[currentAlgorithm].GetType().Name})";
            var meassured = ContentDing.spriteFont.MeasureString(stringToDraw);
            spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle(5, 5, (int)meassured.X + 10, (int)meassured.Y + 10), Color.White);
            spriteBatch.DrawString(ContentDing.spriteFont, stringToDraw, new Vector2(10, 10), Color.White);

            var n = Environment.NewLine;
            string helpStringToDraw = $"{ScreenWidth}x{ScreenHeight}{n}{n}F: Follow Camera ({followCamera}){n}T: Top Camera ({fromAboveCamera}){n}C: Chase Camera ({chaseCamera}){n}   B: Chase Debug ({chaseCameraShowDebugBlocks}){n}{n}H: Roof ({drawRoof}){n}P: Path ({drawPath}){n}{n}Down/Up: Maze Size{n}Left/Right: Algorithm{n}Num-+: Speed{n}R: New Maze{n}G: Restart this maze{n}{n}L: Lighting ({lighting}){n}O: Other Camera ({UseNewCamera})";
            var meassuredHelpString = ContentDing.spriteFont.MeasureString(helpStringToDraw);
            spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle(ScreenWidth - (int)meassuredHelpString.X - 30, 5, (int)meassuredHelpString.X + 20, (int)meassuredHelpString.Y + 10), Color.White);
            spriteBatch.DrawString(ContentDing.spriteFont, helpStringToDraw, new Vector2(ScreenWidth - (int)meassuredHelpString.X - 20, 10), Color.White);

            spriteBatch.End();


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}