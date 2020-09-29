using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmBacktrack3 : Algorithm
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMap<T, TAction>(Stack<MazePoint> stackje, InnerMap map, TAction pixelChangedCallback, long totSteps, long currentStep, int x, int y) where T : struct, IMazeDirGeneric where TAction : struct, IProgressAction
        {
            if (typeof(T) == typeof(MazeDirGenericLeft))
            {
                stackje.Push(new MazePoint(x - 2, y));
                map[x - 1, y] = true;
                map[x - 2, y] = true;
                pixelChangedCallback.Invoke(x - 1, y, currentStep, totSteps);
                pixelChangedCallback.Invoke(x - 2, y, currentStep, totSteps);
            }
            if (typeof(T) == typeof(MazeDirGenericRight))
            {
                stackje.Push(new MazePoint(x + 2, y));
                map[x + 1, y] = true;
                map[x + 2, y] = true;
                pixelChangedCallback.Invoke(x + 1, y, currentStep, totSteps);
                pixelChangedCallback.Invoke(x + 2, y, currentStep, totSteps);
            }
            if (typeof(T) == typeof(MazeDirGenericUp))
            {
                stackje.Push(new MazePoint(x, y - 2));
                map[x, y - 1] = true;
                map[x, y - 2] = true;
                pixelChangedCallback.Invoke(x, y - 1, currentStep, totSteps);
                pixelChangedCallback.Invoke(x, y - 2, currentStep, totSteps);
            }
            if (typeof(T) == typeof(MazeDirGenericDown))
            {
                stackje.Push(new MazePoint(x, y + 2));
                map[x, y + 1] = true;
                map[x, y + 2] = true;
                pixelChangedCallback.Invoke(x, y + 1, currentStep, totSteps);
                pixelChangedCallback.Invoke(x, y + 2, currentStep, totSteps);
            }
        }

        private InnerMap GoGenerateInternal<TAction>(InnerMap map, IRandom random, TAction pixelChangedCallback) where TAction : struct, IProgressAction
        {
            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L);
            long currentStep = 1;

            int width = map.Width - 1;
            int height = map.Height - 1;

            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(1, 1));
            map[1, 1] = true;

            pixelChangedCallback.Invoke(1, 1, currentStep, totSteps);

            int randomDir = 0;

            while (stackje.Count != 0)
            {
                MazePoint cur = stackje.Peek();

                uint validDirections = MazeDir.NoDirections;


                if (cur.X - 2 > 0 && !map[cur.X - 2, cur.Y])
                {
                    validDirections |= MazeDir.L;
                }
                if (cur.X + 2 < width && !map[cur.X + 2, cur.Y])
                {
                    validDirections |= MazeDir.R;
                }
                if (cur.Y - 2 > 0 && !map[cur.X, cur.Y - 2])
                {
                    validDirections |= MazeDir.U;
                }
                if (cur.Y + 2 < height && !map[cur.X, cur.Y + 2])
                {
                    validDirections |= MazeDir.D;
                }


                switch (validDirections)
                {
                    case MazeDir.NoDirections:
                        stackje.Pop();
                        break;
                    case MazeDir.L:
                        SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        break;
                    case MazeDir.R:
                        SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        break;
                    case MazeDir.U:
                        SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        break;
                    case MazeDir.D:
                        SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        break;
                    case MazeDir.LU:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.RU:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.LD:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.RD:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.LR:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.UD:
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.LUR:
                        randomDir = random.Next(3);
                        if (randomDir == 0)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else if (randomDir == 1)
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.URD:
                        randomDir = random.Next(3);
                        if (randomDir == 0)
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else if (randomDir == 1)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.RDL:
                        randomDir = random.Next(3);
                        if (randomDir == 0)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else if (randomDir == 1)
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                    case MazeDir.DLU:
                        randomDir = random.Next(3);
                        if (randomDir == 0)
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else if (randomDir == 1)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        break;
                }
                //var target = targets[random.Next(targetCount)];
                //stackje.Push(target);
                //map[target.X, target.Y] = true;


            }

            return map;
        }
    }
}
