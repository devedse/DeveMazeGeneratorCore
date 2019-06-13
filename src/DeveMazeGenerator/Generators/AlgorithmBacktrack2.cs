using DeveMazeGenerator.Factories;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Generators.SpeedOptimization;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using System;
using System.Collections.Generic;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmBacktrack2 : Algorithm
    {
        public override InnerMap GoGenerate<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            return null;
            //var innerMap = mapFactory.Create();
            //var random = randomFactory.Create();

            //return GoGenerateInternal(innerMap, random, pixelChangedCallback);
        }

        public InnerMap GoGenerate2<M, TAction>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback) where M : InnerMap where TAction : struct, IProgressAction
        {
            var innerMap = mapFactory.Create();
            var random = randomFactory.Create();

            return GoGenerateInternal(innerMap, random, pixelChangedCallback);
        }

        private InnerMap GoGenerateInternal<TAction>(InnerMap map, IRandom random, TAction pixelChangedCallback) where TAction : struct, IProgressAction
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
                if (x + 2 < width - 1 && !map[x + 2, y])
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
                if (y + 2 < height - 1 && !map[x, y + 2])
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
