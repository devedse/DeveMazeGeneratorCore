﻿using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmBacktrack4 : IAlgorithm<Maze>
    {
        public Maze GoGenerate<M, TAction>(int width, int height, int seed, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback)
            where M : InnerMap
            where TAction : struct, IProgressAction
        {
            var innerMap = mapFactory.Create(width, height);
            var random = randomFactory.Create(seed);

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

                if (validDirections == MazeDir.NoDirections)
                {
                    stackje.Pop();
                }
                else if ((validDirections & MazeDir.L) == MazeDir.L)
                {
                    if ((validDirections & MazeDir.D) == MazeDir.D)
                    {
                        if (validDirections == MazeDir.LD)
                        {
                            if (random.Next(2) == 0)
                            {
                                SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                            else
                            {
                                SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                        }
                        else if (validDirections == MazeDir.RDL)
                        {
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
                        }
                        else
                        {
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
                        }
                    }
                    else
                    {
                        if (validDirections == MazeDir.L)
                        {
                            SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else if (validDirections == MazeDir.LU)
                        {
                            if (random.Next(2) == 0)
                            {
                                SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                            else
                            {
                                SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                        }
                        else if (validDirections == MazeDir.LR)
                        {
                            if (random.Next(2) == 0)
                            {
                                SetMap<MazeDirGenericLeft, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                            else
                            {
                                SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                            }
                        }
                        else
                        {
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
                        }
                    }
                }
                else if ((validDirections & MazeDir.R) == MazeDir.R)
                {
                    if (validDirections == MazeDir.R)
                    {
                        SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                    }
                    else if (validDirections == MazeDir.RU)
                    {
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                    }
                    else if (validDirections == MazeDir.RD)
                    {
                        if (random.Next(2) == 0)
                        {
                            SetMap<MazeDirGenericRight, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                        else
                        {
                            SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                        }
                    }
                    else if (validDirections == MazeDir.URD)
                    {
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
                    }
                }
                else if (validDirections == MazeDir.U)
                {
                    SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                }
                else if (validDirections == MazeDir.D)
                {
                    SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                }
                else
                {
                    if (random.Next(2) == 0)
                    {
                        SetMap<MazeDirGenericUp, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                    }
                    else
                    {
                        SetMap<MazeDirGenericDown, TAction>(stackje, map, pixelChangedCallback, totSteps, currentStep, cur.X, cur.Y);
                    }
                }
            }
            return new Maze(map);
        }

    }
}
