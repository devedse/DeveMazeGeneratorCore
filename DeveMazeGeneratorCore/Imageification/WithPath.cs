using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGeneratorCore.Imageification
{
    public static class WithPath
    {
        public static void SaveMazeAsImageDeluxePng(InnerMap map, List<MazePointPos> pathPosjes, Stream stream)
        {
            pathPosjes.Sort((first, second) =>
            {
                if (first.Y == second.Y)
                {
                    return first.X - second.X;
                }
                return first.Y - second.Y;
            });


            int curpos = 0;

            var w = Stopwatch.StartNew();
            var image = new Image<Argb32>(map.Width - 1, map.Height - 1);

            for (int y = 0; y < map.Height - 1; y++)
            {
                for (int x = 0; x < map.Width - 1; x++)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;

                    MazePointPos curPathPos;
                    if (curpos < pathPosjes.Count)
                    {
                        curPathPos = pathPosjes[curpos];
                        if (curPathPos.X == x && curPathPos.Y == y)
                        {
                            r = curPathPos.RelativePos;
                            g = 255 - curPathPos.RelativePos;
                            b = 0;
                            curpos++;
                        }
                        else if (map[x, y])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }
                    }
                    else if (map[x, y])
                    {
                        r = 255;
                        g = 255;
                        b = 255;
                    }
                    image[x, y] = new Argb32((byte)r, (byte)g, (byte)b);
                }
                //lineSavingProgress(y, this.Height - 2);
            }

            var timeForFirstImageSavePart = w.Elapsed;
            w.Restart();

            image.SaveAsPng(stream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });

            var timeForSaveAsPng = w.Elapsed;

            Debug.WriteLine($"First image conversion time: {timeForFirstImageSavePart}, Time for saving as PNG: {timeForSaveAsPng}");
        }

        public static void SaveMazeAsImageDeluxePng(InnerMap map, InnerMap pathMap, Stream stream)
        {

            var w = Stopwatch.StartNew();
            var image = new Image<Argb32>(map.Width - 1, map.Height - 1);

            for (int y = 0; y < map.Height - 1; y++)
            {
                for (int x = 0; x < map.Width - 1; x++)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;

                    if (pathMap[x, y])
                    {
                        r = 255;
                    }
                    else
                    {
                        if (map[x, y])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }
                    }
                    image[x, y] = new Argb32((byte)r, (byte)g, (byte)b);
                }
                //lineSavingProgress(y, this.Height - 2);
            }

            var timeForFirstImageSavePart = w.Elapsed;
            w.Restart();

            image.SaveAsPng(stream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });

            var timeForSaveAsPng = w.Elapsed;

            Debug.WriteLine($"First image conversion time: {timeForFirstImageSavePart}, Time for saving as PNG: {timeForSaveAsPng}");
        }

        public static void SaveMazeAsImageDeluxePngWithParts(InnerMap map, InnerMap pathMap, int xPart, int yPart, int widthPart, int heightPart, Stream stream)
        {
            var w = Stopwatch.StartNew();
            var image = new Image<Argb32>(widthPart, heightPart);

            var maxWidth = widthPart;
            var maxHeight = heightPart;


            int yInPart = yPart;
            for (int y = 0; y < maxHeight; y++)
            {
                int xInPart = xPart;
                for (int x = 0; x < maxWidth; x++)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;

                    if (pathMap[xInPart, yInPart])
                    {
                        r = 255;
                    }
                    else
                    {
                        if (map[xInPart, yInPart])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }
                    }
                    image[x, y] = new Argb32((byte)r, (byte)g, (byte)b);

                    xInPart++;
                }
                //lineSavingProgress(y, this.Height - 2);
                yInPart++;
            }

            var timeForFirstImageSavePart = w.Elapsed;
            w.Restart();

            image.SaveAsPng(stream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });

            var timeForSaveAsPng = w.Elapsed;

            Debug.WriteLine($"First image conversion time: {timeForFirstImageSavePart}, Time for saving as PNG: {timeForSaveAsPng}");
        }
    }
}
