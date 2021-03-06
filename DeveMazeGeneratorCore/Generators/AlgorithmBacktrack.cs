﻿using System;
using System.Collections.Generic;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmBacktrack : Algorithm
    {
        public override InnerMap GoGenerate<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            var innerMap = mapFactory.Create();
            var random = randomFactory.Create();

            return GoGenerateInternal(innerMap, random, pixelChangedCallback);
        }

        private InnerMap GoGenerateInternal(InnerMap map, IRandom random, Action<int, int, long, long> pixelChangedCallback)
        {
            if (pixelChangedCallback == null)
            {
                pixelChangedCallback = (vvv, yyy, zzz, www) => { };
            }

            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L);
            long currentStep = 1;

            int width = map.Width - 1;
            int height = map.Height - 1;
            int x = 1;
            int y = 1;

            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(x, y));
            map[x, y] = true;

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
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = x - 2;
                    curTarget.Y = y;
                    targetCount++;
                }
                if (x + 2 < width && !map[x + 2, y])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = x + 2;
                    curTarget.Y = y;
                    targetCount++;
                }
                if (y - 2 > 0 && !map[x, y - 2])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = x;
                    curTarget.Y = y - 2;
                    targetCount++;
                }
                if (y + 2 < height && !map[x, y + 2])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = x;
                    curTarget.Y = y + 2;
                    targetCount++;
                }

                if (targetCount > 0)
                {
                    var target = targets[random.Next(targetCount)];
                    stackje.Push(target);
                    map[target.X, target.Y] = true;

                    currentStep++;

                    if (target.X < x)
                    {
                        map[x - 1, y] = true;
                        pixelChangedCallback.Invoke(x - 1, y, currentStep, totSteps);
                    }
                    else if (target.X > x)
                    {
                        map[x + 1, y] = true;
                        pixelChangedCallback.Invoke(x + 1, y, currentStep, totSteps);
                    }
                    else if (target.Y < y)
                    {
                        map[x, y - 1] = true;
                        pixelChangedCallback.Invoke(x, y - 1, currentStep, totSteps);
                    }
                    else if (target.Y > y)
                    {
                        map[x, y + 1] = true;

                        pixelChangedCallback.Invoke(x, y + 1, currentStep, totSteps);
                    }

                    pixelChangedCallback.Invoke(target.X, target.Y, currentStep, totSteps);
                }
                else
                {
                    stackje.Pop();
                }
            }

            return map;
        }
    }
}
