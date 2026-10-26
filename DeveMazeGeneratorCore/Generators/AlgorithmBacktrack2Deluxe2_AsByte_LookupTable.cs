using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DeveMazeGeneratorCore.Generators
{
    /// <summary>
    /// AlgorithmBacktrack2Deluxe2_AsByte with the direction picked from a lookup table (PR #109). Generates the same maze for the same seed.
    /// </summary>
    public class AlgorithmBacktrack2Deluxe2_AsByte_LookupTable : IAlgorithm<Maze>
    {
        // Index is (mask << 2) | k, giving the step towards the k-th valid direction in the order Left, Right, Up, Down.
        // Mask bits: 1 = Left, 2 = Right, 4 = Up, 8 = Down.
        private static ReadOnlySpan<sbyte> StepXTable => new sbyte[]
        {
             0,  0,  0,  0, // -
            -1,  0,  0,  0, // L
             1,  0,  0,  0, // R
            -1,  1,  0,  0, // L R
             0,  0,  0,  0, // U
            -1,  0,  0,  0, // L U
             1,  0,  0,  0, // R U
            -1,  1,  0,  0, // L R U
             0,  0,  0,  0, // D
            -1,  0,  0,  0, // L D
             1,  0,  0,  0, // R D
            -1,  1,  0,  0, // L R D
             0,  0,  0,  0, // U D
            -1,  0,  0,  0, // L U D
             1,  0,  0,  0, // R U D
            -1,  1,  0,  0, // L R U D
        };

        private static ReadOnlySpan<sbyte> StepYTable => new sbyte[]
        {
             0,  0,  0,  0, // -
             0,  0,  0,  0, // L
             0,  0,  0,  0, // R
             0,  0,  0,  0, // L R
            -1,  0,  0,  0, // U
             0, -1,  0,  0, // L U
             0, -1,  0,  0, // R U
             0,  0, -1,  0, // L R U
             1,  0,  0,  0, // D
             0,  1,  0,  0, // L D
             0,  1,  0,  0, // R D
             0,  0,  1,  0, // L R D
            -1,  1,  0,  0, // U D
             0, -1,  1,  0, // L U D
             0, -1,  1,  0, // R U D
             0,  0, -1,  1, // L R U D
        };

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

            ref sbyte stepXTable = ref MemoryMarshal.GetReference(StepXTable);
            ref sbyte stepYTable = ref MemoryMarshal.GetReference(StepYTable);

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

                int mask = Unsafe.As<bool, byte>(ref validLeft)
                    | (Unsafe.As<bool, byte>(ref validRight) << 1)
                    | (Unsafe.As<bool, byte>(ref validUp) << 2)
                    | (Unsafe.As<bool, byte>(ref validDown) << 3);

                if (mask == 0)
                {
                    stackje.Pop();
                }
                else
                {
                    currentStep++;
                    // Index stays below 64: mask <= 15 and Next(count) < count <= 4.
                    int index = (mask << 2) | random.Next(BitOperations.PopCount((uint)mask));
                    int stepX = Unsafe.Add(ref stepXTable, index);
                    int stepY = Unsafe.Add(ref stepYTable, index);

                    var nextXInBetween = cur.X + stepX;
                    var nextYInBetween = cur.Y + stepY;

                    var nextX = nextXInBetween + stepX;
                    var nextY = nextYInBetween + stepY;

                    stackje.Push(new MazePoint(nextX, nextY));
                    map[nextXInBetween, nextYInBetween] = true;
                    map[nextX, nextY] = true;

                    pixelChangedCallback.Invoke(nextXInBetween, nextYInBetween, currentStep, totSteps);
                    pixelChangedCallback.Invoke(nextX, nextY, currentStep, totSteps);
                }
            }


            return new Maze(map);
        }
    }
}
