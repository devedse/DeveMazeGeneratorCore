﻿using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGeneratorCore.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MazesController : ControllerBase
    {
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
            var alg = new AlgorithmDivisionDynamic();

            var map = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(width, height, null);

            using (var memoryStream = new MemoryStream())
            {
                WithoutPath.MazeToImage(map, memoryStream);

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("MazePath/{width}/{height}", Name = "GenerateMazeWithPath")]
        public ActionResult GenerateMazeWithPath(int width, int height)
        {
            var alg = new AlgorithmBacktrack();

            var w = Stopwatch.StartNew();
            var map = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(width, height, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            var path = PathFinderDepthFirstSmartWithPos.GoFind(map, null);
            var pathGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePng(map, path, memoryStream);
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
            var map = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(width, height, seed, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            var path = PathFinderDepthFirstSmartWithPos.GoFind(map, null);
            var pathGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePng(map, path, memoryStream);
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
            var map = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(width, height, seed, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            using (var memoryStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePngWithParts(map, map.PathData, xPart, yPart, partWidth, partHeight, memoryStream);
                var toImageTime = w.Elapsed;

                Console.WriteLine($"Maze generation time: {mazeGenerationTime} To image time: {toImageTime}");

                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }
    }
}
