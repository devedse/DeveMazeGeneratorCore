using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using DeveMazeGeneratorCore.Structures;
using System.Collections.Generic;
using System.Linq;

namespace DeveMazeGeneratorCore.Helpers
{
    public static class MazeSplitter
    {
        public static IEnumerable<MazeWithPath> SplitUpMazeIntoChunks(MazeWithPath mazeWithPath, int chunkSize)
        {
            var innerMapFactory = new InnerMapFactory<ForwardingInnerMap>();
            for (int x = 0; x < mazeWithPath.InnerMap.Width; x += chunkSize)
            {
                for (int y = 0; y < mazeWithPath.InnerMap.Height; y += chunkSize)
                {
                    int capturedX = x;
                    int capturedY = y;
                    var chunk = new ForwardingInnerMap(chunkSize, chunkSize, (xx, yy) =>
                    {
                        // Calculate the actual coordinates in the original maze
                        int actualX = xx + capturedX;
                        int actualY = yy + capturedY;
                        // Check if the coordinates are within bounds
                        if (actualX < mazeWithPath.InnerMap.Width && actualY < mazeWithPath.InnerMap.Height)
                        {
                            return mazeWithPath.InnerMap[actualX, actualY];
                        }
                        return false;
                    });

                    var pathPart = mazeWithPath.Path
                        .Where(p => p.X >= x && p.X < x + chunkSize && p.Y >= y && p.Y < y + chunkSize)
                        .Select(p => new MazePointPos(p.X - x, p.Y - y, p.RelativePos))
                        .ToList();
                    yield return new MazeWithPath(chunk, pathPart);
                }
            }
        }
    }
}
