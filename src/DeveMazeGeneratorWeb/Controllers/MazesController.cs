using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using ImageProcessorCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            var map = new BitArreintjeFastInnerMap(width, height);
            var alg = new AlgorithmBacktrack();

            alg.Generate(map, null);

            var memoryStream = new MemoryStream();
            MazeToImage(map, memoryStream);
            return new FileStreamResult(memoryStream, new MediaTypeHeaderValue("image/png"));
        }

        public static void MazeToImage(InnerMap map, Stream stream)
        {
            var image = new Image(map.Width - 1, map.Height - 1);
            using (var pixels = image.Lock())
            {
                for (int y = 0; y < map.Height - 1; y++)
                {
                    for (int x = 0; x < map.Width - 1; x++)
                    {
                        var wall = map[x, y];

                        if (!wall)
                        {
                            pixels[x, y] = Color.Black;
                        }
                        else
                        {
                            pixels[x, y] = Color.White;
                        }
                    }
                }
            }

            image.SaveAsPng(stream);
        }
    }
}
