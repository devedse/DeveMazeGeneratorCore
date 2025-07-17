using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using DeveMazeGeneratorCore.Coaster3MF.Models;

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



        /// <summary>
        /// First generates quads representing the maze geometry, then converts them to vertices and triangles.
        /// This is the refactored version that uses the two-step approach.
        /// </summary>
        public MeshData GenerateMazeGeometry(InnerMap maze, List<MazePointPos> path)
        {
            // Step 1: Generate quads
            var quads = GenerateMazeQuads(maze, path);
            
            // Step 2: Convert quads to mesh data
            return ConvertQuadsToMesh(quads);
        }

        /// <summary>
        /// Generates quads representing the maze geometry (ground, walls, path).
        /// </summary>
        public List<Quad> GenerateMazeQuads(InnerMap maze, List<MazePointPos> path)
        {
            var quads = new List<Quad>();
            
            // Convert path to PathData for better organization
            var pathData = new PathData(path);

            // Ground plane quads
            AddGroundPlaneQuads(quads, maze);

            // Add wall quads - inlined AddWall method logic
            var mazeWalls = maze.GenerateListOfMazeWalls();
            foreach (var wall in mazeWalls)
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
                    AddCubeQuadsWithDimensions(quads, xstart, wall.Ystart, xend + 1, wall.Yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Horizontal wall
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

                    AddCubeQuadsWithDimensions(quads, wall.Xstart, ystart, wall.Xend + 1, yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Vertical wall
                }
            }

            // Path cube quads - only within the valid maze area (excluding rightmost and bottommost edge)
            foreach (var point in pathData.PathSet)
            {
                if (point.X < maze.Width - 1 && point.Y < maze.Height - 1 && maze[point.X, point.Y]) // Open space that's part of the path and within valid area
                {
                    // Determine color based on position in path (0-255)
                    var relativePos = pathData.PathPositions[point];
                    var paintColor = relativePos < 128 ? Colors[2] : Colors[3];

                    AddCubeQuads(quads, point.X, point.Y, GroundHeight, GroundHeight + PathHeight, paintColor);
                }
            }

            return quads;
        }

        /// <summary>
        /// Converts a list of quads to mesh data (vertices and triangles).
        /// </summary>
        public MeshData ConvertQuadsToMesh(List<Quad> quads)
        {
            var meshData = new MeshData();

            foreach (var quad in quads)
            {
                // Add the 4 vertices of the quad
                int baseIndex = meshData.Vertices.Count;
                meshData.Vertices.Add(quad.V1);
                meshData.Vertices.Add(quad.V2);
                meshData.Vertices.Add(quad.V3);
                meshData.Vertices.Add(quad.V4);

                // Create 2 triangles from the quad (assuming vertices are in correct order)
                meshData.Triangles.Add(new Triangle(baseIndex + 0, baseIndex + 2, baseIndex + 1, quad.PaintColor));
                meshData.Triangles.Add(new Triangle(baseIndex + 0, baseIndex + 3, baseIndex + 2, quad.PaintColor));
            }

            return meshData;
        }

        private void AddGroundPlaneQuads(List<Quad> quads, InnerMap maze)
        {
            // Bottom face (z = 0) - black
            quads.Add(new Quad(
                new Vertex(0, 0, 0),
                new Vertex(maze.Width - 1, 0, 0),
                new Vertex(maze.Width - 1, maze.Height - 1, 0),
                new Vertex(0, maze.Height - 1, 0),
                Colors[0]
            ));

            // Top face (z = GroundHeight) - white
            quads.Add(new Quad(
                new Vertex(0, 0, GroundHeight),
                new Vertex(maze.Width - 1, 0, GroundHeight),
                new Vertex(maze.Width - 1, maze.Height - 1, GroundHeight),
                new Vertex(0, maze.Height - 1, GroundHeight),
                Colors[1]
            ));

            // Side faces - black
            // Front face
            quads.Add(new Quad(
                new Vertex(0, 0, 0),
                new Vertex(maze.Width - 1, 0, 0),
                new Vertex(maze.Width - 1, 0, GroundHeight),
                new Vertex(0, 0, GroundHeight),
                Colors[0]
            ));

            // Right face
            quads.Add(new Quad(
                new Vertex(maze.Width - 1, 0, 0),
                new Vertex(maze.Width - 1, maze.Height - 1, 0),
                new Vertex(maze.Width - 1, maze.Height - 1, GroundHeight),
                new Vertex(maze.Width - 1, 0, GroundHeight),
                Colors[0]
            ));

            // Back face
            quads.Add(new Quad(
                new Vertex(maze.Width - 1, maze.Height - 1, 0),
                new Vertex(0, maze.Height - 1, 0),
                new Vertex(0, maze.Height - 1, GroundHeight),
                new Vertex(maze.Width - 1, maze.Height - 1, GroundHeight),
                Colors[0]
            ));

            // Left face
            quads.Add(new Quad(
                new Vertex(0, maze.Height - 1, 0),
                new Vertex(0, 0, 0),
                new Vertex(0, 0, GroundHeight),
                new Vertex(0, maze.Height - 1, GroundHeight),
                Colors[0]
            ));
        }

        private void AddCubeQuads(List<Quad> quads, int x, int y, float zBottom, float zTop, string paintColor)
        {
            AddCubeQuadsWithDimensions(quads, x, y, x + 1, y + 1, zBottom, zTop, paintColor);
        }

        private void AddCubeQuadsWithDimensions(List<Quad> quads, float x, float y, float endX, float endY, float zBottom, float zTop, string paintColor)
        {
            // Bottom face
            quads.Add(new Quad(
                new Vertex(x, y, zBottom),
                new Vertex(endX, y, zBottom),
                new Vertex(endX, endY, zBottom),
                new Vertex(x, endY, zBottom),
                paintColor
            ));

            // Top face
            quads.Add(new Quad(
                new Vertex(x, y, zTop),
                new Vertex(endX, y, zTop),
                new Vertex(endX, endY, zTop),
                new Vertex(x, endY, zTop),
                paintColor
            ));

            // Front face
            quads.Add(new Quad(
                new Vertex(x, y, zBottom),
                new Vertex(endX, y, zBottom),
                new Vertex(endX, y, zTop),
                new Vertex(x, y, zTop),
                paintColor
            ));

            // Right face
            quads.Add(new Quad(
                new Vertex(endX, y, zBottom),
                new Vertex(endX, endY, zBottom),
                new Vertex(endX, endY, zTop),
                new Vertex(endX, y, zTop),
                paintColor
            ));

            // Back face
            quads.Add(new Quad(
                new Vertex(endX, endY, zBottom),
                new Vertex(x, endY, zBottom),
                new Vertex(x, endY, zTop),
                new Vertex(endX, endY, zTop),
                paintColor
            ));

            // Left face
            quads.Add(new Quad(
                new Vertex(x, endY, zBottom),
                new Vertex(x, y, zBottom),
                new Vertex(x, y, zTop),
                new Vertex(x, endY, zTop),
                paintColor
            ));
        }

        /// <summary>
        /// Original method for backward compatibility - generates mesh data directly.
        /// </summary>
        public MeshData GenerateMazeGeometryDirect(InnerMap maze, List<MazePointPos> path)
        {
            var meshData = new MeshData();
            
            // Convert path to PathData for better organization
            var pathData = new PathData(path);

            // Ground plane vertices and triangles
            AddGroundPlane(meshData.Vertices, meshData.Triangles, maze);

            // Add walls - inlined AddWall method logic
            var mazeWalls = maze.GenerateListOfMazeWalls();
            foreach (var wall in mazeWalls)
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
                    AddCubeWithDimensions(meshData.Vertices, meshData.Triangles, xstart, wall.Ystart, xend + 1, wall.Yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Horizontal wall
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

                    AddCubeWithDimensions(meshData.Vertices, meshData.Triangles, wall.Xstart, ystart, wall.Xend + 1, yend + 1, GroundHeight, GroundHeight + WallHeight, Colors[0]); // Vertical wall
                }
            }




            // Path cubes - only within the valid maze area (excluding rightmost and bottommost edge)
            foreach (var point in pathData.PathSet)
            {
                if (point.X < maze.Width - 1 && point.Y < maze.Height - 1 && maze[point.X, point.Y]) // Open space that's part of the path and within valid area
                {
                    // Determine color based on position in path (0-255)
                    var relativePos = pathData.PathPositions[point];
                    var paintColor = relativePos < 128 ? Colors[2] : Colors[3];

                    AddCube(meshData.Vertices, meshData.Triangles, point.X, point.Y, GroundHeight, GroundHeight + PathHeight, paintColor);
                }
            }

            return meshData;
        }

        private void VertexAndIndexOptimizer(List<Vertex> vertices, List<Triangle> triangles)
        {
            // We will go over all vertices
            // Let's say we start with vertex 0
            // We find all vertices on the same position
            // Then we check if these vertices are in the same plane

        }


        private void AddGroundPlane(List<Vertex> vertices, List<Triangle> triangles, InnerMap maze)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices - exclude the rightmost and bottommost edge (following image generation convention)
            vertices.Add(new Vertex(0, 0, 0));
            vertices.Add(new Vertex(maze.Width - 1, 0, 0));
            vertices.Add(new Vertex(maze.Width - 1, maze.Height - 1, 0));
            vertices.Add(new Vertex(0, maze.Height - 1, 0));

            // Top vertices
            vertices.Add(new Vertex(0, 0, GroundHeight));
            vertices.Add(new Vertex(maze.Width - 1, 0, GroundHeight));
            vertices.Add(new Vertex(maze.Width - 1, maze.Height - 1, GroundHeight));
            vertices.Add(new Vertex(0, maze.Height - 1, GroundHeight));

            // Bottom face (z = 0) - black
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 2, baseIndex + 1, Colors[0]));
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 3, baseIndex + 2, Colors[0]));

            // Top face (z = GroundHeight) - white
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 5, baseIndex + 6, Colors[1]));
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 6, baseIndex + 7, Colors[1]));

            // Side faces - black
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 1, baseIndex + 5, Colors[0])); // Front
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 5, baseIndex + 4, Colors[0]));

            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 2, baseIndex + 6, Colors[0])); // Right
            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 6, baseIndex + 5, Colors[0]));

            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 3, baseIndex + 7, Colors[0])); // Back
            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 7, baseIndex + 6, Colors[0]));

            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 0, baseIndex + 4, Colors[0])); // Left
            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 4, baseIndex + 7, Colors[0]));
        }

        private void AddCube(List<Vertex> vertices, List<Triangle> triangles, int x, int y, float zBottom, float zTop, string paintColor)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices
            vertices.Add(new Vertex(x, y, zBottom));
            vertices.Add(new Vertex(x + 1, y, zBottom));
            vertices.Add(new Vertex(x + 1, y + 1, zBottom));
            vertices.Add(new Vertex(x, y + 1, zBottom));

            // Top vertices
            vertices.Add(new Vertex(x, y, zTop));
            vertices.Add(new Vertex(x + 1, y, zTop));
            vertices.Add(new Vertex(x + 1, y + 1, zTop));
            vertices.Add(new Vertex(x, y + 1, zTop));

            // Bottom face
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 2, baseIndex + 1, paintColor));
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 3, baseIndex + 2, paintColor));

            // Top face
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 5, baseIndex + 6, paintColor));
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 6, baseIndex + 7, paintColor));

            // Side faces
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 1, baseIndex + 5, paintColor)); // Front
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 5, baseIndex + 4, paintColor));

            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 2, baseIndex + 6, paintColor)); // Right
            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 6, baseIndex + 5, paintColor));

            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 3, baseIndex + 7, paintColor)); // Back
            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 7, baseIndex + 6, paintColor));

            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 0, baseIndex + 4, paintColor)); // Left
            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 4, baseIndex + 7, paintColor));
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
        private void AddCubeWithDimensions(List<Vertex> vertices, List<Triangle> triangles,
            float x, float y, float endX, float endY, float zBottom, float zTop, string paintColor)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices
            vertices.Add(new Vertex(x, y, zBottom));
            vertices.Add(new Vertex(endX, y, zBottom));
            vertices.Add(new Vertex(endX, endY, zBottom));
            vertices.Add(new Vertex(x, endY, zBottom));

            // Top vertices
            vertices.Add(new Vertex(x, y, zTop));
            vertices.Add(new Vertex(endX, y, zTop));
            vertices.Add(new Vertex(endX, endY, zTop));
            vertices.Add(new Vertex(x, endY, zTop));

            // Bottom face
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 2, baseIndex + 1, paintColor));
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 3, baseIndex + 2, paintColor));

            // Top face
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 5, baseIndex + 6, paintColor));
            triangles.Add(new Triangle(baseIndex + 4, baseIndex + 6, baseIndex + 7, paintColor));

            // Side faces
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 1, baseIndex + 5, paintColor)); // Front
            triangles.Add(new Triangle(baseIndex + 0, baseIndex + 5, baseIndex + 4, paintColor));

            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 2, baseIndex + 6, paintColor)); // Right
            triangles.Add(new Triangle(baseIndex + 1, baseIndex + 6, baseIndex + 5, paintColor));

            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 3, baseIndex + 7, paintColor)); // Back
            triangles.Add(new Triangle(baseIndex + 2, baseIndex + 7, baseIndex + 6, paintColor));

            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 0, baseIndex + 4, paintColor)); // Left
            triangles.Add(new Triangle(baseIndex + 3, baseIndex + 4, baseIndex + 7, paintColor));
        }

        public int CalculateFaceCount(InnerMap maze, PathData pathData)
        {
            // Ground plane: 2 triangles per unit square
            int groundFaces = (maze.Width - 1) * (maze.Height - 1) * 2;

            // Count wall cubes
            int wallCubes = 0;
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathData.Contains(x, y)) // Wall position
                    {
                        wallCubes++;
                    }
                }
            }

            // Count path cubes (within valid maze area)
            int pathCubes = 0;
            foreach (var point in pathData.PathSet)
            {
                if (point.X < maze.Width - 1 && point.Y < maze.Height - 1 && maze[point.X, point.Y])
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