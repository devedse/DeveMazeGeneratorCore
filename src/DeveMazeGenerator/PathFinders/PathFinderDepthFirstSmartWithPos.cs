﻿using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeveMazeGenerator.PathFinders
{
    /// <summary>
    /// This class specifically ads a position to the path it returns which can be used to more efficiently save the maze path later
    /// </summary>
    public  static class PathFinderDepthFirstSmartWithPos
    {
        /// <summary>
        /// Finds the path between the start and the endpoint in a maze
        /// </summary>
        /// <param name="map">The maze.InnerMap</param>
        /// <param name="callBack">The callback that can be used to see what the pathfinder is doing (or null), the boolean true = a new path find thingy or false when it determined that path is not correct</param>
        /// <returns>The shortest path in a list of points</returns>
        public static List<MazePointPos> GoFind(InnerMap map, Action<int, int, Boolean> callBack)
        {
            return GoFind(new MazePoint(1, 1), new MazePoint(map.Width - 3, map.Height - 3), map, callBack);
        }

        /// <summary>
        /// Finds the path between the start and the endpoint in a maze
        /// </summary>
        /// <param name="start">The start point</param>
        /// <param name="end">The end point</param>
        /// <param name="map">The maze.InnerMap</param>
        /// <param name="callBack">The callback that can be used to see what the pathfinder is doing (or null), the boolean true = a new path find thingy or false when it determined that path is not correct</param>
        /// <returns>The shortest path in a list of points</returns>
        public static List<MazePointPos> GoFind(MazePoint startBefore, MazePoint endBefore, InnerMap map, Action<int, int, Boolean> callBack)
        {
            if (callBack == null)
            {
                callBack = (x, y, z) => { };
            }

            var start = new MazePointPos(startBefore.X, startBefore.Y);
            var end = new MazePointPos(endBefore.X, endBefore.Y);

            //Callback won't work nice with this since it will find its path from back to front
            //Swap them so we don't have to reverse at the end ;)
            //MazePoint temp = start;
            //start = end;
            //end = temp;



            int width = map.Width;
            int height = map.Height;


            List<MazePointPos> stackje = new List<MazePointPos>();
            stackje.Add(start);

            MazePointPos cur = new MazePointPos();
            MazePointPos prev = new MazePointPos(-1, -1);


            var lastBackTrackDir = -1;

            while (stackje.Count != 0)
            {

                cur = stackje[stackje.Count - 1];
                var x = cur.X;
                var y = cur.Y;


                MazePointPos target = new MazePointPos(-1, -1);
                //Make sure the point was not the previous point, also make sure that if we backtracked we don't go to a direction we already went to, also make sure that the point is white
                if ((prev.X != x + 1 || prev.Y != y) && lastBackTrackDir < 0 && x + 1 < width - 1 && map[x + 1, y])
                {
                    target = new MazePointPos(x + 1, y);
                }
                else if ((prev.X != x || prev.Y != y + 1) && lastBackTrackDir < 1 && y + 1 < height - 1 && map[x, y + 1])
                {
                    target = new MazePointPos(x, y + 1);
                }
                else if ((prev.X != x - 1 || prev.Y != y) && lastBackTrackDir < 2 && x - 1 > 0 && map[x - 1, y])
                {
                    target = new MazePointPos(x - 1, y);
                }
                else if ((prev.X != x || prev.Y != y - 1) && lastBackTrackDir < 3 && y - 1 > 0 && map[x, y - 1])
                {
                    target = new MazePointPos(x, y - 1);
                }
                else
                {
                    var prepoppy = stackje[stackje.Count - 1];
                    stackje.RemoveAt(stackje.Count - 1);

                    if (stackje.Count == 0)
                    {
                        //No path found
                        break;
                    }

                    var newcur = stackje[stackje.Count - 1];

                    //Set the new previous point
                    if (stackje.Count == 1)
                    {
                        prev = new MazePointPos(-1, -1);
                    }
                    else
                    {
                        prev = stackje.ElementAt(stackje.Count - 2);
                    }

                    //Console.WriteLine("Backtracking to X: " + newcur.X + " Y: " + newcur.Y);
                    //Console.WriteLine("Setting new prev: " + prev.X + " Y: " + prev.Y);

                    callBack.Invoke(prepoppy.X, prepoppy.Y, false);

                    //Set the direction we backtracked from
                    if (prepoppy.X > newcur.X)
                    {
                        lastBackTrackDir = 0;
                    }
                    else if (prepoppy.Y > newcur.Y)
                    {
                        lastBackTrackDir = 1;
                    }
                    else if (prepoppy.X < newcur.X)
                    {
                        lastBackTrackDir = 2;
                    }
                    else if (prepoppy.Y < newcur.Y)
                    {
                        lastBackTrackDir = 3;
                    }

                    //Console.WriteLine("Lastbacktrackdir: " + lastBackTrackDir);
                    continue;

                }

                lastBackTrackDir = -1;

                //Console.WriteLine("Going to X: " + target.X + " Y: " + target.Y);

                callBack.Invoke(x, y, true);
                stackje.Add(target);

                if (target.X == end.X && target.Y == end.Y)
                {
                    //Path found
                    break;
                }

                prev = cur;

            }

            for (int i = 0; i < stackje.Count; i++)
            {
                byte formulathing = (byte)((double)i / (double)stackje.Count * 255.0);
                var currentStackjeThing = stackje[i];
                stackje[i] = new MazePointPos(currentStackjeThing.X, currentStackjeThing.Y, formulathing);
            }

            return stackje;
        }
    }
}
