using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class MazeGeometryGenerator
    {
        private const float GroundHeight = 2.5f; // White ground base height in mm
        private const float WallHeight = 2.5f; // Additional height for walls (black) in mm
        private const float PathHeight = 1.25f; // Additional height for path in mm

        private static readonly string[] Colors =
        [
            "4", // [0] Slot 1 in AMS: Black for walls
            "8", // [1] Slot 2 in AMS: Green for first half of path
            "0C", // [2] Slot 3 in AMS: Red for second half of path
            "1C", // [3] Slot 4 in AMS: White for ground
            "2C", "3C", "4C", "5C", "6C", "7C", "8C", "9C", "AC", "BC", "CC", "DC", "EC",
            "0FC", "1FC", "2FC", "3FC", "4FC", "5FC", "6FC", "7FC", "8FC", "9FC", "AFC", "BFC", "CFC", "DFC", "EFC"
        ];

        public class MeshData
        {
            public List<(float x, float y, float z)> Vertices { get; } = new();
            public List<(int v1, int v2, int v3, string paintColor)> Triangles { get; } = new();
        }

        public MeshData GenerateMazeGeometry(InnerMap maze, List<MazePointPos> path)
        {
            var meshData = new MeshData();
            
            // Convert path to a HashSet for quick lookup
            var pathSet = new HashSet<(int x, int y)>();
            var pathPositions = new Dictionary<(int x, int y), byte>();

            foreach (var point in path)
            {
                pathSet.Add((point.X, point.Y));
                pathPositions[(point.X, point.Y)] = point.RelativePos;
            }

            // Ground plane vertices and triangles
            AddGroundPlane(meshData.Vertices, meshData.Triangles, maze);

            // Add walls
            var mazeWalls = maze.GenerateListOfMazeWalls();
            foreach (var wall in mazeWalls)
            {
                AddWall(meshData.Vertices, meshData.Triangles, maze, wall);
            }

            // Path cubes - only within the valid maze area (excluding rightmost and bottommost edge)
            foreach (var (x, y) in pathSet)
            {
                if (x < maze.Width - 1 && y < maze.Height - 1 && maze[x, y]) // Open space that's part of the path and within valid area
                {
                    // Determine color based on position in path (0-255)
                    var relativePos = pathPositions[(x, y)];
                    var paintColor = relativePos < 128 ? Colors[2] : Colors[3];

                    AddCube(meshData.Vertices, meshData.Triangles, x, y, GroundHeight, GroundHeight + PathHeight, paintColor);
                }
            }

            return meshData;
        }

        private void AddWall(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, InnerMap maze, MazeWall wall)
        {
            var isHorizontal = wall.Ystart == wall.Yend; // Check if the wall is horizontal or vertical

            if (isHorizontal)
            {
                var xstart = wall.Xstart;
                var xend = wall.Xend;

                if (wall.Ystart > 0 && wall.Ystart < maze.Height - 2)
                {
                    if (maze[wall.Xstart, wall.Ystart - 1] == false || maze[wall.Xstart, wall.Ystart + 1] == false)
                    {
                        xstart++;
                    }
                }
                else if (wall.Xstart == 0)
                {
                    xstart++;
                }

                if (wall.Yend < maze.Height - 2 && wall.Yend > 0)
                {
                    if (maze[wall.Xend, wall.Yend - 1] == false || maze[wall.Xend, wall.Yend + 1] == false)
                    {
                        xend--;
                    }
                }
                else if (wall.Xend == maze.Width - 2)
                {
                    xend--;
                }
                AddCubeWithDimensions(vertices, triangles, xstart, wall.Ystart, xend + 1, wall.Yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Horizontal wall
            }
            else
            {
                var ystart = wall.Ystart;
                var yend = wall.Yend;

                if (wall.Xstart > 0 && wall.Xstart < maze.Width - 2)
                {
                    if (maze[wall.Xstart - 1, wall.Ystart] == false && maze[wall.Xstart + 1, wall.Ystart] == false)
                    {
                        ystart++;
                    }
                }

                if (wall.Xend < maze.Width - 2 && wall.Xend > 0)
                {
                    if (maze[wall.Xend - 1, wall.Yend] == false && maze[wall.Xend + 1, wall.Yend] == false)
                    {
                        yend--;
                    }
                }

                AddCubeWithDimensions(vertices, triangles, wall.Xstart, ystart, wall.Xend + 1, yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Vertical wall
            }
        }

        private void AddGroundPlane(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, InnerMap maze)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices - exclude the rightmost and bottommost edge (following image generation convention)
            vertices.Add((0, 0, 0));
            vertices.Add((maze.Width - 1, 0, 0));
            vertices.Add((maze.Width - 1, maze.Height - 1, 0));
            vertices.Add((0, maze.Height - 1, 0));

            // Top vertices
            vertices.Add((0, 0, GroundHeight));
            vertices.Add((maze.Width - 1, 0, GroundHeight));
            vertices.Add((maze.Width - 1, maze.Height - 1, GroundHeight));
            vertices.Add((0, maze.Height - 1, GroundHeight));

            // Bottom face (z = 0) - black
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, Colors[0]));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, Colors[0]));

            // Top face (z = GroundHeight) - white
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, Colors[1]));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, Colors[1]));

            // Side faces - black
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, Colors[0])); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, Colors[0]));

            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, Colors[0])); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, Colors[0]));

            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, Colors[0])); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, Colors[0]));

            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, Colors[0])); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, Colors[0]));
        }

        private void AddCube(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, int x, int y, float zBottom, float zTop, string paintColor)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices
            vertices.Add((x, y, zBottom));
            vertices.Add((x + 1, y, zBottom));
            vertices.Add((x + 1, y + 1, zBottom));
            vertices.Add((x, y + 1, zBottom));

            // Top vertices
            vertices.Add((x, y, zTop));
            vertices.Add((x + 1, y, zTop));
            vertices.Add((x + 1, y + 1, zTop));
            vertices.Add((x, y + 1, zTop));

            // Bottom face
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, paintColor));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, paintColor));

            // Top face
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, paintColor));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, paintColor));

            // Side faces
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, paintColor)); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, paintColor));

            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, paintColor)); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, paintColor));

            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, paintColor)); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, paintColor));

            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, paintColor)); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, paintColor));
        }

        /// <summary>
        /// Adds a cube with custom dimensions to the mesh.
        /// </summary>
        /// <param name="vertices">The list of vertices to add to</param>
        /// <param name="triangles">The list of triangles to add to</param>
        /// <param name="x">Start X position of the cube</param>
        /// <param name="y">Start Y position of the cube</param>
        /// <param name="endX">End X position of the cube</param>
        /// <param name="endY">End Y position of the cube</param>
        /// <param name="zBottom">Bottom Z coordinate</param>
        /// <param name="zTop">Top Z coordinate</param>
        /// <param name="paintColor">Color of the cube</param>
        private void AddCubeWithDimensions(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles,
            float x, float y, float endX, float endY, float zBottom, float zTop, string paintColor)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices
            vertices.Add((x, y, zBottom));
            vertices.Add((endX, y, zBottom));
            vertices.Add((endX, endY, zBottom));
            vertices.Add((x, endY, zBottom));

            // Top vertices
            vertices.Add((x, y, zTop));
            vertices.Add((endX, y, zTop));
            vertices.Add((endX, endY, zTop));
            vertices.Add((x, endY, zTop));

            // Bottom face
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, paintColor));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, paintColor));

            // Top face
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, paintColor));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, paintColor));

            // Side faces
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, paintColor)); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, paintColor));

            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, paintColor)); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, paintColor));

            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, paintColor)); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, paintColor));

            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, paintColor)); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, paintColor));
        }

        public int CalculateFaceCount(InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            // Ground plane: 2 triangles per unit square
            int groundFaces = (maze.Width - 1) * (maze.Height - 1) * 2;

            // Count wall cubes
            int wallCubes = 0;
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y))) // Wall position
                    {
                        wallCubes++;
                    }
                }
            }

            // Count path cubes (within valid maze area)
            int pathCubes = 0;
            foreach (var (x, y) in pathSet)
            {
                if (x < maze.Width - 1 && y < maze.Height - 1 && maze[x, y])
                {
                    pathCubes++;
                }
            }

            // Each cube has 12 triangles (6 faces * 2 triangles per face)
            int cubeFaces = (wallCubes + pathCubes) * 12;

            return groundFaces + cubeFaces;
        }
    }
}