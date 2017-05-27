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
        private const int tileSize = 64;

        public override (InnerMap Maze, InnerMap PathMap) GoGenerateWithPath<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            var innerMap = mapFactory.Create();

            Func<int, int, int, int, InnerMap> generateAction = (x, y, widthPart, heightPart) => GenerateMapPartWithPath(x, y, innerMap.Width, innerMap.Height, widthPart, heightPart, randomFactory).Maze;
            Action<InnerMap> storeAction = (x) => { };

            var totalMap = new CachedInnerMap(innerMap.Width, innerMap.Height, tilesCached, Math.Min(Math.Min(innerMap.Width, innerMap.Height), tileSize), generateAction, storeAction);
            return (totalMap, null);
        }

        public (InnerMap Maze, InnerMap PathMap) GenerateMapPartWithPath(int xStart, int yStart, int width, int height, int widthPart, int heightPart, IRandomFactory randomFactory)
        {
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


            var visibleRectangle = new Rectangle(xStart, yStart, widthPart, heightPart, 0);

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

                    var splitPos = new MazePointClassLinkedList(splitnumber, opening);

                    var rect1 = new RectangleWithPath(curRect.X, curRect.Y, curRect.Width, splitnumber + 1, random.Next());
                    var rect2 = new RectangleWithPath(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber, random.Next());

                    if (curRect.PathPassesThroughThis)
                    {
                        var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitnumber, curRect.MazePointLeft.Y, curRect.MazePointRight.Y);
                        DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
                        DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

                        if (pathPassesThroughOpening)
                        {
                            splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
                            pathMap[splitPos.X, splitPos.Y] = true;
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

                    var splitPos = new MazePointClassLinkedList(splitnumber, opening);

                    var rect1 = new RectangleWithPath(curRect.X, curRect.Y, splitnumber + 1, curRect.Height, random.Next());
                    var rect2 = new RectangleWithPath(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height, random.Next());

                    if (curRect.PathPassesThroughThis)
                    {
                        var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitnumber, curRect.MazePointLeft.Y, curRect.MazePointRight.Y);
                        DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
                        DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

                        if (pathPassesThroughOpening)
                        {
                            splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
                            pathMap[splitPos.X, splitPos.Y] = true;
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

                using (var fs = new FileStream("DivisionDynamicWithPath.png", FileMode.Create))
                {
                    WithPath.SaveMazeAsImageDeluxePng(map, pathMap, fs);
                }
            }

            return (map, pathMap);
        }

        //public override (InnerMap Maze, InnerMap PathMap) GoGenerateWithPath2<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        //{
        //    var map = mapFactory.Create();
        //    var random = randomFactory.Create();

        //    var pathMap = mapFactory.Create();

        //    map.FillMap(true);

        //    var startPoint = new MazePointClassLinkedList(1, 1);
        //    var curRect = new RectangleWithPath(0, 0, UnevenHelper.MakeUneven(map.Width), UnevenHelper.MakeUneven(map.Height), random.Next(), startPoint, new MazePointClassLinkedList(map.Width - 3, map.Height - 3), true);

        //    var rectangles = new Stack<RectangleWithPath>();
        //    rectangles.Push(curRect);

        //    int xStart = 0;
        //    int widthPart = map.Width - 1;
        //    int yStart = 0;
        //    int heightPart = map.Height - 1;

        //    //If the maze is out of screen
        //    var theRightEdge = Math.Max(((xStart + widthPart) - map.Width), 0);
        //    var theBottomEdge = Math.Max(((yStart + heightPart) - map.Height), 0);
        //    //Add walls
        //    if (xStart == 0)
        //    {
        //        for (int y = 0; y < heightPart - theBottomEdge; y++)
        //        {
        //            map[0, y] = false;
        //        }
        //    }

        //    if (yStart == 0)
        //    {
        //        for (int x = 0; x < widthPart - theRightEdge; x++)
        //        {
        //            map[x, 0] = false;
        //        }
        //    }

        //    if (xStart + widthPart >= map.Width)
        //    {
        //        for (int y = 0; y < heightPart - theBottomEdge; y++)
        //        {
        //            map[widthPart - 1 - theRightEdge, y] = false;
        //        }

        //        if (UnevenHelper.NumberIsEven(map.Width))
        //        {
        //            for (int y = 0; y < heightPart - theBottomEdge; y++)
        //            {
        //                map[widthPart - 2 - theRightEdge, y] = false;
        //            }
        //        }
        //    }

        //    if (yStart + heightPart >= map.Height)
        //    {
        //        for (int x = 0; x < widthPart - theRightEdge; x++)
        //        {
        //            map[x, heightPart - 1 - theBottomEdge] = false;
        //        }

        //        if (UnevenHelper.NumberIsEven(map.Height))
        //        {
        //            for (int x = 0; x < widthPart - theRightEdge; x++)
        //            {
        //                map[x, heightPart - 2 - theBottomEdge] = false;
        //            }
        //        }
        //    }

        //    while (rectangles.Count > 0)
        //    {
        //        curRect = rectangles.Pop();

        //        bool horizontalSplit = true;

        //        if (curRect.Width > curRect.Height)
        //        {
        //            horizontalSplit = false;
        //        }
        //        else if (curRect.Width < curRect.Height)
        //        {
        //            horizontalSplit = true;
        //        }
        //        else
        //        {
        //            if (random.Next(2) == 0)
        //            {
        //                horizontalSplit = false;
        //            }
        //        }




        //        if (horizontalSplit)
        //        {
        //            int splitnumber = 2 + random.Next((curRect.Height - 2) / 2) * 2;
        //            int opening = 1 + random.Next((curRect.Width) / 2) * 2 + curRect.X;

        //            var splitPos = new MazePointClassLinkedList(splitnumber, opening);

        //            var rect1 = new RectangleWithPath(curRect.X, curRect.Y, curRect.Width, splitnumber + 1, random.Next());
        //            var rect2 = new RectangleWithPath(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber, random.Next());

        //            if (curRect.PathPassesThroughThis)
        //            {
        //                var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitnumber, curRect.MazePointLeft.Y, curRect.MazePointRight.Y);
        //                DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
        //                DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

        //                if (pathPassesThroughOpening)
        //                {
        //                    splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
        //                    pathMap[splitPos.X, splitPos.Y] = true;
        //                }
        //            }

        //            int xStartDraw = Math.Max(0, curRect.X - xStart);
        //            int xEndDraw = Math.Min(widthPart, curRect.X - xStart + curRect.Width);

        //            int yPos = curRect.Y + splitnumber - yStart;

        //            if (yPos >= 0 && yPos < heightPart - 1)
        //            {
        //                for (int i = xStartDraw; i < xEndDraw; i++)
        //                {
        //                    if (i != opening - xStart)
        //                    {
        //                        map[i, yPos] = false;
        //                    }
        //                }
        //            }

        //            //if (IsValidRect(visibleRectangle, rect1))
        //            //{
        //            rectangles.Push(rect1);
        //            //}
        //            //if (IsValidRect(visibleRectangle, rect2))
        //            //{
        //            rectangles.Push(rect2);
        //            //}
        //        }
        //        else
        //        {
        //            int splitnumber = 2 + random.Next((curRect.Width - 2) / 2) * 2;
        //            int opening = 1 + random.Next((curRect.Height) / 2) * 2 + curRect.Y;

        //            var splitPos = new MazePointClassLinkedList(splitnumber, opening);

        //            var rect1 = new RectangleWithPath(curRect.X, curRect.Y, splitnumber + 1, curRect.Height, random.Next());
        //            var rect2 = new RectangleWithPath(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height, random.Next());

        //            if (curRect.PathPassesThroughThis)
        //            {
        //                var pathPassesThroughOpening = AreNumberOnTheSidesOfThisValue(splitnumber, curRect.MazePointLeft.X, curRect.MazePointRight.X);
        //                DetermineRectanglePathPassingThrough(curRect, rect1, splitPos);
        //                DetermineRectanglePathPassingThrough(curRect, rect2, splitPos);

        //                if (pathPassesThroughOpening)
        //                {
        //                    splitPos.InsertMeInBetweenTheseTwo(curRect.MazePointLeft, curRect.MazePointRight);
        //                    pathMap[splitPos.X, splitPos.Y] = true;
        //                }
        //            }

        //            var yStartDraw = Math.Max(0, curRect.Y - yStart);
        //            int yEndDraw = Math.Min(heightPart, curRect.Y - yStart + curRect.Height);

        //            int xPos = curRect.X + splitnumber - xStart;

        //            if (xPos >= 0 && xPos < widthPart - 1)
        //            {
        //                for (int i = yStartDraw; i < yEndDraw; i++)
        //                {
        //                    if (i != opening - yStart)
        //                    {
        //                        map[xPos, i] = false;
        //                    }
        //                }
        //            }

        //            //if (IsValidRect(visibleRectangle, rect1))
        //            //{
        //            rectangles.Push(rect1);
        //            //}
        //            //if (IsValidRect(visibleRectangle, rect2))
        //            //{
        //            rectangles.Push(rect2);
        //            //}
        //        }

        //        Console.WriteLine(map.ToString());

        //        using (var fs = new FileStream("DivisionDynamicWithPath.png", FileMode.Create))
        //        {
        //            WithPath.SaveMazeAsImageDeluxePng(map, pathMap, fs);
        //        }

        //    }


        //    return (map, pathMap);
        //}

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
