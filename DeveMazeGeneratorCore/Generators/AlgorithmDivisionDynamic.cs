using System;
using System.Collections.Generic;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmDivisionDynamic : IAlgorithm<Maze>
    {
        private const int tilesCached = 20;
        private const int tileSize = 64;

        public Maze GoGenerate<M, TAction>(int width, int height, int seed, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback)
           where M : InnerMap
           where TAction : struct, IProgressAction
        {
            Func<int, int, int, int, M> generateAction = (x, y, widthPart, heightPart) => GenerateMapPart(x, y, width, height, widthPart, heightPart, mapFactory, randomFactory, seed);
            Action<InnerMap> storeAction = (x) => { };

            var gridSize = Math.Min(Math.Min(width, height), tileSize);
            var totalMap = new CachedInnerMap2<M>(tilesCached, gridSize, generateAction, storeAction);
            return new Maze(new ForwardingInnerMap(width, height, (x, y) => totalMap.GetMapPoint(x, y, t => t)));
        }

        private M GenerateMapPart<M>(int xStart, int yStart, int width, int height, int widthPart, int heightPart, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, int seed) where M : InnerMap
        {
            var random = randomFactory.Create(seed);

            //InnerMap map = new BitArreintjeFastInnerMap(widthPart, heightPart) { StartX = xStart, StartY = yStart };
            var map = mapFactory.Create(widthPart, heightPart, xStart, yStart);

            //If the maze is out of screen
            var theRightEdge = Math.Max(xStart + widthPart - width, 0);
            var theBottomEdge = Math.Max(yStart + heightPart - height, 0);

            map.FillMap(true);

            //Add walls
            if (xStart == 0)
            {
                for (int y = 0; y < heightPart - theBottomEdge; y++)
                {
                    map[0, y] = false;
                }
            }

            if (yStart == 0)
            {
                for (int x = 0; x < widthPart - theRightEdge; x++)
                {
                    map[x, 0] = false;
                }
            }

            if (xStart + widthPart >= width)
            {
                for (int y = 0; y < heightPart - theBottomEdge; y++)
                {
                    map[widthPart - 1 - theRightEdge, y] = false;
                }

                if (UnevenHelper.NumberIsEven(width))
                {
                    for (int y = 0; y < heightPart - theBottomEdge; y++)
                    {
                        map[widthPart - 2 - theRightEdge, y] = false;
                    }
                }
            }

            if (yStart + heightPart >= height)
            {
                for (int x = 0; x < widthPart - theRightEdge; x++)
                {
                    map[x, heightPart - 1 - theBottomEdge] = false;
                }

                if (UnevenHelper.NumberIsEven(height))
                {
                    for (int x = 0; x < widthPart - theRightEdge; x++)
                    {
                        map[x, heightPart - 2 - theBottomEdge] = false;
                    }
                }
            }


            var visibleRectangle = new Rectangle(xStart, yStart, widthPart, heightPart, 0);

            var rectangles = new Stack<Rectangle>();


            var startRect = new Rectangle(0, 0, UnevenHelper.MakeUneven(width), UnevenHelper.MakeUneven(height), random.Next());
            rectangles.Push(startRect);

            while (rectangles.Count > 0)
            {
                var curRect = rectangles.Pop();

                //Console.WriteLine($"X: {curRect.X} Y: {curRect.Y} Width: {curRect.Width} Height: {curRect.Height}");

                //random.Reinitialise(curRect.Seed);
                random = randomFactory.Create(curRect.Seed);

                bool horizontalSplit = true;

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
                    int opening = 1 + random.Next(curRect.Width / 2) * 2 + curRect.X;

                    var rect1 = new Rectangle(curRect.X, curRect.Y, curRect.Width, splitnumber + 1, random.Next());
                    var rect2 = new Rectangle(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber, random.Next());

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

                    if (IsValidRect(visibleRectangle, rect1))
                    {
                        rectangles.Push(rect1);
                    }
                    if (IsValidRect(visibleRectangle, rect2))
                    {
                        rectangles.Push(rect2);
                    }
                }
                else
                {
                    int splitnumber = 2 + random.Next((curRect.Width - 2) / 2) * 2;
                    int opening = 1 + random.Next(curRect.Height / 2) * 2 + curRect.Y;

                    var rect1 = new Rectangle(curRect.X, curRect.Y, splitnumber + 1, curRect.Height, random.Next());
                    var rect2 = new Rectangle(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height, random.Next());

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

                    if (IsValidRect(visibleRectangle, rect1))
                    {
                        rectangles.Push(rect1);
                    }
                    if (IsValidRect(visibleRectangle, rect2))
                    {
                        rectangles.Push(rect2);
                    }
                }
            }

            return map;
        }

        private bool IsValidRect(Rectangle visibleRectangle, Rectangle curRect)
        {
            return curRect.Width > 3 && curRect.Height > 3 && visibleRectangle.IntersectsWith(curRect);
        }
    }
}
