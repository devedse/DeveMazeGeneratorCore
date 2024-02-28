using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.PathFinders;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DeveMazeGeneratorCore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MazesController : ControllerBase
    {
        private FontCollection _fontCollection;

        //private static ConcurrentDictionary<int, byte[]> _zoomImageCache = new();

        public MazesController()
        {
            var assembly = Assembly.GetExecutingAssembly();

            _fontCollection = new();
            using Stream stream = assembly.GetManifestResourceStream("DeveMazeGeneratorCore.Web.SecularOne-Regular.ttf");
            _fontCollection.Add(stream);
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Test1", "value2" };
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Maze/{width}/{height}", Name = "GenerateMaze")]
        public ActionResult GenerateMaze(int width, int height)
        {
            var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, NetRandom>(width, height, null);

            using (var memoryStream = new MemoryStream())
            {
                WithoutPath.MazeToImage(maze.InnerMap, memoryStream);

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("MazePath/{width}/{height}", Name = "GenerateMazeWithPath")]
        public ActionResult GenerateMazeWithPath(int width, int height)
        {
            var w = Stopwatch.StartNew();
            var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, NetRandom>(width, height, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            var pathGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, path, memoryStream);
                var toImageTime = w.Elapsed;

                Console.WriteLine($"Maze generation time: {mazeGenerationTime}, Path find time: {pathGenerationTime}, To image time: {toImageTime}");

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("MazePathSeed/{seed}/{width}/{height}", Name = "GenerateMazeWithPathSeed")]
        public ActionResult GenerateMazeWithPathSeed(int seed, int width, int height)
        {
            var alg = new AlgorithmBacktrack();

            var w = Stopwatch.StartNew();
            var maze = MazeGenerator.Generate<AlgorithmBacktrack, BitArreintjeFastInnerMap, NetRandom>(width, height, seed, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            var pathGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePng(maze.InnerMap, path, memoryStream);
                var toImageTime = w.Elapsed;

                Console.WriteLine($"Maze generation time: {mazeGenerationTime}, Path find time: {pathGenerationTime}, To image time: {toImageTime}");

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("MazeDynamicPathSeedPart/{seed}/{width}/{height}/{xPart}/{yPart}/{partWidth}/{partHeight}", Name = "MazeDynamicPathSeedPart")]
        public ActionResult MazeDynamicPathSeedPart(int seed, int width, int height, int xPart, int yPart, int partWidth, int partHeight)
        {
            var alg = new AlgorithmDivisionDynamicWithPath();

            var w = Stopwatch.StartNew();
            var maze = (MazeWithPathAsInnerMap)MazeGenerator.Generate<AlgorithmDivisionDynamicWithPath, BitArreintjeFastInnerMap, NetRandom>(width, height, seed, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePngWithParts(maze.InnerMap, maze.PathMap, xPart, yPart, partWidth, partHeight, memoryStream);
                var toImageTime = w.Elapsed;

                Console.WriteLine($"Maze generation time: {mazeGenerationTime} To image time: {toImageTime}");

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("MazeDynamicOpenSeaDragon/{level}/{xPartNumber}_{yPartNumber}.png", Name = "MazeDynamicOpenSeaDragon")]
        public ActionResult MazeDynamicOpenSeaDragon(int level, int xPartNumber, int yPartNumber)
        {

            int seed = 1337;
            int xPart = xPartNumber * 256;
            int yPart = yPartNumber * 256;
            int width = 134217728;
            int height = 134217728;
            int partWidth = 256;
            int partHeight = 256;

            int deepestLevel = 27;

            if (level != deepestLevel)
            {
                var wZoomImage = Stopwatch.StartNew();

                //if (_zoomImageCache.TryGetValue(level, out var cachedImage))
                //{
                //    return File(cachedImage, "image/png");
                //}

                var image = new Image<Argb32>(partWidth, partHeight);
                image.Mutate(t => t.Fill(Color.Black));

                _fontCollection.TryGet("Secular One", out var fontFamily);
                var font = new Font(fontFamily, 26);
                var fontSmall = new Font(fontFamily, 12);

                var textOptions = new RichTextOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Origin = new Point(partWidth / 2, partHeight / 2),
                };

                var textOptionsSmall = new RichTextOptions(fontSmall)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Origin = new Point(partWidth / 2, partHeight - 10),
                }; 

                image.Mutate(x => x.DrawText(textOptions, $"X: {xPartNumber}\nY: {yPartNumber}\nLevel: {level}\n\nZoom in\nfurther", Color.White));
                image.Mutate(x => x.DrawText(textOptionsSmall, $"Maze will show at level: {deepestLevel}", Color.White));
                image.Mutate(x => x.Draw(Brushes.ForwardDiagonal(Color.Yellow, Color.Red), 2, new RectangleF(2, 2, partWidth - 5, partHeight - 5)));


                //for (int y = 0; y < partHeight; y++)
                //{
                //    for (int x = 0; x < partWidth; x++)
                //    {
                //        image[x, y] = new Argb32((byte)12, (byte)166, (byte)14);
                //    }
                //}

                using var memstream = new MemoryStream();
                image.SaveAsPng(memstream);
                var imageData = memstream.ToArray();
                //_zoomImageCache.TryAdd(level, imageData);

                wZoomImage.Stop();
                Console.WriteLine($"Zoom in further image time: {wZoomImage.Elapsed}");
                return File(imageData, "image/png");
            }


            var alg = new AlgorithmDivisionDynamicWithPath();

            var w = Stopwatch.StartNew();
            var maze = (MazeWithPathAsInnerMap)MazeGenerator.Generate<AlgorithmDivisionDynamicWithPath, BitArreintjeFastInnerMap, NetRandom>(width, height, seed, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePngWithParts(maze.InnerMap, maze.PathMap, xPart, yPart, partWidth, partHeight, memoryStream);
                var toImageTime = w.Elapsed;

                Console.WriteLine($"Maze generation time: {mazeGenerationTime} To image time: {toImageTime}");

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }
    }
}
