using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DeveMazeGenerator.Imageification
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
            using (var pixels = image.Lock())
            {
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
                        pixels[x, y] = new Argb32((byte)r, (byte)g, (byte)b);
                    }
                    //lineSavingProgress(y, this.Height - 2);
                }
            }
            var timeForFirstImageSavePart = w.Elapsed;
            w.Restart();

            var pngEncored = new PngEncoder();

            image.Save(stream, pngEncored, new PngEncoderOptions() { CompressionLevel = 9 });
            //image.SaveAsPng(stream);
            var timeForSaveAsPng = w.Elapsed;

            Console.WriteLine($"First image conversion time: {timeForFirstImageSavePart}, Time for saving as PNG: {timeForSaveAsPng}");
        }
    }
}
