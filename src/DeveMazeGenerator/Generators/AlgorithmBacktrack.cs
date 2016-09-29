using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.InnerMaps;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmBacktrack : Algorithm
    {
        public void Generate(InnerMap map, Action<int, int, long, long> pixelChangedCallback)
        {
            //if (pixelChangedCallback == null)
            //{
            //    pixelChangedCallback = (x, y, z, u) => { };
            //}

            GoGenerate(map, new FastRandom(), pixelChangedCallback);
        }

        public void Generate(InnerMap map, int seed, Action<int, int, long, long> pixelChangedCallback)
        {
            //if (pixelChangedCallback == null)
            //{
            //    pixelChangedCallback = (x, y, z, u) => { };
            //}

            GoGenerate(map, new FastRandom(seed), pixelChangedCallback);
        }

        private void GoGenerate(InnerMap map, FastRandom r, Action<int, int, long, long> pixelChangedCallback)
        {
            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L);
            long currentStep = 1;

            int width = map.Width;
            int height = map.Height;
            int x = 1;
            int y = 1;

            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(x, y));
            map[x, y] = true;

            if (pixelChangedCallback != null)
                pixelChangedCallback.Invoke(x, y, currentStep, totSteps);

            MazePoint[] targets = new MazePoint[4];

            while (stackje.Count != 0)
            {
                MazePoint cur = stackje.Peek();
                x = cur.X;
                y = cur.Y;

                int targetCount = 0;
                if (x - 2 > 0 && !map[x - 2, y])
                {
                    targets[targetCount].X = x - 2;
                    targets[targetCount].Y = y;
                    targetCount++;
                }
                if (x + 2 < width - 1 && !map[x + 2, y])
                {
                    targets[targetCount].X = x + 2;
                    targets[targetCount].Y = y;
                    targetCount++;
                }
                if (y - 2 > 0 && !map[x, y - 2])
                {
                    targets[targetCount].X = x;
                    targets[targetCount].Y = y - 2;
                    targetCount++;
                }
                if (y + 2 < height - 1 && !map[x, y + 2])
                {
                    targets[targetCount].X = x;
                    targets[targetCount].Y = y + 2;
                    targetCount++;
                }

                if (targetCount > 0)
                {
                    var target = targets[r.Next(targetCount)];
                    stackje.Push(target);
                    map[target.X, target.Y] = true;

                    currentStep++;

                    if (target.X < x)
                    {
                        map[x - 1, y] = true;

                        if (pixelChangedCallback != null)
                            pixelChangedCallback.Invoke(x - 1, y, currentStep, totSteps);
                    }
                    else if (target.X > x)
                    {
                        map[x + 1, y] = true;

                        if (pixelChangedCallback != null)
                            pixelChangedCallback.Invoke(x + 1, y, currentStep, totSteps);
                    }
                    else if (target.Y < y)
                    {
                        map[x, y - 1] = true;

                        if (pixelChangedCallback != null)
                            pixelChangedCallback.Invoke(x, y - 1, currentStep, totSteps);
                    }
                    else if (target.Y > y)
                    {
                        map[x, y + 1] = true;

                        if (pixelChangedCallback != null)
                            pixelChangedCallback.Invoke(x, y + 1, currentStep, totSteps);
                    }

                    if (pixelChangedCallback != null)
                        pixelChangedCallback.Invoke(target.X, target.Y, currentStep, totSteps);
                }
                else
                {
                    stackje.Pop();
                }
            }
        }
    }
}
