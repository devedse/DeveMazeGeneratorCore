using DeveMazeGenerator.Factories;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Generators.SpeedOptimization;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmBacktrack2Deluxe : Algorithm
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

            int width = map.Width - 1;
            int height = map.Height - 1;

            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(1, 1));
            map[1, 1] = true;

            pixelChangedCallback.Invoke(1, 1, currentStep, totSteps);

            while (stackje.Count != 0)
            {
                MazePoint cur = stackje.Peek();

                bool validLeft = cur.X - 2 > 0 && !map[cur.X - 2, cur.Y];
                bool validRight = cur.X + 2 < width && !map[cur.X + 2, cur.Y];
                bool validUp = cur.Y - 2 > 0 && !map[cur.X, cur.Y - 2];
                bool validDown = cur.Y + 2 < height && !map[cur.X, cur.Y + 2];

                byte validLeftByte = Unsafe.As<bool, byte>(ref validLeft);
                byte validRightByte = Unsafe.As<bool, byte>(ref validRight);
                byte validUpByte = Unsafe.As<bool, byte>(ref validUp);
                byte validDownByte = Unsafe.As<bool, byte>(ref validDown);

                int targetCount = validLeftByte + validRightByte + validUpByte + validDownByte;

                if (targetCount == 0)
                {
                    stackje.Pop();
                }
                else
                {
                    var chosenDirection = random.Next(targetCount);
                    int countertje = 0;

                    bool actuallyGoingLeft = validLeft && chosenDirection == countertje++;
                    bool actuallyGoingRight = validRight && chosenDirection == countertje++;
                    bool actuallyGoingUp = validUp && chosenDirection == countertje++;
                    bool actuallyGoingDown = validDown && chosenDirection == countertje;

                    byte actuallyGoingLeftByte = Unsafe.As<bool, byte>(ref actuallyGoingLeft);
                    byte actuallyGoingRightByte = Unsafe.As<bool, byte>(ref actuallyGoingRight);
                    byte actuallyGoingUpByte = Unsafe.As<bool, byte>(ref actuallyGoingUp);
                    byte actuallyGoingDownByte = Unsafe.As<bool, byte>(ref actuallyGoingDown);

                    var nextX = cur.X + actuallyGoingLeftByte * -2 + actuallyGoingRightByte * 2;
                    var nextY = cur.Y + actuallyGoingUpByte * -2 + actuallyGoingDownByte * 2;

                    var nextXInBetween = cur.X - actuallyGoingLeftByte + actuallyGoingRightByte;
                    var nextYInBetween = cur.Y - actuallyGoingUpByte + actuallyGoingDownByte;

                    stackje.Push(new MazePoint(nextX, nextY));
                    map[nextXInBetween, nextYInBetween] = true;
                    map[nextX, nextY] = true;

                    pixelChangedCallback.Invoke(nextXInBetween, nextYInBetween, currentStep, totSteps);
                    pixelChangedCallback.Invoke(nextX, nextY, currentStep, totSteps);
                }
            }

            return map;
        }
    }
}
