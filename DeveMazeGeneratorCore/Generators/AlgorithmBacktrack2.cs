using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;
using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmBacktrack2 : IAlgorithm<Maze>
    {
        public Maze GoGenerate<M, TAction>(int width, int height, int seed, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback)
          where M : InnerMap
          where TAction : struct, IProgressAction
        {
            var innerMap = mapFactory.Create(width, height);
            var random = randomFactory.Create(seed);

            return GoGenerateInternal(innerMap, random, pixelChangedCallback);
        }

        private Maze GoGenerateInternal<M, TAction>(M map, IRandom random, TAction pixelChangedCallback) where M : InnerMap where TAction : struct, IProgressAction
        {
            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L);
            long currentStep = 1;

            int width = map.Width - 1;
            int height = map.Height - 1;

            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(1, 1));
            map[1, 1] = true;

            pixelChangedCallback.Invoke(1, 1, currentStep, totSteps);

            MazePoint[] targets = new MazePoint[4];
            //Span<MazePoint> targets = stackalloc MazePoint[4];

            while (stackje.Count != 0)
            {
                MazePoint cur = stackje.Peek();

                int targetCount = 0;
                if (cur.X - 2 > 0 && !map[cur.X - 2, cur.Y])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = cur.X - 2;
                    curTarget.Y = cur.Y;
                    targetCount++;
                }
                if (cur.X + 2 < width && !map[cur.X + 2, cur.Y])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = cur.X + 2;
                    curTarget.Y = cur.Y;
                    targetCount++;
                }
                if (cur.Y - 2 > 0 && !map[cur.X, cur.Y - 2])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = cur.X;
                    curTarget.Y = cur.Y - 2;
                    targetCount++;
                }
                if (cur.Y + 2 < height && !map[cur.X, cur.Y + 2])
                {
                    ref var curTarget = ref targets[targetCount];
                    curTarget.X = cur.X;
                    curTarget.Y = cur.Y + 2;
                    targetCount++;
                }

                if (targetCount > 0)
                {
                    var target = targets[random.Next(targetCount)];
                    stackje.Push(target);
                    map[target.X, target.Y] = true;

                    currentStep++;

                    if (target.X < cur.X)
                    {
                        map[cur.X - 1, cur.Y] = true;
                        pixelChangedCallback.Invoke(cur.X - 1, cur.Y, currentStep, totSteps);
                    }
                    else if (target.X > cur.X)
                    {
                        map[cur.X + 1, cur.Y] = true;
                        pixelChangedCallback.Invoke(cur.X + 1, cur.Y, currentStep, totSteps);
                    }
                    else if (target.Y < cur.Y)
                    {
                        map[cur.X, cur.Y - 1] = true;
                        pixelChangedCallback.Invoke(cur.X, cur.Y - 1, currentStep, totSteps);
                    }
                    else if (target.Y > cur.Y)
                    {
                        map[cur.X, cur.Y + 1] = true;

                        pixelChangedCallback.Invoke(cur.X, cur.Y + 1, currentStep, totSteps);
                    }

                    pixelChangedCallback.Invoke(target.X, target.Y, currentStep, totSteps);
                }
                else
                {
                    stackje.Pop();
                }
            }

            return new Maze(map);
        }
    }
}
