using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using DeveMazeGenerator.Structures;
using System.Collections.Generic;

namespace DeveMazeGenerator.Helpers
{

    public class MazeVerifier
    {
        public static bool IsPerfectMaze(InnerMap map)
        {
            var copiedInnerMap = map.Clone();

            FloodFill(copiedInnerMap);

            //Make uneven because a maze actually is this size and not an even number
            var unevenHeight = UnevenHelper.MakeUneven(copiedInnerMap.Height);
            var unevenWidth = UnevenHelper.MakeUneven(copiedInnerMap.Width);

            for (int y = 0; y < unevenHeight; y++)
            {
                for (int x = 0; x < unevenWidth; x++)
                {
                    if (copiedInnerMap[x, y] == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void FloodFill(InnerMap map)
        {
            var stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(0, 0));

            MazePoint[] targets = new MazePoint[4];

            int x = 0;
            int y = 0;

            int width = map.Width;
            int height = map.Height;

            while (stackje.Count != 0)
            {
                MazePoint cur = stackje.Pop();
                x = cur.X;
                y = cur.Y;

                map[x, y] = true;

                int targetCount = 0;
                if (x - 1 > 0 && !map[x - 1, y])
                {
                    targets[targetCount].X = x - 1;
                    targets[targetCount].Y = y;
                    targetCount++;
                }
                if (x + 1 < width - 1 && !map[x + 1, y])
                {
                    targets[targetCount].X = x + 1;
                    targets[targetCount].Y = y;
                    targetCount++;
                }
                if (y - 1 > 0 && !map[x, y - 1])
                {
                    targets[targetCount].X = x;
                    targets[targetCount].Y = y - 1;
                    targetCount++;
                }
                if (y + 1 < height - 1 && !map[x, y + 1])
                {
                    targets[targetCount].X = x;
                    targets[targetCount].Y = y + 1;
                    targetCount++;
                }

                for (int i = 0; i < targetCount; i++)
                {
                    var target = targets[i];
                    stackje.Push(target);
                }
            }
        }
    }
}
