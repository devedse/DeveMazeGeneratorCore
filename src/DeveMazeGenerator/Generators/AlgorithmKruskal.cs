using DeveMazeGenerator.Factories;
using DeveMazeGenerator.InnerMaps;
using System;
using System.Collections.Generic;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.Structures;
using DeveMazeGenerator.ExtensionMethods;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmKruskal : Algorithm
    {
        public override InnerMap GoGenerate<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            var innerMap = mapFactory.Create();
            var random = randomFactory.Create();

            return GoGenerateInternal(innerMap, random, pixelChangedCallback);
        }

        private InnerMap GoGenerateInternal(InnerMap innerMap, IRandom random, Action<int, int, long, long> pixelChangedCallback)
        {
            long totSteps = (((long)innerMap.Width - 1L) / 2L) * (((long)innerMap.Height - 1L) / 2L) * 2;
            long currentStep = 1;

            KruskalCell[][] theMap;

            //Prepare
            theMap = new KruskalCell[innerMap.Width][];
            for (int x = 0; x < innerMap.Width; x++)
            {
                theMap[x] = new KruskalCell[innerMap.Height];
                for (int y = 0; y < innerMap.Height; y++)
                {
                    KruskalCell c = new KruskalCell(x, y);
                    theMap[x][y] = c;

                    if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0 && x != innerMap.Width - 1 && y != innerMap.Height - 1)
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
            for (int y = 1; y < innerMap.Height - 2; y++)
            {
                Boolean horizontalwall = false;
                int startje = 1;
                if (y % 2 == 1)
                {
                    horizontalwall = true;
                    startje = 2;
                }
                for (int x = startje; x < innerMap.Width - 2; x = x + 2)
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
                    pixelChangedCallback(wall.X, wall.Y, currentStep, totSteps);
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

            for (int y = 0; y < innerMap.Height; y++)
            {
                for (int x = 0; x < innerMap.Width; x++)
                {
                    var solid = theMap[x][y].Solid;
                    if (solid)
                    {
                        innerMap[x, y] = false;
                    }
                    else
                    {
                        innerMap[x, y] = true;
                    }

                }
            }

            return innerMap;
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
