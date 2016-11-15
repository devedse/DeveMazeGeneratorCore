using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.InnerMaps;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using DeveMazeGenerator.Imageification;
using DeveMazeGenerator.PathFinders;
using System.Diagnostics;
using System;

namespace DeveMazeGeneratorWeb.Controllers
{
    [Route("api/[controller]")]
    public class MazesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("Maze/{width}/{height}", Name = "GenerateMaze")]
        public FileStreamResult GenerateMaze(int width, int height)
        {
            var alg = new AlgorithmDivisionDynamic();

            var map = alg.Generate<UndefinedInnerMap, NetRandom>(width, height, null);

            var memoryStream = new MemoryStream();
            WithoutPath.MazeToImage(map, memoryStream);
            return new FileStreamResult(memoryStream, new MediaTypeHeaderValue("image/png"));
        }

        [HttpGet("MazePath/{width}/{height}", Name = "GenerateMazeWithPath")]
        public FileStreamResult GenerateMazeWithPath(int width, int height)
        {
            var alg = new AlgorithmBacktrack();

            var w = Stopwatch.StartNew();
            var map = alg.Generate<BitArreintjeFastInnerMap, NetRandom>(width, height, null);
            var mazeGenerationTime = w.Elapsed;

            w.Restart();
            var path = PathFinderDepthFirstSmartWithPos.GoFind(map, null);
            var pathGenerationTime = w.Elapsed;

            w.Restart();
            var memoryStream = new MemoryStream();
            WithPath.SaveMazeAsImageDeluxePng(map, path, memoryStream);
            var toImageTime = w.Elapsed;

            Console.WriteLine($"Maze generation time: {mazeGenerationTime}, Path find time: {pathGenerationTime}, To image time: {toImageTime}");
            return new FileStreamResult(memoryStream, new MediaTypeHeaderValue("image/png"));
        }
    }
}
