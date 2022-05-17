using DeveMazeGeneratorCore.ExtensionMethods;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;
using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Generators
{
    public class AlgorithmKruskal : IAlgorithm<Maze>
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
            long totSteps = (map.Width - 1L) / 2L * ((map.Height - 1L) / 2L) * 2;
            long currentStep = 1;

            KruskalCell[][] theMap;

            //Prepare
            theMap = new KruskalCell[map.Width][];
            for (int x = 0; x < map.Width; x++)
            {
                theMap[x] = new KruskalCell[map.Height];
                for (int y = 0; y < map.Height; y++)
                {
                    KruskalCell c = new KruskalCell(x, y);
                    theMap[x][y] = c;

                    if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0 && x != map.Width - 1 && y != map.Height - 1)
                    {
                        currentStep++;
                        //pixelChangedCallback(x, y, currentStep, totSteps);
                        c.Solid = false;
                        c.CellSet.Add(c);
                    }
                    else
                    {
                        c.Solid = true;
                    }
                }
            }

            //Find walls and add neighbouring cells
            List<KruskalCell> walls = new List<KruskalCell>();
            for (int y = 1; y < map.Height - 2; y++)
            {
                bool horizontalwall = false;
                int startje = 1;
                if (y % 2 == 1)
                {
                    horizontalwall = true;
                    startje = 2;
                }
                for (int x = startje; x < map.Width - 2; x = x + 2)
                {
                    KruskalCell ccc = theMap[x][y];
                    ccc.Solid = true;
                    walls.Add(ccc);
                    ccc.CellSet.Clear();
                    if (horizontalwall)
                    {
                        //form.pixelDraw(x, y, Brushes.Blue);
                        ccc.CellSet.Add(theMap[x - 1][y]);
                        ccc.CellSet.Add(theMap[x + 1][y]);
                    }
                    else
                    {
                        //form.pixelDraw(x, y, Brushes.Yellow);
                        ccc.CellSet.Add(theMap[x][y - 1]);
                        ccc.CellSet.Add(theMap[x][y + 1]);
                    }
                }
            }

            walls = walls.RandomPermutation(random);
            int cur = 0;
            foreach (KruskalCell wall in walls)
            {
                cur++;

                KruskalCell cell1 = wall.CellSet[0];
                KruskalCell cell2 = wall.CellSet[1];
                if (!cell1.CellSet.Equals(cell2.CellSet))
                {
                    wall.Solid = false;
                    currentStep++;
                    pixelChangedCallback.Invoke(wall.X, wall.Y, currentStep, totSteps);
                    List<KruskalCell> l1 = cell1.CellSet;
                    List<KruskalCell> l2 = cell2.CellSet;

                    if (l1.Count > l2.Count)
                    {
                        l1.AddRange(l2);
                        foreach (KruskalCell c in l2)
                        {
                            c.CellSet = l1;
                        }
                    }
                    else
                    {
                        l2.AddRange(l1);
                        foreach (KruskalCell c in l1)
                        {
                            c.CellSet = l2;
                        }
                    }
                }
            }

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var solid = theMap[x][y].Solid;
                    if (solid)
                    {
                        map[x, y] = false;
                    }
                    else
                    {
                        map[x, y] = true;
                    }

                }
            }

            return new Maze(map);
        }

        private static bool isValid(int x, int y, InnerMap map)
        {
            //Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (x > 0 && x < map.Width - 1 && y > 0 && y < map.Height - 1)
            {
                return !map[x, y];
            }
            return false;
        }
    }
}
