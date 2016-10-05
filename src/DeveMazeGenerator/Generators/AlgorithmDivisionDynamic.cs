using DeveMazeGenerator.InnerMaps;
using System;
using System.Collections.Generic;
namespace DeveMazeGenerator.Generators
{
    public class AlgorithmDivisionDynamic
    {
        private int width;
        private int height;
        private int seed;

        public AlgorithmDivisionDynamic(int width, int height, int seed)
        {
            this.width = width;
            this.height = height;
            this.seed = seed;
        }

        public InnerMap GenerateMapPart(int xStart, int yStart, int widthPart, int heightPart)
        {
            var map = new BitArreintjeFastInnerMap(widthPart, heightPart);

            for (int y = 0; y < heightPart; y++)
            {
                for (int x = 0; x < widthPart; x++)
                {
                    map[x, y] = true;
                }
            }

            //Add walls
            if (xStart == 0)
            {
                for (int y = 0; y < heightPart; y++)
                {
                    map[0, y] = false;
                }
            }

            if (yStart == 0)
            {
                for (int x = 0; x < widthPart; x++)
                {
                    map[x, 0] = false;
                }
            }

            if (xStart + widthPart == width)
            {
                for (int y = 0; y < heightPart; y++)
                {
                    map[widthPart - 1, y] = false;
                }

                if (NumberIsEven(width))
                {
                    for (int y = 0; y < heightPart; y++)
                    {
                        map[widthPart - 2, y] = false;
                    }
                }
            }

            if (yStart + heightPart == height)
            {
                for (int x = 0; x < widthPart; x++)
                {
                    map[x, heightPart - 1] = false;
                }

                if (NumberIsEven(height))
                {
                    for (int x = 0; x < widthPart; x++)
                    {
                        map[x, heightPart - 2] = false;
                    }
                }
            }


            var visibleRectangle = new Rectangle(xStart, yStart, widthPart, heightPart, 0);

            var rectangles = new Stack<Rectangle>();


            var startRect = new Rectangle(0, 0, MakeUneven(width), MakeUneven(height), seed);
            rectangles.Push(startRect);

            while (rectangles.Count > 0)
            {
                var curRect = rectangles.Pop();

                if (curRect.IntersectsWith(visibleRectangle))
                {
                    //DrawRectToMap(curRect, map);


                    if (curRect.Width > 3 && curRect.Height > 3)
                    {
                        var random = new Random(curRect.Seed);

                        Boolean horizontalSplit = true;
                        //form.drawRectangle(curRect.X, curRect.Y, curRect.Width, curRect.Height, Brushes.Pink);

                        if (curRect.Width > curRect.Height)
                        {
                            horizontalSplit = false;
                        }
                        else if (curRect.Width < curRect.Height)
                        {
                            horizontalSplit = true;
                        }
                        else
                        {
                            if (random.Next(2) == 0)
                            {
                                horizontalSplit = false;
                            }
                        }

                        if (horizontalSplit)
                        {
                            int splitnumber = 2 + random.Next((curRect.Height - 2) / 2) * 2;
                            int opening = 1 + random.Next((curRect.Width) / 2) * 2 + curRect.X;

                            Rectangle rect1 = new Rectangle(curRect.X, curRect.Y, curRect.Width, splitnumber + 1, random.Next());
                            Rectangle rect2 = new Rectangle(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber, random.Next());


                            int xStartDraw = Math.Max(0, curRect.X - xStart);
                            int xEndDraw = Math.Min(widthPart, curRect.X - xStart + curRect.Width);

                            int yPos = curRect.Y + splitnumber - yStart;

                            if (yPos >= 0 && yPos < heightPart - 1)
                            {
                                for (int i = xStartDraw; i < xEndDraw; i++)
                                {
                                    if (i != opening - xStart)
                                    {
                                        map[i, yPos] = false;
                                    }
                                }
                            }


                            rectangles.Push(rect1);
                            rectangles.Push(rect2);
                        }
                        else
                        {
                            int splitnumber = 2 + random.Next((curRect.Width - 2) / 2) * 2;
                            int opening = 1 + random.Next((curRect.Height) / 2) * 2 + curRect.Y;

                            Rectangle rect1 = new Rectangle(curRect.X, curRect.Y, splitnumber + 1, curRect.Height, random.Next());
                            Rectangle rect2 = new Rectangle(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height, random.Next());


                            var yStartDraw = Math.Max(0, curRect.Y - yStart);
                            int yEndDraw = Math.Min(heightPart, curRect.Y - yStart + curRect.Height);

                            int xPos = curRect.X + splitnumber - xStart;

                            if (xPos >= 0 && xPos < widthPart - 1)
                            {
                                for (int i = yStartDraw; i < yEndDraw; i++)
                                {
                                    if (i != opening - yStart)
                                    {
                                        map[xPos, i] = false;
                                    }
                                }
                            }


                            rectangles.Push(rect1);
                            rectangles.Push(rect2);
                        }
                    }

                }
            }

            return map;
        }

        private bool NumberIsEven(int number)
        {
            return number % 2 == 0;
        }

        private int MakeUneven(int number)
        {
            if (NumberIsEven(number))
            {
                return number - 1;
            }
            return number;
        }

        private void DrawRectToMap(Rectangle curRect, BitArreintjeFastInnerMap map)
        {
            for (int x = curRect.X; x < curRect.Width; x++)
            {
                map[x, curRect.Y] = false;
                map[x, curRect.Y + curRect.Height - 1] = false;
            }

            for (int y = curRect.Y + 1; y < curRect.Height - 1; y++)
            {
                map[curRect.X, y] = false;
                map[curRect.X + curRect.Width - 1, y] = false;
            }
        }
    }
}
