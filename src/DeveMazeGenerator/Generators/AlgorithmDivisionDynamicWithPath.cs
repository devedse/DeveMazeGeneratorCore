using System;
using System.Collections.Generic;
using System.Text;
using DeveMazeGenerator.Factories;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using DeveMazeGenerator.Helpers;
using System.IO;
using DeveMazeGenerator.Imageification;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmDivisionDynamicWithPath : AlgorithmWithPath
    {
        private const int tilesCached = 20;
        private const int tileSize = 256;

        public override InnerMap GoGenerateWithPath<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            var innerMap = mapFactory.Create();

            Func<int, int, int, int, InnerMap> generateAction = (x, y, widthPart, heightPart) => GenerateMapPartWithPath(x, y, innerMap.Width, innerMap.Height, widthPart, heightPart, randomFactory);
            Action<InnerMap> storeAction = (x) => { };

            var totalMap = new CachedInnerMap(innerMap.Width, innerMap.Height, tilesCached, Math.Min(Math.Min(innerMap.Width, innerMap.Height), tileSize), generateAction, storeAction);
            return totalMap;
        }

        public InnerMap GenerateMapPartWithPath(int xStart, int yStart, int width, int height, int widthPart, int heightPart, IRandomFactory randomFactory)
        {
            var visibleRectangle = new Rectangle(xStart, yStart, widthPart, heightPart, 0);
            //Console.WriteLine($"Generating rectangle: {visibleRectangle}");

            var random = randomFactory.Create();

            InnerMap map = new BitArreintjeFastInnerMap(widthPart, heightPart) { StartX = xStart, StartY = yStart };

            //If the maze is out of screen
            var theRightEdge = Math.Max(((xStart + widthPart) - width), 0);
            var theBottomEdge = Math.Max(((yStart + heightPart) - height), 0);

            map.FillMap(true);

            var pathMap = new BitArreintjeFastInnerMap(widthPart, heightPart) { StartX = xStart, StartY = yStart };

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

            var rectangles = new Stack<RectangleWithPath>();


            var startRect = new RectangleWithPath(0, 0, UnevenHelper.MakeUneven(width), UnevenHelper.MakeUneven(height), random.Next(), new MazePointClassLinkedList(1, 1), new MazePointClassLinkedList(width - 3, height - 3), true);
            rectangles.Push(startRect);

            while (rectangles.Count > 0)
            {
                var curRect = rectangles.Pop();

                //Console.WriteLine($"X: {curRect.X} Y: {curRect.Y} Width: {curRect.Width} Height: {curRect.Height}");

                random.Reinitialise(curRect.Seed);

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
                    int opening = 1 + random.Next((curRect.Width) / 2) * 2 + curRect.X;

                    var splitPos = new MazePointClassLinkedList(opening, curRect.Y + splitnumber);

                    var rect1 = new RectangleWithPath(curRect.X, curRect.Y, curRect.Width, splitnumber + 1, random.Next());
                    var rect2 = new RectangleWithPath(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber, random.Next());

                    if (curRect.PathPassesThroughThis)
                    {
                        var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitPos.Y, curRect.MazePointLeft.Y, curRect.MazePointRight.Y);
                        DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
                        DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

                        if (pathPassesThroughOpening)
                        {
                            splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
                            if (visibleRectangle.IntersectsWith(splitPos))
                            {
                                pathMap[splitPos.X - visibleRectangle.X, splitPos.Y - visibleRectangle.Y] = true;
                            }
                        }

                        //DrawRedStuff
                        if (rect1.PathPassesThroughThis && rect1.Height == 3)
                        {
                            FillInPathForRectangleX(visibleRectangle, pathMap, rect1.MazePointLeft, rect1.MazePointRight, rect1);
                        }

                        if (rect2.PathPassesThroughThis && rect2.Height == 3)
                        {
                            //FillInPathForRectangle(visibleRectangle, pathMap, curRect.MazePointRight.Previous, curRect.MazePointRight, rect2);
                            FillInPathForRectangleX(visibleRectangle, pathMap, rect2.MazePointLeft, rect2.MazePointRight, rect2);
                        }
                    }

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
                    int opening = 1 + random.Next((curRect.Height) / 2) * 2 + curRect.Y;

                    var splitPos = new MazePointClassLinkedList(curRect.X + splitnumber, opening);

                    var rect1 = new RectangleWithPath(curRect.X, curRect.Y, splitnumber + 1, curRect.Height, random.Next());
                    var rect2 = new RectangleWithPath(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height, random.Next());

                    if (curRect.PathPassesThroughThis)
                    {
                        var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitPos.X, curRect.MazePointLeft.X, curRect.MazePointRight.X);
                        DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
                        DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

                        if (pathPassesThroughOpening)
                        {
                            splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
                            if (visibleRectangle.IntersectsWith(splitPos))
                            {
                                pathMap[splitPos.X - visibleRectangle.X, splitPos.Y - visibleRectangle.Y] = true;
                            }
                        }

                        //DrawRedStuff
                        if (rect1.PathPassesThroughThis && rect1.Width == 3)
                        {
                            FillInPathForRectangleY(visibleRectangle, pathMap, rect1.MazePointLeft, rect1.MazePointRight, rect1);
                        }

                        if (rect2.PathPassesThroughThis && rect2.Width == 3)
                        {
                            //FillInPathForRectangle(visibleRectangle, pathMap, curRect.MazePointRight.Previous, curRect.MazePointRight, rect2);
                            FillInPathForRectangleY(visibleRectangle, pathMap, rect2.MazePointLeft, rect2.MazePointRight, rect2);
                        }
                    }

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

                //using (var fs = new FileStream("DivisionDynamicWithPath2.png", FileMode.Create))
                //{
                //    WithPath.SaveMazeAsImageDeluxePng(map, pathMap, fs);
                //}
            }

            map.PathData = pathMap;
            return map;
        }

        private static void FillInPathForRectangleY(Rectangle visibleRectangle, BitArreintjeFastInnerMap pathMap, MazePointClassLinkedList mazePointToWriteFor, MazePointClassLinkedList splitPos, RectangleWithPath rect1)
        {
            var theX = rect1.X + 1 - visibleRectangle.X;

            if (theX >= 0 && theX < visibleRectangle.Width)
            {
                var startYWriting = mazePointToWriteFor.Y - visibleRectangle.Y;
                var endWritingY = splitPos.Y - visibleRectangle.Y;

                var lowest = Math.Min(startYWriting, endWritingY);
                var highest = Math.Max(startYWriting, endWritingY);

                lowest = Math.Max(lowest, 0);
                highest = Math.Min(highest, visibleRectangle.Height - 1);


                for (int i = lowest; i <= highest; i++)
                {
                    pathMap[theX, i] = true;
                }
            }
        }

        private static void FillInPathForRectangleX(Rectangle visibleRectangle, BitArreintjeFastInnerMap pathMap, MazePointClassLinkedList mazePointToWriteFor, MazePointClassLinkedList splitPos, RectangleWithPath rect1)
        {
            var theY = rect1.Y + 1 - visibleRectangle.Y;

            if (theY >= 0 && theY < visibleRectangle.Height)
            {
                var startXWriting = mazePointToWriteFor.X - visibleRectangle.X;
                var endWritingX = splitPos.X - visibleRectangle.X;

                var lowest = Math.Min(startXWriting, endWritingX);
                var highest = Math.Max(startXWriting, endWritingX);

                lowest = Math.Max(lowest, 0);
                highest = Math.Min(highest, visibleRectangle.Width - 1);


                for (int i = lowest; i <= highest; i++)
                {
                    pathMap[i, theY] = true;
                }
            }
        }

        private static void DetermineRectanglePathPassingThrough(RectangleWithPath curRect, RectangleWithPath newRect, MazePointClassLinkedList opening)
        {
            if (newRect.IntersectsWith(curRect.MazePointLeft))
            {
                newRect.MazePointLeft = curRect.MazePointLeft;
            }
            if (newRect.IntersectsWith(curRect.MazePointRight))
            {
                newRect.MazePointRight = curRect.MazePointRight;
            }

            if (newRect.MazePointLeft == null && newRect.MazePointRight == null)
            {
                return;
            }

            if (newRect.MazePointLeft == null)
            {
                newRect.MazePointLeft = opening;
            }
            else if (newRect.MazePointRight == null)
            {
                newRect.MazePointRight = opening;
            }
            newRect.PathPassesThroughThis = true;
        }

        private bool AreNumberOnTheSidesOfThisValue(int value, int numberOne, int numberTwo)
        {
            if (numberOne < value && numberTwo < value)
            {
                return false;
            }
            if (numberOne > value && numberTwo > value)
            {
                return false;
            }
            return true;
        }

        private bool IsValidRect(Rectangle visibleRectangle, Rectangle curRect)
        {
            return curRect.Width > 3 && curRect.Height > 3 && visibleRectangle.IntersectsWith(curRect);
        }
    }
}
