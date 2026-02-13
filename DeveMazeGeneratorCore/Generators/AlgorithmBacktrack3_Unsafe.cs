using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmBacktrack3_Unsafe : IAlgorithm<Maze>
    {
        public Maze GoGenerate<M, TAction>(int width, int height, int seed, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback)
            where M : InnerMap
            where TAction : struct, IProgressAction
        {
            var innerMap = mapFactory.Create(width, height);
            var random = randomFactory.Create(seed);

            // Special handling for our unsafe map
            if (innerMap is BitArreintjeFastInnerMapUnsafe unsafeMap)
            {
                return GoGenerateInternalUnsafe(unsafeMap, random, pixelChangedCallback);
            }
            
            // Fallback (or throw, but let's try to be nice)
            // But since I don't want to duplicate the safe logic here, I'll just throw 
            // because the user selected the "Unsafe" algorithm implying they want speed.
            throw new InvalidOperationException($"AlgorithmBacktrack3_Unsafe requires BitArreintjeFastInnerMapUnsafe, but got {innerMap.GetType().Name}");
        }

        private unsafe Maze GoGenerateInternalUnsafe<TAction>(BitArreintjeFastInnerMapUnsafe map, IRandom random, TAction pixelChangedCallback)
            where TAction : struct, IProgressAction
        {
            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L);
            long currentStep = 1;

            int width = map.Width - 1;
            int height = map.Height - 1;
            int heightLongs = map._heightLongs;

            // Pin the array
            fixed (long* internalDataPtr = map._innerData)
            {
                var stackje = new Stack<MazePoint>();
                stackje.Push(new MazePoint(1, 1));
                
                // Set (1, 1) to true
                // map[1, 1] = true;
                // Manual set:
                int indexInitial = 1 * heightLongs + (1 / 64); // x=1 is 2nd column
                long maskInitial = 1L << 1;
                internalDataPtr[indexInitial] |= maskInitial;


                pixelChangedCallback.Invoke(1, 1, currentStep, totSteps);

                while (stackje.Count != 0)
                {
                    MazePoint cur = stackje.Peek();
                    int curX = cur.X;
                    int curY = cur.Y;

                    // Check neighbors
                    // We need to check if neighbor is within bounds (e.g. > 0) AND if it is NOT visited (value is 0/false)
                    // !map[x, y] means the bit is 0.

                    // Left: x - 2
                    bool validLeft = curX - 2 > 0;
                    if (validLeft)
                    {
                        // Check map
                         // x is curX - 2
                        int idx = (curX - 2) * heightLongs + (curY / 64);
                        long valIsSet = internalDataPtr[idx] & (1L << curY);
                        validLeft = (valIsSet == 0);
                    }

                    // Right: x + 2
                    bool validRight = curX + 2 < width;
                    if (validRight)
                    {
                        int idx = (curX + 2) * heightLongs + (curY / 64);
                        long valIsSet = internalDataPtr[idx] & (1L << curY);
                        validRight = (valIsSet == 0);
                    }

                    // Up: y - 2
                    bool validUp = curY - 2 > 0;
                    if (validUp)
                    {
                         // x is curX. y is curY - 2
                        int idx = curX * heightLongs + ((curY - 2) / 64);
                        long valIsSet = internalDataPtr[idx] & (1L << (curY - 2));
                        validUp = (valIsSet == 0);
                    }

                    // Down: y + 2
                    bool validDown = curY + 2 < height;
                    if (validDown)
                    {
                        int idx = curX * heightLongs + ((curY + 2) / 64);
                        long valIsSet = internalDataPtr[idx] & (1L << (curY + 2));
                        validDown = (valIsSet == 0);
                    }

                    int validLeftByte = Unsafe.As<bool, byte>(ref validLeft);
                    int validRightByte = Unsafe.As<bool, byte>(ref validRight);
                    int validUpByte = Unsafe.As<bool, byte>(ref validUp);
                    int validDownByte = Unsafe.As<bool, byte>(ref validDown);

                    int targetCount = validLeftByte + validRightByte + validUpByte + validDownByte;

                    if (targetCount == 0)
                    {
                        stackje.Pop();
                    }
                    else
                    {
                        var chosenDirection = random.Next(targetCount);
                        int countertje = 0;

                        // Same logic to determine direction
                        bool actuallyGoingLeft = validLeft & chosenDirection == countertje;
                        byte actuallyGoingLeftByte = Unsafe.As<bool, byte>(ref actuallyGoingLeft);
                        countertje += validLeftByte;

                        bool actuallyGoingRight = validRight & chosenDirection == countertje;
                        byte actuallyGoingRightByte = Unsafe.As<bool, byte>(ref actuallyGoingRight);
                        countertje += validRightByte;

                        bool actuallyGoingUp = validUp & chosenDirection == countertje;
                        byte actuallyGoingUpByte = Unsafe.As<bool, byte>(ref actuallyGoingUp);
                        countertje += validUpByte;

                        bool actuallyGoingDown = validDown & chosenDirection == countertje;
                        byte actuallyGoingDownByte = Unsafe.As<bool, byte>(ref actuallyGoingDown);

                        var nextX = curX + actuallyGoingLeftByte * -2 + actuallyGoingRightByte * 2;
                        var nextY = curY + actuallyGoingUpByte * -2 + actuallyGoingDownByte * 2;

                        var nextXInBetween = curX - actuallyGoingLeftByte + actuallyGoingRightByte;
                        var nextYInBetween = curY - actuallyGoingUpByte + actuallyGoingDownByte;

                        stackje.Push(new MazePoint(nextX, nextY));
                        
                        // map[nextXInBetween, nextYInBetween] = true;
                        int idxTween = nextXInBetween * heightLongs + (nextYInBetween / 64);
                        internalDataPtr[idxTween] |= (1L << nextYInBetween);

                        // map[nextX, nextY] = true;
                        int idxNext = nextX * heightLongs + (nextY / 64);
                        internalDataPtr[idxNext] |= (1L << nextY);

                        pixelChangedCallback.Invoke(nextXInBetween, nextYInBetween, currentStep, totSteps);
                        pixelChangedCallback.Invoke(nextX, nextY, currentStep, totSteps);
                    }
                }
            }

            return new Maze(map);
        }
    }
}
