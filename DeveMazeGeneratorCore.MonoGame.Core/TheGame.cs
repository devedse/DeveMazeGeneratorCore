#region Using Statements
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
using System.Runtime.CompilerServices;
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

        //private VertexBuffer vertexBuffer;
        //private IndexBuffer indexBuffer;

        //private VertexBuffer vertexBufferPath;
        //private IndexBuffer indexBufferPath;

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

        //Keys.U
        private bool showUi = true;

        private float numbertje = -1f;

        private int speedFactor = 2;

        private Random random = new Random();

        private PlayerModel playerModel;

        //Keys.B
        private bool chaseCameraShowDebugBlocks = false;
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
            new AlgorithmBacktrack2Deluxe2_AsByte(),
            new AlgorithmDivisionDynamic(),
            new AlgorithmKruskal()
        };
        private int currentAlgorithm = 0;

        private readonly string _version = typeof(TheGame).Assembly.GetName().Version.ToString();

        private readonly Stopwatch _fpsMeasureStopwatch = Stopwatch.StartNew();
        private TimeSpan _drawLastFpsMeasure = TimeSpan.Zero;
        private TimeSpan _updateLastFpsMeasure = TimeSpan.Zero;
        private TimeSpan _updateTimePerFrame = TimeSpan.Zero;


        private int _updateCallsCounter = 0;
        private int _updateCallsCounterLastSecond = 0;
        private TimeSpan _updateCallsCounterLastRecordedTime = TimeSpan.Zero;
        private int _drawCallsCounter = 0;
        private int _drawCallsCounterLastSecond = 0;
        private TimeSpan _drawCallsCounterLastRecordedTime = TimeSpan.Zero;


        private Drawable3DObject<VertexPositionNormalTexture> _maze3dObject;
        private Drawable3DObject<VertexPositionNormalTexture> _path3dObject;


        public TheGame() : this(Platform.Desktop)
        {

        }

        public TheGame(Platform platform) : this(null, platform)
        {

        }

        public TheGame(IntSize? desiredScreenSize, Platform platform) : this(null, desiredScreenSize, platform)
        {
        }

        public TheGame(IContentManagerExtension contentManagerExtension, IntSize? desiredScreenSize, Platform platform) : base()
        {
            _contentManagerExtension = contentManagerExtension;
            _desiredScreenSize = desiredScreenSize;
            Platform = platform;

            AllowMouseResets = Platform != Platform.Blazor;
            graphics = new GraphicsDeviceManager(this);

            //This is bugged in MonoGame 3.8.1 and creates a white wash over everything
            graphics.PreferMultiSampling = false;
            //GraphicsDevice.PresentationParameters.MultiSampleCount = 16;

            IsMouseVisible = true;

            if (platform == Platform.Android)
            {
                showUi = false;
            }

            //This is required for Blazor since it loads assets in a custom way
            Content = new ExtendibleContentManager(this.Services, _contentManagerExtension);
            Content.RootDirectory = "Content";
        }

        private void TheGame_Activated(object sender, EventArgs e)
        {
            //Console.WriteLine($"{DateTime.Now}: ACTIVATED");
            ResetMouseToCenter();
        }


        protected override void Initialize()
        {
            graphics.SynchronizeWithVerticalRetrace = true;
            //TargetElapsedTime = TimeSpan.FromTicks(1);
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 240d);
            IsFixedTimeStep = false;

            camera = new Camera(this);
            newcamera = new Basic3dExampleCamera(GraphicsDevice, this);
            newcamera.Position = new Vector3(7.5f, 7.5f, 7.5f);
            newcamera.LookAtDirection = Vector3.Forward;

            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.OrientationChanged += Window_OrientationChanged;

#if !BLAZOR
            if (_desiredScreenSize != null)
            {
                graphics.PreferredBackBufferWidth = _desiredScreenSize.Value.Width;
                graphics.PreferredBackBufferHeight = _desiredScreenSize.Value.Height;
            }
            else if (Platform == Platform.Android)
            {
                //For android I haven't been able to find the "FULL" screen size in the AndroidActivity
                //Whenever I tried it it would only give me the size of everything excluding system bars.
                //Unless I did GetRealMetrics but then it would always grab the full size, I want this to be dynamic.
                //So the value that is actually dynamic and correct is this:
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                //graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                //graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }
#endif

            Window.AllowUserResizing = true;

            if (Platform == Platform.UWP) // && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile"
            {
                //To remove the Battery bar
                graphics.IsFullScreen = true;
            }

            graphics.ApplyChanges();

            Activated += TheGame_Activated;

            FixScreenSize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GenerateMaze();

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

        private void Window_OrientationChanged(object sender, System.EventArgs e)
        {
            FixScreenSize();
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            FixScreenSize();
        }

        private void FixScreenSize()
        {
            ScreenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            ScreenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            camera.TriggerScreenSizeChanged();
        }

        public void ResetMouseToCenter()
        {
            if (AllowMouseResets)
            {
                Mouse.SetPosition(ScreenWidth / 2, ScreenHeight / 2);
            }
        }

        public void ToggleFullScreenBetter()
        {
            if (graphics.IsFullScreen)
            {
                if (_desiredScreenSize != null)
                {
                    graphics.PreferredBackBufferWidth = _desiredScreenSize.Value.Width;
                    graphics.PreferredBackBufferHeight = _desiredScreenSize.Value.Height;
                }
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();
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
            //indexBuffer?.Dispose();
            //vertexBuffer?.Dispose();

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

            var innerMapFactory = new InnerMapFactory<BoolInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();

            currentMaze = alg.GoGenerate(curMazeWidth, curMazeHeight, Environment.TickCount, innerMapFactory, randomFactory, new NoAction());
            var walls = currentMaze.InnerMap.GenerateListOfMazeWalls();
            wallsCount = walls.Count;

            currentPath = PathFinderDepthFirstSmartWithPos.GoFind(currentMaze.InnerMap, null);

            determiner = new LineOfSightDeterminer(currentMaze.InnerMap, currentPath);
            curChaseCameraPoint = null;




            _maze3dObject?.Dispose();
            var indexList = new int[]
            {
                0, 1, 2,
                1, 3, 2,
                4, 5, 6,
                5, 7, 6
            };
            _maze3dObject = new Drawable3DObject<VertexPositionNormalTexture>(GraphicsDevice, walls.Count * 8, walls.Count * 12, 8, indexList, IndexElementSize.SixteenBits);

            foreach (var wall in walls)
            {
                WallModel model = new WallModel(wall);

                model.GoGenerateVerticesv2(_maze3dObject);
            }





            //VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[walls.Count * 8];
            //int[] indices = new int[walls.Count * 12];

            //int curVertice = 0;
            //int curIndice = 0;



            //foreach (var wall in walls)
            //{
            //    //int factorHeight = 10;
            //    //int factorWidth = 10;

            //    WallModel model = new WallModel(wall);

            //    model.GoGenerateVertices(vertices, indices, ref curVertice, ref curIndice);

            //}

            //vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            //vertexBuffer.SetData(vertices);

            //if (Platform == Platform.Blazor)
            //{
            //    indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            //    if (indices.Any(t => t > short.MaxValue))
            //    {
            //        throw new InvalidOperationException("Could not use a maze this big due to the indices being too high");
            //    }
            //    var indicesConverted = indices.Select(t => (short)t).ToArray();
            //    indexBuffer.SetData(indicesConverted);
            //}
            //else
            //{
            //    indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            //    indexBuffer.SetData(indices);
            //}

            GeneratePath(currentPath);
        }

        public void GeneratePath(List<MazePointPos> path)
        {
            //vertexBufferPath?.Dispose();
            //indexBufferPath?.Dispose();

            pathCount = path.Count;

            _path3dObject?.Dispose();
            var indexList = new int[]
            {
                0, 1, 2,
                1, 3, 2
            };
            _path3dObject = new Drawable3DObject<VertexPositionNormalTexture>(GraphicsDevice, path.Count * 4, path.Count * 6, 4, indexList, IndexElementSize.SixteenBits);

            foreach (var pathNode in path)
            {
                VierkantjeModel model = new VierkantjeModel();
                model.GoGenerateVerticesv2(pathNode.X, pathNode.Y, _path3dObject);
            }


            //VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[path.Count * 4];
            //int[] indices = new int[path.Count * 6];

            //int curVertice = 0;
            //int curIndice = 0;



            //foreach (var pathNode in path)
            //{
            //    //int factorHeight = 10;
            //    //int factorWidth = 10;

            //    VierkantjeModel model = new VierkantjeModel();

            //    model.GoGenerateVertices(pathNode.X, pathNode.Y, vertices, indices, ref curVertice, ref curIndice);

            //}

    

            //vertexBufferPath = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            //vertexBufferPath.SetData(vertices);


            //if (Platform == Platform.Blazor)
            //{
            //    indexBufferPath = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            //    if (indices.Any(t => t > short.MaxValue))
            //    {
            //        throw new InvalidOperationException("Could not use a maze this big due to the indices for the path being too high");
            //    }
            //    var indicesConverted = indices.Select(t => (short)t).ToArray();
            //    indexBufferPath.SetData(indicesConverted);
            //}
            //else
            //{
            //    indexBufferPath = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            //    indexBufferPath.SetData(indices);
            //}
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

            var curFpsMeasure = _fpsMeasureStopwatch.Elapsed;
            _updateTimePerFrame = curFpsMeasure - _updateLastFpsMeasure;
            _updateLastFpsMeasure = curFpsMeasure;

            _updateCallsCounter++;
            if (_updateCallsCounterLastRecordedTime < _fpsMeasureStopwatch.Elapsed)
            {
                _updateCallsCounterLastRecordedTime = _updateCallsCounterLastRecordedTime.Add(TimeSpan.FromSeconds(1));
                _updateCallsCounterLastSecond = _updateCallsCounter;
                _updateCallsCounter = 0;
            }


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


            if (InputDing.TouchedOrMouseClickedInRect(new Rectangle((int)(ScreenWidth / 2 - (0.1 * ScreenWidth)), (int)(ScreenHeight - (0.1 * ScreenHeight)), (int)(0.1 * ScreenWidth), (int)(0.1 * ScreenHeight + 1))) || InputDing.KeyDownUp(Keys.U))
            {
                showUi = !showUi;
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
                //graphics.PreferredBackBufferWidth = 3840;
                //graphics.PreferredBackBufferHeight = 2160;
                //graphics.ApplyChanges();
                //graphics.ToggleFullScreen();
                ToggleFullScreenBetter();
            }

            if (InputDing.KeyDownUp(Keys.V))
            {
                graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                graphics.ApplyChanges();
            }

            if (InputDing.KeyDownUp(Keys.F))
            {
                IsFixedTimeStep = !IsFixedTimeStep;
            }


            if (InputDing.KeyDownUp(Keys.O))
            {
                UseNewCamera = !UseNewCamera;
            }


            //Line of sight stuff
            //Should happen when player runs out of range

            if (InputDing.KeyDownUp(Keys.D1))
            {
                camera.ActiveCameraMode = ActiveCameraMode.FollowCamera;
            }
            else if (InputDing.KeyDownUp(Keys.D2))
            {
                camera.ActiveCameraMode = ActiveCameraMode.FreeCamera;
            }
            else if (InputDing.KeyDownUp(Keys.D3))
            {
                camera.ActiveCameraMode = ActiveCameraMode.FromAboveCamera;

                camera.leftrightRot = 0.15f;
                camera.updownRot = -0.72f;
                drawRoof = false;
            }
            else if (InputDing.KeyDownUp(Keys.D4))
            {
                camera.ActiveCameraMode = ActiveCameraMode.ChaseCamera;
            }

            if (InputDing.KeyDownUp(Keys.B))
            {
                chaseCameraShowDebugBlocks = !chaseCameraShowDebugBlocks;
            }

            if (camera.ActiveCameraMode == ActiveCameraMode.ChaseCamera || chaseCameraShowDebugBlocks)
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

            if (camera.ActiveCameraMode == ActiveCameraMode.ChaseCamera && curChaseCameraPoint != null)
            {
                camera.cameraPosition = new Vector3(curChaseCameraPoint.CameraPoint.X * 10.0f, 7.5f, curChaseCameraPoint.CameraPoint.Y * 10.0f);

                var playerPos = GetPosAtThisNumer(numbertje);


                var newRot = (float)Math.Atan2(playerPos.Y - curChaseCameraPoint.CameraPoint.Y, playerPos.X - curChaseCameraPoint.CameraPoint.X) * -1f - (MathHelper.Pi / 2.0f);

                camera.updownRot = 0;
                camera.leftrightRot = newRot;
                //camera.UpdateViewMatrix();
            }

            if (camera.ActiveCameraMode == ActiveCameraMode.FromAboveCamera)
            {
                //float prenumbertje = numbertje - (float)gameTime.ElapsedGameTime.TotalSeconds;

                //var pre = GetPosAtThisNumer(prenumbertje);
                var now = GetPosAtThisNumer(numbertje);

                camera.cameraPosition = new Vector3((now.X + 1.9f) * 10.0f, (7.0f) * 10.0f, (now.Y + 7.0f) * 10.0f);
                //camera.UpdateViewMatrix();
            }



            if (camera.ActiveCameraMode == ActiveCameraMode.FollowCamera)
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
                //camera.UpdateViewMatrix();
            }




            if (UseNewCamera)
            {
                newcamera.Update(gameTime);
            }
            else
            {
                camera.Update(gameTime);
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
            var curFpsMeasure = _fpsMeasureStopwatch.Elapsed;
            var drawTimePerFrame = curFpsMeasure - _drawLastFpsMeasure;
            _drawLastFpsMeasure = curFpsMeasure;

            _drawCallsCounter++;
            if (_drawCallsCounterLastRecordedTime < _fpsMeasureStopwatch.Elapsed)
            {
                _drawCallsCounterLastRecordedTime = _drawCallsCounterLastRecordedTime.Add(TimeSpan.FromSeconds(1));
                _drawCallsCounterLastSecond = _drawCallsCounter;
                _drawCallsCounter = 0;
            }

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
            effect.Texture = ContentDing.skyTexture;
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
            if (camera.ActiveCameraMode != ActiveCameraMode.FollowCamera)
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


                effect.Texture = ContentDing.characterTexture;


                playerModel.Draw(totMatrix, effect, numbertje / 2.0f * speedFactor, newHeadTurn - newRot);
            }


            //Maze
            effect.World = growingScaleMatrix;

            if (_maze3dObject != null)
            {
                effect.Texture = ContentDing.win98WallTexture;
                _maze3dObject.Draw(effect);
            }

            //if (vertexBuffer != null && indexBuffer != null)
            //{

            //    GraphicsDevice.Indices = indexBuffer;
            //    GraphicsDevice.SetVertexBuffer(vertexBuffer);

            //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //    {
            //        pass.Apply();
            //        //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            //        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
            //    }

            //}

            effect.World = scaleMatrix * Matrix.CreateTranslation(0, 0.5f, 0); //Put it slightly above ground level

            //Path
            if (drawPath && _path3dObject != null)
            {
                effect.Texture = ContentDing.win98LegoTexture;
                _path3dObject.Draw(effect);
            }

            //if (drawPath && vertexBufferPath != null && vertexBufferPath != null)
            //{
            //    GraphicsDevice.Indices = indexBufferPath;
            //    GraphicsDevice.SetVertexBuffer(vertexBufferPath);

            //    effect.Texture = ContentDing.win98LegoTexture;

            //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //    {
            //        pass.Apply();
            //        //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBufferPath.VertexCount, 0, indexBufferPath.IndexCount / 3);
            //        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBufferPath.IndexCount / 3);
            //    }

            //}



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
            //Duration:

            //0000285: a += Window.ClientBounds.Width + Window.ClientBounds.Height;
            //0000005: a += graphics.PreferredBackBufferWidth + graphics.PreferredBackBufferHeight;
            //0000005: a += ScreenWidth + ScreenHeight;
            //0001104: a += GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //0000032: a += GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height;
            //0000031: a += GraphicsDevice.PresentationParameters.BackBufferWidth + GraphicsDevice.PresentationParameters.BackBufferHeight;

            var a = 0;

            var w = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                a += GraphicsDevice.PresentationParameters.BackBufferWidth + GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
            w.Stop();

            var w2 = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                a += graphics.PreferredBackBufferWidth + graphics.PreferredBackBufferHeight;
            }
            w2.Stop();

            //Console.WriteLine(w.Elapsed + "   " + w2.Elapsed);




            spriteBatch.Begin();

            int defaultDistanceBetweenComponents = (int)(0.007f * ScreenHeight);
            int extraSizeForBackground = (int)(0.0035f * ScreenHeight);

            string stringToDraw = $"Size: {curMazeWidth}, Walls: {wallsCount}, Path length: {pathCount}, Speed: {speedFactor}, Current: {(int)Math.Max((numbertje - 1f) * speedFactor, 0)}, Algorithm: ({currentAlgorithm}: {algorithms[currentAlgorithm].GetType().Name})";
            var measuredTopString = ContentDing.spriteFont.MeasureString(stringToDraw);
            float maxSizeTopScreenWidth = ScreenWidth * 0.95f;
            float topStringScale = MathF.Min(maxSizeTopScreenWidth, measuredTopString.X) / measuredTopString.X;
            var measuredTopStringScaled = measuredTopString * topStringScale;
            int distanceFromTopTopString = defaultDistanceBetweenComponents;
            spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle((int)((ScreenWidth / 2) - (measuredTopStringScaled.X / 2) - extraSizeForBackground), distanceFromTopTopString, (int)(measuredTopStringScaled.X + (2 * extraSizeForBackground)), (int)measuredTopStringScaled.Y + (2 * extraSizeForBackground)), Color.White);
            spriteBatch.DrawString(ContentDing.spriteFont, stringToDraw, new Vector2((ScreenWidth / 2) - (measuredTopStringScaled.X / 2), distanceFromTopTopString + extraSizeForBackground), Color.White, 0, Vector2.Zero, topStringScale, SpriteEffects.None, 0);

            if (showUi)
            {
                var n = Environment.NewLine;

                var activeString = "<-- active";

                string helpStringToDraw =
                    $"Version: {_version}{n}" +
                    $"FPS: {Math.Round(1.0 / drawTimePerFrame.TotalSeconds, 0)}{n}" +
                    $"UpdateFPS: {Math.Round(1.0 / _updateTimePerFrame.TotalSeconds, 0)}{n}" +
                    $"Updates: {_updateCallsCounterLastSecond}, Draws: {_drawCallsCounterLastSecond}{n}" +
                    $"Vsync (V): {graphics.SynchronizeWithVerticalRetrace}{n}" +
                    $"FixedTimeStep (F): {IsFixedTimeStep}{n}" +
                    $"TargetFps: {1 / TargetElapsedTime.TotalSeconds}{n}" +
                    $"Fullscreen: {graphics.IsFullScreen}{n}" +
                    $"{ScreenWidth}x{ScreenHeight}{n}" +
                    $"{graphics.PreferredBackBufferWidth}x{graphics.PreferredBackBufferHeight}{n}" +
                    $"{Window.ClientBounds.Width}x{Window.ClientBounds.Height}{n}" +
                    $"{GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width}x{GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height} <-- Android Right One{n}" +
                    $"{GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}{n}" +
                    $"{GraphicsDevice.PresentationParameters.BackBufferWidth}x{GraphicsDevice.PresentationParameters.BackBufferHeight}{n}" +
                    $"Aspect: {GraphicsDevice.Viewport.AspectRatio}{n}" +
                    $"{n}Camera modes:{n}1: Follow Camera {(camera.ActiveCameraMode == ActiveCameraMode.FollowCamera ? activeString : "")}{n}2: Free Camera {(camera.ActiveCameraMode == ActiveCameraMode.FreeCamera ? activeString : "")}{n}3: Top Camera {(camera.ActiveCameraMode == ActiveCameraMode.FromAboveCamera ? activeString : "")}{n}4: Chase Camera {(camera.ActiveCameraMode == ActiveCameraMode.ChaseCamera ? activeString : "")}{n}   B: Chase Debug ({chaseCameraShowDebugBlocks}){n}{n}" +
                    $"U: Show UI({showUi}){n}H: Roof ({drawRoof}){n}P: Path ({drawPath}){n}{n}Down/Up: Maze Size{n}Left/Right: Algorithm{n}Num-+: Speed{n}R: New Maze{n}G: Restart this maze{n}{n}L: Lighting ({lighting}){n}O: Other Camera ({UseNewCamera})";
                //Console.WriteLine($"{DateTime.Now}: {ScreenWidth}x{ScreenHeight}  {graphics.PreferredBackBufferWidth}x{graphics.PreferredBackBufferHeight}  {Window.ClientBounds.Width}x{Window.ClientBounds.Height}  {GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width}x{GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height}  {GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}  {GraphicsDevice.PresentationParameters.BackBufferWidth}x{GraphicsDevice.PresentationParameters.BackBufferHeight}");
                var measuredHelpString = ContentDing.spriteFont.MeasureString(helpStringToDraw);

                int distanceBetweenTopStringAndHelpStringVertical = defaultDistanceBetweenComponents;
                int distanceBetweenHelpStringAndBottomVertical = defaultDistanceBetweenComponents;
                int distanceBetweenHelpStringAndRight = defaultDistanceBetweenComponents;

                float distanceFromTopHelpString = distanceFromTopTopString + measuredTopStringScaled.Y + (2 * extraSizeForBackground) + distanceBetweenTopStringAndHelpStringVertical;

                float maxWidthHelpString = ScreenWidth * 0.33f;
                float maxHeightHelpString = ScreenHeight - distanceFromTopHelpString - distanceBetweenHelpStringAndBottomVertical - (2 * extraSizeForBackground);
                float scaleHelpString = MathF.Min(maxWidthHelpString / measuredHelpString.X, maxHeightHelpString / measuredHelpString.Y);
                scaleHelpString = MathF.Min(1, scaleHelpString);

                var measuredHelpStringScaled = measuredHelpString * scaleHelpString;


                spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle(ScreenWidth - (int)measuredHelpStringScaled.X - distanceBetweenHelpStringAndRight - (2 * extraSizeForBackground), (int)distanceFromTopHelpString, (int)measuredHelpStringScaled.X + (2 * extraSizeForBackground), (int)measuredHelpStringScaled.Y + (2 * extraSizeForBackground)), Color.White);
                spriteBatch.DrawString(ContentDing.spriteFont, helpStringToDraw, new Vector2(ScreenWidth - (int)measuredHelpStringScaled.X - distanceBetweenHelpStringAndRight - (1 * extraSizeForBackground), distanceFromTopHelpString + extraSizeForBackground), Color.White, 0, Vector2.Zero, scaleHelpString, SpriteEffects.None, 0);
            }

            //Draw rectangle to click to open secret help screen
            //spriteBatch.Draw(ContentDing.semiTransparantTexture, new Rectangle((int)(ScreenWidth / 2 - (0.1 * ScreenWidth)), (int)(ScreenHeight - (0.1 * ScreenHeight)), (int)(0.1 * ScreenWidth), (int)(0.1 * ScreenHeight) + 1), Color.White);

            spriteBatch.End();


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
