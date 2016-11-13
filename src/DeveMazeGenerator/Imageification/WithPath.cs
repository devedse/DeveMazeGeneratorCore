using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using ImageSharp;
using System.Collections.Generic;
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

            var image = new Image(map.Width - 1, map.Height - 1);
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
                        pixels[x, y] = new Color((byte)r, (byte)g, (byte)b);
                    }
                    //lineSavingProgress(y, this.Height - 2);
                }
            }
            image.SaveAsPng(stream);
        }
    }
}
