using DeveMazeGenerator.Generators;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Helpers;
using DeveMazeGenerator.InnerMaps;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{width}/{height}", Name = "GenerateMaze")]
        public FileStreamResult GenerateMaze(int width, int height)
        {
            var alg = new AlgorithmDivisionDynamic();

            var map = alg.Generate<UndefinedInnerMap, NetRandom>(width, height, null);

            var memoryStream = new MemoryStream();
            MazeImager.MazeToImage(map, memoryStream);
            return new FileStreamResult(memoryStream, new MediaTypeHeaderValue("image/png"));
        }
    }
}
