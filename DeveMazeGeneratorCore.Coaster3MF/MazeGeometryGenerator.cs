using DeveMazeGeneratorCore.Coaster3MF.Models;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using System.Linq;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class MazeGeometryGenerator
    {
        private const float GroundHeight = 2.5f; // White ground base height in mm
        private const float WallHeight = 2.5f; // Additional height for walls (black) in mm
        private const float PathHeight = 1.25f; // Additional height for path in mm
        private const float XYScale = 5.0f; // Scale multiplier for X and Y coordinates

        private static readonly string[] Colors =
        [
            "4", // [0] Slot 1 in AMS: Black for walls
            "8", // [1] Slot 2 in AMS: White for ground
            "0C", // [2] Slot 3 in AMS: Green for first half of path
            "1C", // [3] Slot 4 in AMS: Red for second half of path
            "2C", "3C", "4C", "5C", "6C", "7C", "8C", "9C", "AC", "BC", "CC", "DC", "EC",
            "0FC", "1FC", "2FC", "3FC", "4FC", "5FC", "6FC", "7FC", "8FC", "9FC", "AFC", "BFC", "CFC", "DFC", "EFC"
        ];



        /// <summary>
        /// First generates quads representing the maze geometry, then converts them to vertices and triangles.
        /// This is the refactored version that uses the two-step approach with proper face directions 
        /// and vertex reuse to prevent non-manifold edges.
        /// 
        /// Key improvements:
        /// - Each quad now has a FaceDirection (Front, Back, Left, Right, Top, Bottom)
        /// - Vertices are reused across quads to ensure manifold geometry
        /// - Proper counter-clockwise winding order for outward-facing normals
        /// </summary>
        public MeshData GenerateMazeGeometry(InnerMap maze, List<MazePointPos> path, bool singleCuboidPerPixel = true)
        {
            // Step 1: Generate quads
            var quads = GenerateMazeQuads(maze, path, singleCuboidPerPixel);

            // Step 2: Convert quads to mesh data
            return ConvertQuadsToMesh(quads);
        }

        /// <summary>
        /// Generates quads representing the maze geometry (ground, walls, path).
        /// </summary>
        public List<Quad> GenerateMazeQuads(InnerMap maze, List<MazePointPos> path, bool singleCuboidPerPixel = true, bool enableFaceCulling = true)
        {
            var quads = new List<Quad>();

            // Convert path to PathData for better organization
            var pathData = new PathData(path);

            // Ground plane quads
            AddGroundPlaneQuads(quads, maze, singleCuboidPerPixel);

            // Add wall quads - now split into two parts
            AddMazeWalls(maze, quads, singleCuboidPerPixel);

            // Add path quads (reactivated)
            AddMazePath(maze, quads, pathData);

            // Cull hidden faces (interior faces between adjacent cubes)
            if (enableFaceCulling)
            {
                MeshOptimizer.CullHiddenFaces(quads);
            }

            // Additional quad optimizations (merging adjacent quads) applied after adding paths
            // Even though this algorithm is quite cool, it doesn't work as it causes non-manifold edges
            //MeshOptimizer.OptimizeQuads(quads);

            return quads;
        }

        private void AddMazePath(InnerMap maze, List<Quad> quads, PathData pathData)
        {
            // Path cube quads - only within the valid maze area (excluding rightmost and bottommost edge)
            foreach (var point in pathData.PathSet)
            {
                if (point.X < maze.Width - 1 && point.Y < maze.Height - 1 && maze[point.X, point.Y]) // Open space that's part of the path and within valid area
                {
                    // Determine color based on position in path (0-255)
                    var relativePos = pathData.PathPositions[point];
                    var paintColor = relativePos < 128 ? Colors[2] : Colors[3]; // Green for first half, Red for second half

                    AddCubeQuads(quads, point.X, point.Y, GroundHeight, GroundHeight + PathHeight, paintColor);
                }
            }
        }

        private void AddMazeWalls(InnerMap maze, List<Quad> quads, bool singleCuboidPerPixel)
        {
            if (singleCuboidPerPixel)
            {
                // Simple approach: Add two cubes per maze pixel that is a wall
                // Lower cube: same height as path (GroundHeight to GroundHeight + PathHeight)
                // Upper cube: remaining wall height (GroundHeight + PathHeight to GroundHeight + WallHeight)
                for (int y = 0; y < maze.Height - 1; y++)
                {
                    for (int x = 0; x < maze.Width - 1; x++)
                    {
                        if (!maze[x, y]) // Wall position (false = wall, true = open space)
                        {
                            // Lower wall part - same height as path
                            AddCubeQuads(quads, x, y, GroundHeight, GroundHeight + PathHeight, Colors[0]);

                            // Upper wall part - remaining height
                            AddCubeQuads(quads, x, y, GroundHeight + PathHeight, GroundHeight + WallHeight, Colors[0]);
                        }
                    }
                }
            }
            else
            {
                // Complex approach: Generate optimized wall segments with split cuboids
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
                            if (!maze[wall.Xstart, wall.Ystart - 1] || !maze[wall.Xstart, wall.Ystart + 1])
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
                            if (!maze[wall.Xend, wall.Yend - 1] || !maze[wall.Xend, wall.Yend + 1])
                            {
                                xend--;
                            }
                        }
                        else if (wall.Xend == maze.Width - 2)
                        {
                            xend--;
                        }

                        // Lower wall part - same height as path
                        AddCubeQuadsWithDimensions(quads, xstart, wall.Ystart, xend + 1, wall.Yend + 1, GroundHeight, GroundHeight + PathHeight, Colors[0]);

                        // Upper wall part - remaining height
                        AddCubeQuadsWithDimensions(quads, xstart, wall.Ystart, xend + 1, wall.Yend + 1, GroundHeight + PathHeight, GroundHeight + WallHeight, Colors[0]);
                    }
                    else
                    {
                        var ystart = wall.Ystart;
                        var yend = wall.Yend;

                        if (wall.Xstart > 0 && wall.Xstart < maze.Width - 2 &&
                            !maze[wall.Xstart - 1, wall.Ystart] && !maze[wall.Xstart + 1, wall.Ystart])
                        {
                            ystart++;
                        }

                        if (wall.Xend < maze.Width - 2 && wall.Xend > 0 &&
                            !maze[wall.Xend - 1, wall.Yend] && !maze[wall.Xend + 1, wall.Yend])
                        {
                            yend--;
                        }

                        // Lower wall part - same height as path
                        AddCubeQuadsWithDimensions(quads, wall.Xstart, ystart, wall.Xend + 1, yend + 1, GroundHeight, GroundHeight + PathHeight, Colors[0]);

                        // Upper wall part - remaining height
                        AddCubeQuadsWithDimensions(quads, wall.Xstart, ystart, wall.Xend + 1, yend + 1, GroundHeight + PathHeight, GroundHeight + WallHeight, Colors[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a list of quads to mesh data (vertices and triangles) with proper vertex reuse and winding order.
        /// Uses FaceDirection to determine correct triangle winding for outward-facing normals.
        /// </summary>
        public MeshData ConvertQuadsToMesh(List<Quad> quads)
        {
            var meshData = new MeshData();
            var vertexToIndex = new Dictionary<Vertex, int>();

            foreach (var quad in quads)
            {
                // Get vertices in correct winding order for outward-facing normals
                var orderedVertices = quad.GetOrderedVertices();

                // Get or create vertex indices with reuse
                var indices = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    var vertex = orderedVertices[i];
                    if (!vertexToIndex.TryGetValue(vertex, out int index))
                    {
                        index = meshData.Vertices.Count;
                        meshData.Vertices.Add(vertex);
                        vertexToIndex[vertex] = index;
                    }
                    indices[i] = index;
                }

                // Create 2 triangles from the quad with correct winding order (counter-clockwise)
                // Triangle 1: indices[0], indices[1], indices[2]
                // Triangle 2: indices[0], indices[2], indices[3]
                meshData.Triangles.Add(new Triangle(indices[0], indices[1], indices[2], quad.PaintColor));
                meshData.Triangles.Add(new Triangle(indices[0], indices[2], indices[3], quad.PaintColor));
            }

            Console.WriteLine($"Converted {quads.Count} quads to {meshData.Vertices.Count} unique vertices and {meshData.Triangles.Count} triangles.");

            return meshData;
        }

        private void AddGroundPlaneQuads(List<Quad> quads, InnerMap maze, bool singleCuboidPerPixel)
        {
            if (singleCuboidPerPixel)
            {
                // Generate one cube for each ground cell (maze.Width-1 x maze.Height-1)
                for (int y = 0; y < maze.Height - 1; y++)
                {
                    for (int x = 0; x < maze.Width - 1; x++)
                    {
                        // Each ground cube has a white top and black sides/bottom
                        AddCubeQuads(quads, x, y, 0, GroundHeight, Colors[0], Colors[1]); // White ground cubes
                    }
                }
            }
            else
            {
                // Single large ground plane using AddCubeQuadsWithDimensions
                AddCubeQuadsWithDimensions(quads, 0, 0, maze.Width - 1, maze.Height - 1, 0, GroundHeight, Colors[0], Colors[1]); // White ground
            }
        }

        private void AddCubeQuads(List<Quad> quads, int x, int y, float zBottom, float zTop, string paintColor, string? topFacePaintColor = null)
        {
            AddCubeQuadsWithDimensions(quads, x, y, x + 1, y + 1, zBottom, zTop, paintColor, topFacePaintColor);
        }

        private void AddCubeQuadsWithDimensions(List<Quad> quads, float x, float y, float endX, float endY, float zBottom, float zTop, string paintColor, string? topFacePaintColor = null)
        {
            // Apply XY scaling to coordinates
            var scaledX = x * XYScale;
            var scaledY = y * XYScale * -1;
            var scaledEndX = endX * XYScale;
            var scaledEndY = endY * XYScale * -1;

            // Bottom face
            quads.Add(new Quad(
                new Vertex(scaledX, scaledY, zBottom),
                new Vertex(scaledEndX, scaledY, zBottom),
                new Vertex(scaledEndX, scaledEndY, zBottom),
                new Vertex(scaledX, scaledEndY, zBottom),
                paintColor,
                FaceDirection.Bottom
            ));

            // Top face
            quads.Add(new Quad(
                new Vertex(scaledX, scaledY, zTop),
                new Vertex(scaledEndX, scaledY, zTop),
                new Vertex(scaledEndX, scaledEndY, zTop),
                new Vertex(scaledX, scaledEndY, zTop),
                topFacePaintColor ?? paintColor,
                FaceDirection.Top
            ));

            // Front face
            quads.Add(new Quad(
                new Vertex(scaledX, scaledY, zBottom),
                new Vertex(scaledEndX, scaledY, zBottom),
                new Vertex(scaledEndX, scaledY, zTop),
                new Vertex(scaledX, scaledY, zTop),
                paintColor,
                FaceDirection.Front
            ));

            // Right face
            quads.Add(new Quad(
                new Vertex(scaledEndX, scaledY, zBottom),
                new Vertex(scaledEndX, scaledEndY, zBottom),
                new Vertex(scaledEndX, scaledEndY, zTop),
                new Vertex(scaledEndX, scaledY, zTop),
                paintColor,
                FaceDirection.Right
            ));

            // Back face
            quads.Add(new Quad(
                new Vertex(scaledEndX, scaledEndY, zBottom),
                new Vertex(scaledX, scaledEndY, zBottom),
                new Vertex(scaledX, scaledEndY, zTop),
                new Vertex(scaledEndX, scaledEndY, zTop),
                paintColor,
                FaceDirection.Back
            ));

            // Left face
            quads.Add(new Quad(
                new Vertex(scaledX, scaledEndY, zBottom),
                new Vertex(scaledX, scaledY, zBottom),
                new Vertex(scaledX, scaledY, zTop),
                new Vertex(scaledX, scaledEndY, zTop),
                paintColor,
                FaceDirection.Left
            ));
        }
    }
}