using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using DeveMazeGeneratorCore.Imageification;
using System.IO.Compression;
using System.Xml;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class MazeCoaster3MF
    {
        private const float CoasterSize = 5.0f; // Total height in mm
        private const float GroundHeight = 2.5f; // White ground base height in mm
        private const float WallHeight = 2.5f; // Additional height for walls (black) in mm
        private const float PathHeight = 1.25f; // Additional height for path in mm



        private static string[] Colors =
        [
            "4", // [0] Slot 1 in AMS: Black for walls
            "8", // [1] Slot 2 in AMS: Green for first half of path
            "0C", // [2] Slot 3 in AMS: Red for second half of path
            "1C", // [3] Slot 4 in AMS: White for ground
            "2C",
            "3C",
            "4C",
            "5C",
            "6C",
            "7C",
            "8C",
            "9C",
            "AC",
            "BC",
            "CC",
            "DC",
            "EC",
            "0FC",
            "1FC",
            "2FC",
            "3FC",
            "4FC",
            "5FC",
            "6FC",
            "7FC",
            "8FC",
            "9FC",
            "AFC",
            "BFC",
            "CFC",
            "DFC",
            "EFC"
        ];

        public void Generate3MFCoaster(string filename, int mazeSize, int? seed = null)
        {
            Console.WriteLine($"Generating {mazeSize}x{mazeSize} maze...");

            // Generate maze using AlgorithmBacktrack2Deluxe2_AsByte
            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();
            var actionThing = new NoAction();

            var usedSeed = seed ?? 1337;
            var maze = alg.GoGenerate(mazeSize, mazeSize, usedSeed, innerMapFactory, randomFactory, actionThing);

            Console.WriteLine("Finding path through maze...");

            // Find the path with position information
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

            Console.WriteLine($"Path found with {path.Count} points (seed: {usedSeed})");
            
            // Calculate mesh statistics
            var (triangles, vertices) = CalculateMeshStats(maze.InnerMap, path);
            Console.WriteLine($"Mesh stats: {triangles} triangles, {vertices} vertices");
            
            Console.WriteLine("Generating 3MF file...");

            // Generate the 3MF file
            Generate3MFFile(maze.InnerMap, path, filename);
        }

        private void Generate3MFFile(InnerMap maze, List<MazePointPos> path, string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                // Create required 3MF files
                CreateContentTypesFile(archive);
                CreateRelsFile(archive);
                Create3DModelFile(archive);
                Create3DModelRelsFile(archive);
                CreateObjectFile(archive, maze, path);
                CreateModelSettingsFile(archive, maze, path);
                CreateMetadataFiles(archive);

                // Generate and add thumbnail images
                CreateThumbnailImages(archive, maze, path);
            }
        }

        private void CreateContentTypesFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("[Content_Types].xml");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                        <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                        <Default Extension="model" ContentType="application/vnd.ms-package.3dmanufacturing-3dmodel+xml"/>
                        <Default Extension="png" ContentType="image/png"/>
                        <Default Extension="gcode" ContentType="text/x.gcode"/>
                    </Types>
                    """);
            }
        }

        private void CreateRelsFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("_rels/.rels");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                        <Relationship Target="/3D/3dmodel.model" Id="rel-1" Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>
                        <Relationship Target="/Metadata/plate_1.png" Id="rel-2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail"/>
                        <Relationship Target="/Metadata/plate_1.png" Id="rel-4" Type="http://schemas.bambulab.com/package/2021/cover-thumbnail-middle"/>
                        <Relationship Target="/Metadata/plate_1_small.png" Id="rel-5" Type="http://schemas.bambulab.com/package/2021/cover-thumbnail-small"/>
                    </Relationships>
                    """);
            }
        }

        private void Create3DModelRelsFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("3D/_rels/3dmodel.model.rels");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                        <Relationship Target="/3D/Objects/object_1.model" Id="rel-1" Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"/>
                    </Relationships>
                    """);
            }
        }

        private void Create3DModelFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("3D/3dmodel.model");
            using (var stream = entry.Open())
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("model", "http://schemas.microsoft.com/3dmanufacturing/core/2015/02");
                writer.WriteAttributeString("unit", "millimeter");
                writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", "en-US");
                writer.WriteAttributeString("xmlns", "p", null, "http://schemas.microsoft.com/3dmanufacturing/production/2015/06");
                writer.WriteAttributeString("requiredextensions", "p");

                // Metadata
                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Application");
                writer.WriteString("BambuStudio-02.01.01.52");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "BambuStudio:3mfVersion");
                writer.WriteString("1");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Copyright");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "CreationDate");
                writer.WriteString("2025-01-15");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Description");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Designer");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "DesignerCover");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "DesignerUserId");
                writer.WriteString("2360007279");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "License");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "ModificationDate");
                writer.WriteString("2025-01-15");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Origin");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", "Title");
                writer.WriteString("");
                writer.WriteEndElement();

                // Resources section
                writer.WriteStartElement("resources");

                writer.WriteStartElement("object");
                writer.WriteAttributeString("id", "2");
                writer.WriteAttributeString("p", "uuid", null, "00000001-61cb-4c03-9d28-80fed5dfa1dc");
                writer.WriteAttributeString("type", "model");

                writer.WriteStartElement("components");
                writer.WriteStartElement("component");
                writer.WriteAttributeString("p", "path", null, "/3D/Objects/object_1.model");
                writer.WriteAttributeString("objectid", "1");
                writer.WriteAttributeString("p", "uuid", null, "00010000-b206-40ff-9872-83e8017abed1");
                writer.WriteAttributeString("transform", "1 0 0 0 1 0 0 0 1 0 0 0");
                writer.WriteEndElement(); // component
                writer.WriteEndElement(); // components

                writer.WriteEndElement(); // object
                writer.WriteEndElement(); // resources

                // Build section
                writer.WriteStartElement("build");
                writer.WriteAttributeString("p", "uuid", null, "2c7c17d8-22b5-4d84-8835-1976022ea369");

                writer.WriteStartElement("item");
                writer.WriteAttributeString("objectid", "2");
                writer.WriteAttributeString("p", "uuid", null, "00000002-b1ec-4553-aec9-835e5b724bb4");
                writer.WriteAttributeString("transform", "1 0 0 0 1 0 0 0 1 128 128 2.5");
                writer.WriteAttributeString("printable", "1");
                writer.WriteEndElement(); // item

                writer.WriteEndElement(); // build

                writer.WriteEndElement(); // model
                writer.WriteEndDocument();
            }
        }


        private void CreateObjectFile(ZipArchive archive, InnerMap maze, List<MazePointPos> path)
        {
            var entry = archive.CreateEntry("3D/Objects/object_1.model");
            using (var stream = entry.Open())
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            }))
            {
                // Convert path to a HashSet for quick lookup
                var pathSet = new HashSet<(int x, int y)>();
                var pathPositions = new Dictionary<(int x, int y), byte>();

                foreach (var point in path)
                {
                    pathSet.Add((point.X, point.Y));
                    pathPositions[(point.X, point.Y)] = point.RelativePos;
                }

                writer.WriteStartDocument();
                writer.WriteStartElement("model", "http://schemas.microsoft.com/3dmanufacturing/core/2015/02");
                writer.WriteAttributeString("unit", "millimeter");

                // Resources section
                writer.WriteStartElement("resources");

                // Create a single combined mesh object
                CreateCombinedMesh(writer, maze, pathSet, pathPositions);

                writer.WriteEndElement(); // resources

                writer.WriteEndElement(); // model
                writer.WriteEndDocument();
            }
        }

        private void CreateMetadataFiles(ZipArchive archive)
        {
            // Cut information
            var cutEntry = archive.CreateEntry("Metadata/cut_information.xml");
            using (var stream = cutEntry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.CutInformation);
            }

            // Project settings
            var projectEntry = archive.CreateEntry("Metadata/project_settings.config");
            using (var stream = projectEntry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.ProjectSettings);
            }

            // Slice info
            var sliceEntry = archive.CreateEntry("Metadata/slice_info.config");
            using (var stream = sliceEntry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.SliceInfo);
            }
        }
        private void CreateCombinedMesh(XmlWriter writer, InnerMap maze, HashSet<(int x, int y)> pathSet, Dictionary<(int x, int y), byte> pathPositions)
        {
            writer.WriteStartElement("object");
            writer.WriteAttributeString("id", "1");
            writer.WriteAttributeString("type", "model");

            writer.WriteStartElement("mesh");

            var vertices = new List<(float x, float y, float z)>();
            var triangles = new List<(int v1, int v2, int v3, string paintColor)>();

            // Ground plane vertices and triangles
            AddGroundPlane(vertices, triangles, maze);

            // Optimized wall generation using MazeWall segments
            AddOptimizedWalls(vertices, triangles, maze, pathSet);

            // Optimized path generation using connected rectangular regions
            AddOptimizedPaths(vertices, triangles, maze, pathSet, pathPositions);

            // Write vertices
            writer.WriteStartElement("vertices");
            foreach (var (x, y, z) in vertices)
            {
                writer.WriteStartElement("vertex");
                writer.WriteAttributeString("x", x.ToString());
                writer.WriteAttributeString("y", y.ToString());
                writer.WriteAttributeString("z", z.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // vertices

            // Write triangles
            writer.WriteStartElement("triangles");
            foreach (var (v1, v2, v3, paintColor) in triangles)
            {
                writer.WriteStartElement("triangle");
                writer.WriteAttributeString("v1", v1.ToString());
                writer.WriteAttributeString("v2", v2.ToString());
                writer.WriteAttributeString("v3", v3.ToString());
                if (!string.IsNullOrEmpty(paintColor))
                {
                    writer.WriteAttributeString("paint_color", paintColor);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // triangles

            writer.WriteEndElement(); // mesh
            writer.WriteEndElement(); // object
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

        private void AddOptimizedWalls(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            // Generate optimized wall rectangles using T-intersection aware algorithm
            var wallRectangles = GenerateOptimizedWallRectangles(maze, pathSet);
            Console.WriteLine($"Generated {wallRectangles.Count} optimized wall rectangles");
            
            foreach (var rect in wallRectangles)
            {
                // Create a cuboid for the entire wall rectangle
                AddCuboid(vertices, triangles, rect.XStart, rect.YStart, rect.XEnd, rect.YEnd, 
                         GroundHeight, GroundHeight + WallHeight, Colors[0]); // Black walls
            }
        }

        private List<WallRectangle> GenerateOptimizedWallRectangles(InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            var rectangles = new List<WallRectangle>();
            
            // Create a modifiable set of wall cells
            var wallCells = new HashSet<(int x, int y)>();
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y)))
                    {
                        wallCells.Add((x, y));
                    }
                }
            }
            
            Console.WriteLine($"Starting with {wallCells.Count} wall cells");
            
            // Generate rectangles using intelligent T-intersection spanning
            var optimizedRectangles = CreateIntelligentWallRectangles(wallCells, maze.Width - 1, maze.Height - 1);
            
            Console.WriteLine($"Generated {optimizedRectangles.Count} optimized wall rectangles");
            return optimizedRectangles;
        }
        private List<WallRectangle> CreateIntelligentWallRectangles(HashSet<(int x, int y)> wallCells, int maxX, int maxY)
        {
            var rectangles = new List<WallRectangle>();
            var processedCells = new HashSet<(int x, int y)>();
            
            // First pass: prioritize very long horizontal segments
            ProcessLongHorizontalSegments(wallCells, processedCells, rectangles, maxX, maxY);
            
            // Second pass: prioritize very long vertical segments
            ProcessLongVerticalSegments(wallCells, processedCells, rectangles, maxX, maxY);
            
            // Third pass: handle remaining cells with normal logic
            ProcessRemainingCells(wallCells, processedCells, rectangles, maxX, maxY);
            
            Console.WriteLine($"Intelligent rectangle generation: {rectangles.Count} rectangles created");
            return rectangles;
        }
        
        private void ProcessLongHorizontalSegments(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                                  List<WallRectangle> rectangles, int maxX, int maxY)
        {
            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (processedCells.Contains((x, y)) || !wallCells.Contains((x, y)))
                        continue;
                        
                    // Check if this could start a long horizontal segment (≥5 cells)
                    int potentialWidth = GetMaximumHorizontalExtent(wallCells, processedCells, x, y, maxX);
                    if (potentialWidth >= 5)
                    {
                        var rect = CreateOptimalHorizontalRectangle(wallCells, processedCells, x, y, maxX, maxY);
                        if (rect != null)
                        {
                            rectangles.Add(rect);
                        }
                    }
                }
            }
        }
        
        private void ProcessLongVerticalSegments(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                                List<WallRectangle> rectangles, int maxX, int maxY)
        {
            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    if (processedCells.Contains((x, y)) || !wallCells.Contains((x, y)))
                        continue;
                        
                    // Check if this could start a long vertical segment (≥5 cells)
                    int potentialHeight = GetMaximumVerticalExtent(wallCells, processedCells, x, y, maxY);
                    if (potentialHeight >= 5)
                    {
                        // Create vertical rectangle without affecting processed cells yet
                        var rect = CreateOptimalVerticalRectangleWithMarking(wallCells, processedCells, x, y, maxX, maxY);
                        if (rect != null)
                        {
                            rectangles.Add(rect);
                        }
                    }
                }
            }
        }
        
        private void ProcessRemainingCells(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                          List<WallRectangle> rectangles, int maxX, int maxY)
        {
            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (processedCells.Contains((x, y)) || !wallCells.Contains((x, y)))
                        continue;
                        
                    var rect = CreateLargestRectangleWithTSpanning(wallCells, processedCells, x, y, maxX, maxY);
                    if (rect != null)
                    {
                        rectangles.Add(rect);
                    }
                }
            }
        }
        
        private int GetMaximumHorizontalExtent(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                              int startX, int startY, int maxX)
        {
            int width = 0;
            for (int x = startX; x <= maxX && wallCells.Contains((x, startY)) && !processedCells.Contains((x, startY)); x++)
            {
                width++;
            }
            return width;
        }
        
        private int GetMaximumVerticalExtent(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                            int startX, int startY, int maxY)
        {
            int height = 0;
            for (int y = startY; y <= maxY && wallCells.Contains((startX, y)) && !processedCells.Contains((startX, y)); y++)
            {
                height++;
            }
            return height;
        }
        
        private WallRectangle? CreateOptimalVerticalRectangleWithMarking(HashSet<(int x, int y)> wallCells, 
                                                                        HashSet<(int x, int y)> processedCells, 
                                                                        int startX, int startY, int maxX, int maxY)
        {
            // Find the maximum height while spanning through T-intersections when beneficial
            int height = 1;
            
            // Extend down as far as possible, potentially spanning through T-intersections
            while (startY + height <= maxY)
            {
                var currentPos = (startX, startY + height);
                
                if (!wallCells.Contains(currentPos) || processedCells.Contains(currentPos))
                    break;
                
                // Check if this position is a T-intersection
                if (IsTIntersection(wallCells, startX, startY + height, maxX, maxY))
                {
                    // Determine if we should span through this T-intersection
                    if (ShouldSpanThroughTIntersection(wallCells, startX, startY, startX, startY + height, false, maxX, maxY))
                    {
                        height++;
                        continue; // Continue through the T-intersection
                    }
                    else
                    {
                        break; // Don't span through this T-intersection
                    }
                }
                
                height++;
            }
            
            // Try to extend horizontally to create a wider rectangle
            int width = 1;
            bool canExtendRight = true;
            while (canExtendRight && startX + width <= maxX)
            {
                // Check if all cells in the next column are available
                for (int y = startY; y < startY + height; y++)
                {
                    var pos = (startX + width, y);
                    if (!wallCells.Contains(pos) || processedCells.Contains(pos))
                    {
                        canExtendRight = false;
                        break;
                    }
                }
                if (canExtendRight)
                {
                    width++;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + width,
                YEnd = startY + height
            };
        }
        
        private WallRectangle? CreateLargestRectangleWithTSpanning(HashSet<(int x, int y)> wallCells, 
                                                                  HashSet<(int x, int y)> processedCells, 
                                                                  int startX, int startY, int maxX, int maxY)
        {
            if (processedCells.Contains((startX, startY))) return null;
            
            // Try both horizontal and vertical rectangles, choose the one with larger area
            var horizontalRect = CreateOptimalHorizontalRectangle(wallCells, processedCells, startX, startY, maxX, maxY);
            var verticalRect = CreateOptimalVerticalRectangle(wallCells, processedCells, startX, startY, maxX, maxY);
            
            // Choose the rectangle with the larger area
            var chosenRect = horizontalRect;
            if (verticalRect != null && horizontalRect != null)
            {
                int hArea = (horizontalRect.XEnd - horizontalRect.XStart) * (horizontalRect.YEnd - horizontalRect.YStart);
                int vArea = (verticalRect.XEnd - verticalRect.XStart) * (verticalRect.YEnd - verticalRect.YStart);
                if (vArea > hArea)
                {
                    chosenRect = verticalRect;
                }
            }
            else if (verticalRect != null)
            {
                chosenRect = verticalRect;
            }
            
            return chosenRect;
        }
        
        private WallRectangle? CreateOptimalHorizontalRectangle(HashSet<(int x, int y)> wallCells, 
                                                               HashSet<(int x, int y)> processedCells, 
                                                               int startX, int startY, int maxX, int maxY)
        {
            // Find the maximum width while spanning through T-intersections when beneficial
            int width = 1;
            
            // Extend right as far as possible, potentially spanning through T-intersections
            while (startX + width <= maxX)
            {
                var currentPos = (startX + width, startY);
                
                if (!wallCells.Contains(currentPos) || processedCells.Contains(currentPos))
                    break;
                
                // Check if this position is a T-intersection
                if (IsTIntersection(wallCells, startX + width, startY, maxX, maxY))
                {
                    // Determine if we should span through this T-intersection
                    if (ShouldSpanThroughTIntersection(wallCells, startX, startY, startX + width, startY, true, maxX, maxY))
                    {
                        width++;
                        continue; // Continue through the T-intersection
                    }
                    else
                    {
                        break; // Don't span through this T-intersection
                    }
                }
                
                width++;
            }
            
            // Try to extend vertically to create a thicker rectangle
            int height = 1;
            bool canExtendDown = true;
            while (canExtendDown && startY + height <= maxY)
            {
                // Check if all cells in the next row are available and don't conflict with T-intersections
                for (int x = startX; x < startX + width; x++)
                {
                    var pos = (x, startY + height);
                    if (!wallCells.Contains(pos) || processedCells.Contains(pos))
                    {
                        canExtendDown = false;
                        break;
                    }
                }
                if (canExtendDown)
                {
                    height++;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + width,
                YEnd = startY + height
            };
        }
        
        private WallRectangle? CreateOptimalVerticalRectangle(HashSet<(int x, int y)> wallCells, 
                                                             HashSet<(int x, int y)> processedCells, 
                                                             int startX, int startY, int maxX, int maxY)
        {
            // Save the current state
            var savedProcessedCells = new HashSet<(int x, int y)>(processedCells);
            
            // Find the maximum height while spanning through T-intersections when beneficial
            int height = 1;
            
            // Extend down as far as possible, potentially spanning through T-intersections
            while (startY + height <= maxY)
            {
                var currentPos = (startX, startY + height);
                
                if (!wallCells.Contains(currentPos) || processedCells.Contains(currentPos))
                    break;
                
                // Check if this position is a T-intersection
                if (IsTIntersection(wallCells, startX, startY + height, maxX, maxY))
                {
                    // Determine if we should span through this T-intersection
                    if (ShouldSpanThroughTIntersection(wallCells, startX, startY, startX, startY + height, false, maxX, maxY))
                    {
                        height++;
                        continue; // Continue through the T-intersection
                    }
                    else
                    {
                        break; // Don't span through this T-intersection
                    }
                }
                
                height++;
            }
            
            // Try to extend horizontally to create a wider rectangle
            int width = 1;
            bool canExtendRight = true;
            while (canExtendRight && startX + width <= maxX)
            {
                // Check if all cells in the next column are available
                for (int y = startY; y < startY + height; y++)
                {
                    var pos = (startX + width, y);
                    if (!wallCells.Contains(pos) || processedCells.Contains(pos))
                    {
                        canExtendRight = false;
                        break;
                    }
                }
                if (canExtendRight)
                {
                    width++;
                }
            }
            
            // Only return this if it's different from a horizontal rectangle starting from the same point
            // Restore the processed cells state since we're just exploring
            processedCells.Clear();
            processedCells.UnionWith(savedProcessedCells);
            
            // Return the dimensions without marking cells as processed yet
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + width,
                YEnd = startY + height
            };
        }
        
        private bool IsTIntersection(HashSet<(int x, int y)> wallCells, int x, int y, int maxX, int maxY)
        {
            // Check the 4 orthogonal directions for wall presence
            bool left = x > 0 && wallCells.Contains((x - 1, y));
            bool right = x < maxX && wallCells.Contains((x + 1, y));
            bool up = y > 0 && wallCells.Contains((x, y - 1));
            bool down = y < maxY && wallCells.Contains((x, y + 1));
            
            int connections = (left ? 1 : 0) + (right ? 1 : 0) + (up ? 1 : 0) + (down ? 1 : 0);
            
            // T-intersection has exactly 3 connections
            return connections == 3;
        }
        
        private bool ShouldSpanThroughTIntersection(HashSet<(int x, int y)> wallCells, 
                                                   int lineStartX, int lineStartY, 
                                                   int tIntersectionX, int tIntersectionY, 
                                                   bool isHorizontalLine, int maxX, int maxY)
        {
            if (isHorizontalLine)
            {
                // For horizontal lines, be MORE AGGRESSIVE - prioritize horizontal continuity
                int horizontalLeft = 0;
                int horizontalRight = 0;
                int verticalStub = 0;
                
                // Measure horizontal extent from T-intersection
                for (int x = tIntersectionX - 1; x >= 0 && wallCells.Contains((x, tIntersectionY)); x--)
                    horizontalLeft++;
                for (int x = tIntersectionX + 1; x <= maxX && wallCells.Contains((x, tIntersectionY)); x++)
                    horizontalRight++;
                
                // Measure vertical stub from T-intersection
                bool hasUp = tIntersectionY > 0 && wallCells.Contains((tIntersectionX, tIntersectionY - 1));
                bool hasDown = tIntersectionY < maxY && wallCells.Contains((tIntersectionX, tIntersectionY + 1));
                
                if (hasUp)
                {
                    for (int y = tIntersectionY - 1; y >= 0 && wallCells.Contains((tIntersectionX, y)); y--)
                        verticalStub++;
                }
                if (hasDown)
                {
                    for (int y = tIntersectionY + 1; y <= maxY && wallCells.Contains((tIntersectionX, y)); y++)
                        verticalStub++;
                }
                
                int totalHorizontal = horizontalLeft + horizontalRight + 1; // +1 for the T-intersection itself
                
                // Be more aggressive: span if horizontal is equal or longer, OR if the stub is very short
                return totalHorizontal >= verticalStub || verticalStub <= 2;
            }
            else
            {
                // For vertical lines, be MORE AGGRESSIVE - prioritize vertical continuity
                int verticalUp = 0;
                int verticalDown = 0;
                int horizontalStub = 0;
                
                // Measure vertical extent from T-intersection
                for (int y = tIntersectionY - 1; y >= 0 && wallCells.Contains((tIntersectionX, y)); y--)
                    verticalUp++;
                for (int y = tIntersectionY + 1; y <= maxY && wallCells.Contains((tIntersectionX, y)); y++)
                    verticalDown++;
                
                // Measure horizontal stub from T-intersection
                bool hasLeft = tIntersectionX > 0 && wallCells.Contains((tIntersectionX - 1, tIntersectionY));
                bool hasRight = tIntersectionX < maxX && wallCells.Contains((tIntersectionX + 1, tIntersectionY));
                
                if (hasLeft)
                {
                    for (int x = tIntersectionX - 1; x >= 0 && wallCells.Contains((x, tIntersectionY)); x--)
                        horizontalStub++;
                }
                if (hasRight)
                {
                    for (int x = tIntersectionX + 1; x <= maxX && wallCells.Contains((x, tIntersectionY)); x++)
                        horizontalStub++;
                }
                
                int totalVertical = verticalUp + verticalDown + 1; // +1 for the T-intersection itself
                
                // Be more aggressive: span if vertical is equal or longer, OR if the stub is very short
                return totalVertical >= horizontalStub || horizontalStub <= 2;
            }
        }
        
        private List<WallRectangle> CreateOptimalWallLines(HashSet<(int x, int y)> wallCells, int maxX, int maxY)
        {
            var rectangles = new List<WallRectangle>();
            var processedCells = new HashSet<(int x, int y)>();
            
            // Priority 1: Create the longest possible horizontal lines first
            CreateLongestLines(wallCells, processedCells, rectangles, true, maxX, maxY);
            
            // Priority 2: Create the longest possible vertical lines from remaining cells
            CreateLongestLines(wallCells, processedCells, rectangles, false, maxX, maxY);
            
            // Priority 3: Handle any remaining individual cells as small rectangles
            CreateRemainingRectangles(wallCells, processedCells, rectangles, maxX, maxY);
            
            Console.WriteLine($"Line-based optimization: {rectangles.Count} rectangles created");
            return rectangles;
        }
        
        private void CreateLongestLines(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                      List<WallRectangle> rectangles, bool horizontal, int maxX, int maxY)
        {
            var direction = horizontal ? "horizontal" : "vertical";
            var linesCreated = 0;
            
            if (horizontal)
            {
                // Process row by row to find the longest horizontal segments
                for (int y = 0; y <= maxY; y++)
                {
                    for (int x = 0; x <= maxX; x++)
                    {
                        if (processedCells.Contains((x, y)) || !wallCells.Contains((x, y)))
                            continue;
                        
                        // Find the longest horizontal line starting from this point
                        var rect = CreateLongestHorizontalLine(wallCells, processedCells, x, y, maxX);
                        if (rect != null)
                        {
                            rectangles.Add(rect);
                            linesCreated++;
                        }
                    }
                }
            }
            else
            {
                // Process column by column to find the longest vertical segments
                for (int x = 0; x <= maxX; x++)
                {
                    for (int y = 0; y <= maxY; y++)
                    {
                        if (processedCells.Contains((x, y)) || !wallCells.Contains((x, y)))
                            continue;
                        
                        // Find the longest vertical line starting from this point
                        var rect = CreateLongestVerticalLine(wallCells, processedCells, x, y, maxY);
                        if (rect != null)
                        {
                            rectangles.Add(rect);
                            linesCreated++;
                        }
                    }
                }
            }
            
            Console.WriteLine($"Created {linesCreated} {direction} lines");
        }
        
        private WallRectangle? CreateLongestHorizontalLine(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                                          int startX, int startY, int maxX)
        {
            if (processedCells.Contains((startX, startY)))
                return null;
            
            // Find the maximum width for this horizontal line, ignoring T-intersections
            int width = 1;
            while (startX + width <= maxX && wallCells.Contains((startX + width, startY)))
            {
                // Skip over T-intersections - prioritize the horizontal line
                width++;
            }
            
            // Try to extend this line vertically to create a thicker rectangle if possible
            int height = 1;
            bool canExtendDown = true;
            while (canExtendDown && startY + height <= maxX) // Note: using maxX for height limit due to maze bounds
            {
                // Check if all cells in the next row are available
                for (int x = startX; x < startX + width; x++)
                {
                    if (!wallCells.Contains((x, startY + height)) || processedCells.Contains((x, startY + height)))
                    {
                        canExtendDown = false;
                        break;
                    }
                }
                if (canExtendDown)
                {
                    height++;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + width,
                YEnd = startY + height
            };
        }
        
        private WallRectangle? CreateLongestVerticalLine(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                                        int startX, int startY, int maxY)
        {
            if (processedCells.Contains((startX, startY)))
                return null;
            
            // Find the maximum height for this vertical line, ignoring T-intersections
            int height = 1;
            while (startY + height <= maxY && wallCells.Contains((startX, startY + height)))
            {
                // Skip over T-intersections - prioritize the vertical line
                height++;
            }
            
            // Try to extend this line horizontally to create a wider rectangle if possible
            int width = 1;
            bool canExtendRight = true;
            while (canExtendRight && startX + width <= maxY) // Note: using maxY for width limit due to maze bounds
            {
                // Check if all cells in the next column are available
                for (int y = startY; y < startY + height; y++)
                {
                    if (!wallCells.Contains((startX + width, y)) || processedCells.Contains((startX + width, y)))
                    {
                        canExtendRight = false;
                        break;
                    }
                }
                if (canExtendRight)
                {
                    width++;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + width,
                YEnd = startY + height
            };
        }
        
        private void CreateRemainingRectangles(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                             List<WallRectangle> rectangles, int maxX, int maxY)
        {
            var remainingCount = 0;
            
            foreach (var (x, y) in wallCells)
            {
                if (processedCells.Contains((x, y)))
                    continue;
                
                // Create individual cell rectangles for any remaining cells
                processedCells.Add((x, y));
                rectangles.Add(new WallRectangle
                {
                    XStart = x,
                    YStart = y,
                    XEnd = x + 1,
                    YEnd = y + 1
                });
                remainingCount++;
            }
            
            if (remainingCount > 0)
            {
                Console.WriteLine($"Created {remainingCount} individual cell rectangles for remaining cells");
            }
        }


        private void AddOptimizedPaths(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, 
                                     InnerMap maze, HashSet<(int x, int y)> pathSet, Dictionary<(int x, int y), byte> pathPositions)
        {
            // Generate path rectangles by finding connected rectangular regions
            var pathRectangles = GeneratePathRectangles(maze, pathSet, pathPositions);
            
            foreach (var pathRect in pathRectangles)
            {
                AddCuboid(vertices, triangles, pathRect.XStart, pathRect.YStart, pathRect.XEnd, pathRect.YEnd,
                         GroundHeight, GroundHeight + PathHeight, pathRect.Color);
            }
        }

        private List<WallRectangle> GenerateWallRectangles(InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            var rectangles = new List<WallRectangle>();
            var processedCells = new HashSet<(int x, int y)>();
            
            // Step 1: Identify all wall cells
            var wallCells = new HashSet<(int x, int y)>();
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y)))
                    {
                        wallCells.Add((x, y));
                    }
                }
            }
            
            // Step 2: Priority-based rectangle generation
            // First pass: Generate longer horizontal rectangles
            GeneratePriorityRectangles(wallCells, processedCells, rectangles, true, maze.Width - 1, maze.Height - 1);
            
            // Second pass: Generate remaining vertical rectangles
            GeneratePriorityRectangles(wallCells, processedCells, rectangles, false, maze.Width - 1, maze.Height - 1);
            
            return rectangles;
        }

        private void GeneratePriorityRectangles(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                               List<WallRectangle> rectangles, bool prioritizeHorizontal, int maxX, int maxY)
        {
            // Create a list of potential rectangle starting points with their priorities
            var prioritizedCandidates = new List<(int x, int y, int priority)>();
            
            foreach (var (x, y) in wallCells)
            {
                if (processedCells.Contains((x, y))) continue;
                
                int priority = CalculateRectanglePriority(wallCells, x, y, prioritizeHorizontal, maxX, maxY);
                if (priority > 0)
                {
                    prioritizedCandidates.Add((x, y, priority));
                }
            }
            
            // Sort by priority (highest first) to process the most promising rectangles first
            prioritizedCandidates.Sort((a, b) => b.priority.CompareTo(a.priority));
            
            foreach (var (x, y, priority) in prioritizedCandidates)
            {
                if (processedCells.Contains((x, y))) continue;
                
                var rect = FindOptimalRectangle(wallCells, processedCells, x, y, prioritizeHorizontal, maxX, maxY);
                if (rect != null)
                {
                    rectangles.Add(rect);
                }
            }
        }
        
        private int CalculateRectanglePriority(HashSet<(int x, int y)> wallCells, int x, int y, bool prioritizeHorizontal, int maxX, int maxY)
        {
            if (prioritizeHorizontal)
            {
                // For horizontal priority, measure potential horizontal extent
                int leftExtent = GetHorizontalExtent(wallCells, x, y, -1);
                int rightExtent = GetHorizontalExtent(wallCells, x, y, 1);
                int totalHorizontal = leftExtent + rightExtent + 1;
                
                // Check if this is part of a T-intersection where horizontal should be prioritized
                bool isOptimalTPosition = IsOptimalTIntersection(wallCells, x, y, true, maxX, maxY);
                
                return totalHorizontal * (isOptimalTPosition ? 10 : 1); // Boost priority for T-intersections
            }
            else
            {
                // For vertical priority, measure potential vertical extent
                int upExtent = GetVerticalExtent(wallCells, x, y, -1);
                int downExtent = GetVerticalExtent(wallCells, x, y, 1);
                int totalVertical = upExtent + downExtent + 1;
                
                // Check if this is part of a T-intersection where vertical should be prioritized
                bool isOptimalTPosition = IsOptimalTIntersection(wallCells, x, y, false, maxX, maxY);
                
                return totalVertical * (isOptimalTPosition ? 10 : 1); // Boost priority for T-intersections
            }
        }
        
        private int GetHorizontalExtent(HashSet<(int x, int y)> wallCells, int x, int y, int direction)
        {
            int extent = 0;
            int currentX = x + direction;
            
            while (wallCells.Contains((currentX, y)))
            {
                extent++;
                currentX += direction;
            }
            
            return extent;
        }
        
        private int GetVerticalExtent(HashSet<(int x, int y)> wallCells, int x, int y, int direction)
        {
            int extent = 0;
            int currentY = y + direction;
            
            while (wallCells.Contains((x, currentY)))
            {
                extent++;
                currentY += direction;
            }
            
            return extent;
        }
        
        private bool IsOptimalTIntersection(HashSet<(int x, int y)> wallCells, int x, int y, bool checkingHorizontal, int maxX, int maxY)
        {
            // Check the 4 orthogonal directions for wall presence
            bool left = x > 0 && wallCells.Contains((x - 1, y));
            bool right = x < maxX && wallCells.Contains((x + 1, y));
            bool up = y > 0 && wallCells.Contains((x, y - 1));
            bool down = y < maxY && wallCells.Contains((x, y + 1));
            
            int orthogonalConnections = (left ? 1 : 0) + (right ? 1 : 0) + (up ? 1 : 0) + (down ? 1 : 0);
            
            if (orthogonalConnections != 3) return false; // Not a T-intersection
            
            if (checkingHorizontal)
            {
                // For horizontal priority, favor T-intersections where horizontal is the main line
                return left && right && (up || down); // T-up or T-down configurations
            }
            else
            {
                // For vertical priority, favor T-intersections where vertical is the main line
                return up && down && (left || right); // T-left or T-right configurations
            }
        }
        
        private WallRectangle FindOptimalRectangle(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                                  int startX, int startY, bool prioritizeHorizontal, int maxX, int maxY)
        {
            if (processedCells.Contains((startX, startY))) return null;
            
            int bestWidth = 1;
            int bestHeight = 1;
            int maxArea = 1;
            
            if (prioritizeHorizontal)
            {
                // For horizontal priority, try to maximize width first
                int maxWidth = FindMaximumWidth(wallCells, processedCells, startX, startY, maxX);
                
                for (int width = maxWidth; width >= 1; width--) // Start with maximum width
                {
                    int height = FindMaximumHeightForWidth(wallCells, processedCells, startX, startY, width, maxY);
                    int area = width * height;
                    
                    if (area > maxArea)
                    {
                        maxArea = area;
                        bestWidth = width;
                        bestHeight = height;
                    }
                }
            }
            else
            {
                // For vertical priority, try to maximize height first
                int maxHeight = FindMaximumHeight(wallCells, processedCells, startX, startY, maxY);
                
                for (int height = maxHeight; height >= 1; height--) // Start with maximum height
                {
                    int width = FindMaximumWidthForHeight(wallCells, processedCells, startX, startY, height, maxX);
                    int area = width * height;
                    
                    if (area > maxArea)
                    {
                        maxArea = area;
                        bestWidth = width;
                        bestHeight = height;
                    }
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + bestHeight; y++)
            {
                for (int x = startX; x < startX + bestWidth; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + bestWidth,
                YEnd = startY + bestHeight
            };
        }
        
        private int FindMaximumWidth(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, int startX, int startY, int maxX)
        {
            int width = 1;
            while (startX + width < maxX && 
                   wallCells.Contains((startX + width, startY)) && 
                   !processedCells.Contains((startX + width, startY)))
            {
                width++;
            }
            return width;
        }
        
        private int FindMaximumHeight(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, int startX, int startY, int maxY)
        {
            int height = 1;
            while (startY + height < maxY && 
                   wallCells.Contains((startX, startY + height)) && 
                   !processedCells.Contains((startX, startY + height)))
            {
                height++;
            }
            return height;
        }
        
        private int FindMaximumWidthForHeight(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                             int startX, int startY, int height, int maxX)
        {
            int width = 1;
            while (startX + width < maxX)
            {
                bool canExtendWidth = true;
                for (int y = startY; y < startY + height; y++)
                {
                    if (!wallCells.Contains((startX + width, y)) || processedCells.Contains((startX + width, y)))
                    {
                        canExtendWidth = false;
                        break;
                    }
                }
                
                if (canExtendWidth)
                {
                    width++;
                }
                else
                {
                    break;
                }
            }
            return width;
        }
        
        private int FindMaximumHeightForWidth(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, 
                                             int startX, int startY, int width, int maxY)
        {
            int height = 1;
            while (startY + height < maxY)
            {
                bool canExtendHeight = true;
                for (int x = startX; x < startX + width; x++)
                {
                    if (!wallCells.Contains((x, startY + height)) || processedCells.Contains((x, startY + height)))
                    {
                        canExtendHeight = false;
                        break;
                    }
                }
                
                if (canExtendHeight)
                {
                    height++;
                }
                else
                {
                    break;
                }
            }
            return height;
        }
        
        private WallRectangle FindLargestOptimizedWallRectangle(HashSet<(int x, int y)> wallCells, HashSet<(int x, int y)> processedCells, int startX, int startY, int maxX, int maxY)
        {
            // Use a dynamic programming approach to find the largest rectangle
            int maxWidth = 1;
            int bestWidth = 1;
            int bestHeight = 1;
            int maxArea = 1;
            
            // Find maximum width at the starting row
            while (startX + maxWidth < maxX && 
                   wallCells.Contains((startX + maxWidth, startY)) &&
                   !processedCells.Contains((startX + maxWidth, startY)))
            {
                maxWidth++;
            }
            
            // For each possible width, find the maximum height
            for (int width = 1; width <= maxWidth; width++)
            {
                int height = 1;
                
                // Find maximum height that works for this width
                while (startY + height < maxY)
                {
                    bool canExtendHeight = true;
                    
                    // Check if the entire row can be added
                    for (int x = startX; x < startX + width; x++)
                    {
                        if (!wallCells.Contains((x, startY + height)) ||
                            processedCells.Contains((x, startY + height)))
                        {
                            canExtendHeight = false;
                            break;
                        }
                    }
                    
                    if (canExtendHeight)
                    {
                        height++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // Check if this rectangle is larger than our current best
                int area = width * height;
                if (area > maxArea)
                {
                    maxArea = area;
                    bestWidth = width;
                    bestHeight = height;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + bestHeight; y++)
            {
                for (int x = startX; x < startX + bestWidth; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new WallRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + bestWidth,
                YEnd = startY + bestHeight
            };
        }



        private List<PathRectangle> GeneratePathRectangles(InnerMap maze, HashSet<(int x, int y)> pathSet, Dictionary<(int x, int y), byte> pathPositions)
        {
            var rectangles = new List<PathRectangle>();
            var processedCells = new HashSet<(int x, int y)>();
            
            foreach (var (x, y) in pathSet)
            {
                if (x >= maze.Width - 1 || y >= maze.Height - 1 || !maze[x, y] || processedCells.Contains((x, y)))
                    continue;
                
                // Find the largest rectangle starting from this point
                var rect = FindLargestPathRectangle(maze, pathSet, pathPositions, processedCells, x, y);
                if (rect != null)
                {
                    rectangles.Add(rect);
                }
            }
            
            return rectangles;
        }

        private PathRectangle FindLargestPathRectangle(InnerMap maze, HashSet<(int x, int y)> pathSet, Dictionary<(int x, int y), byte> pathPositions, 
                                                     HashSet<(int x, int y)> processedCells, int startX, int startY)
        {
            // Determine the dominant color for this region (based on the starting point)
            var startRelativePos = pathPositions[(startX, startY)];
            var dominantColor = startRelativePos < 128 ? Colors[2] : Colors[3];
            
            // Try to find the largest rectangle that can be formed
            int maxWidth = 1;
            int maxHeight = 1;
            
            // Find maximum width at the starting row
            while (startX + maxWidth < maze.Width - 1 && 
                   pathSet.Contains((startX + maxWidth, startY)) && 
                   maze[startX + maxWidth, startY] &&
                   !processedCells.Contains((startX + maxWidth, startY)))
            {
                var relativePos = pathPositions[(startX + maxWidth, startY)];
                var cellColor = relativePos < 128 ? Colors[2] : Colors[3];
                if (cellColor != dominantColor) break;
                maxWidth++;
            }
            
            // Find maximum height that works for this width
            bool canExtendHeight = true;
            while (canExtendHeight && startY + maxHeight < maze.Height - 1)
            {
                // Check if the entire row can be added
                for (int x = startX; x < startX + maxWidth; x++)
                {
                    if (!pathSet.Contains((x, startY + maxHeight)) || 
                        !maze[x, startY + maxHeight] ||
                        processedCells.Contains((x, startY + maxHeight)))
                    {
                        canExtendHeight = false;
                        break;
                    }
                    
                    var relativePos = pathPositions[(x, startY + maxHeight)];
                    var cellColor = relativePos < 128 ? Colors[2] : Colors[3];
                    if (cellColor != dominantColor)
                    {
                        canExtendHeight = false;
                        break;
                    }
                }
                
                if (canExtendHeight)
                {
                    maxHeight++;
                }
            }
            
            // Mark all cells in this rectangle as processed
            for (int y = startY; y < startY + maxHeight; y++)
            {
                for (int x = startX; x < startX + maxWidth; x++)
                {
                    processedCells.Add((x, y));
                }
            }
            
            return new PathRectangle
            {
                XStart = startX,
                YStart = startY,
                XEnd = startX + maxWidth,
                YEnd = startY + maxHeight,
                Color = dominantColor
            };
        }

        private void AddCuboid(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, 
                             int xStart, int yStart, int xEnd, int yEnd, float zBottom, float zTop, string paintColor)
        {
            int baseIndex = vertices.Count;

            // Bottom vertices
            vertices.Add((xStart, yStart, zBottom));         // 0
            vertices.Add((xEnd, yStart, zBottom));           // 1
            vertices.Add((xEnd, yEnd, zBottom));             // 2
            vertices.Add((xStart, yEnd, zBottom));           // 3

            // Top vertices
            vertices.Add((xStart, yStart, zTop));            // 4
            vertices.Add((xEnd, yStart, zTop));              // 5
            vertices.Add((xEnd, yEnd, zTop));                // 6
            vertices.Add((xStart, yEnd, zTop));              // 7

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

        private class PathRectangle
        {
            public int XStart { get; set; }
            public int YStart { get; set; }
            public int XEnd { get; set; }
            public int YEnd { get; set; }
            public string Color { get; set; } = string.Empty;
        }

        private class WallRectangle
        {
            public int XStart { get; set; }
            public int YStart { get; set; }
            public int XEnd { get; set; }
            public int YEnd { get; set; }
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

        private void CreateModelSettingsFile(ZipArchive archive, InnerMap maze, List<MazePointPos> path)
        {
            // Calculate face count based on the mesh that will be generated
            var pathSet = new HashSet<(int x, int y)>();
            foreach (var point in path)
            {
                pathSet.Add((point.X, point.Y));
            }

            var faceCount = CalculateFaceCount(maze, pathSet);

            var entry = archive.CreateEntry("Metadata/model_settings.config");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write($"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <config>
                      <object id="2">
                        <metadata key="name" value="Maze_Coaster"/>
                        <metadata key="extruder" value="1"/>
                        <metadata key="face_count" value="{faceCount}"/>
                        <part id="1" subtype="normal_part">
                          <metadata key="name" value="Maze_Coaster"/>
                          <metadata key="matrix" value="1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"/>
                          <metadata key="source_file" value="maze_coaster.3mf"/>
                          <metadata key="source_object_id" value="0"/>
                          <metadata key="source_volume_id" value="0"/>
                          <metadata key="source_offset_x" value="9.5"/>
                          <metadata key="source_offset_y" value="9.5"/>
                          <metadata key="source_offset_z" value="2.5"/>
                          <mesh_stat face_count="{faceCount}" edges_fixed="0" degenerate_facets="0" facets_removed="0" facets_reversed="0" backwards_edges="0"/>
                        </part>
                      </object>
                      <plate>
                        <metadata key="plater_id" value="1"/>
                        <metadata key="plater_name" value=""/>
                        <metadata key="locked" value="false"/>
                        <metadata key="filament_map_mode" value="Auto For Flush"/>
                        <metadata key="thumbnail_file" value="Metadata/plate_1.png"/>
                        <metadata key="thumbnail_no_light_file" value="Metadata/plate_no_light_1.png"/>
                        <metadata key="top_file" value="Metadata/top_1.png"/>
                        <metadata key="pick_file" value="Metadata/pick_1.png"/>
                        <model_instance>
                          <metadata key="object_id" value="2"/>
                          <metadata key="instance_id" value="0"/>
                          <metadata key="identify_id" value="84"/>
                        </model_instance>
                      </plate>
                      <assemble>
                       <assemble_item object_id="2" instance_id="0" transform="1 0 0 0 1 0 0 0 1 0 0 0" offset="0 0 0"/>
                      </assemble>
                    </config>
                    """);
            }
        }

        private int CalculateFaceCount(InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            // Ground plane: 12 triangles (6 faces * 2 triangles per face)
            int groundFaces = 12;

            // Count optimized wall rectangles
            var wallRectangles = GenerateOptimizedWallRectangles(maze, pathSet);
            int wallCuboids = wallRectangles.Count;

            // Count optimized path cuboids
            var pathPositions = new Dictionary<(int x, int y), byte>();
            foreach (var (x, y) in pathSet)
            {
                pathPositions[(x, y)] = 128; // Default value for face count calculation
            }
            var pathRectangles = GeneratePathRectangles(maze, pathSet, pathPositions);
            int pathCuboids = pathRectangles.Count;

            // Each cuboid has 12 triangles (6 faces * 2 triangles per face)
            int cuboidFaces = (wallCuboids + pathCuboids) * 12;

            return groundFaces + cuboidFaces;
        }

        private int CalculateVertexCount(InnerMap maze, HashSet<(int x, int y)> pathSet)
        {
            // Ground plane: 8 vertices
            int groundVertices = 8;

            // Count optimized wall rectangles
            var wallRectangles = GenerateOptimizedWallRectangles(maze, pathSet);
            int wallCuboids = wallRectangles.Count;

            // Count optimized path cuboids
            var pathPositions = new Dictionary<(int x, int y), byte>();
            foreach (var (x, y) in pathSet)
            {
                pathPositions[(x, y)] = 128; // Default value for vertex count calculation
            }
            var pathRectangles = GeneratePathRectangles(maze, pathSet, pathPositions);
            int pathCuboids = pathRectangles.Count;

            // Each cuboid has 8 vertices (no sharing accounted for in this approximation)
            int cuboidVertices = (wallCuboids + pathCuboids) * 8;

            return groundVertices + cuboidVertices;
        }

        public (int triangles, int vertices) CalculateMeshStats(InnerMap maze, List<MazePointPos> path)
        {
            var pathSet = new HashSet<(int x, int y)>();
            foreach (var point in path)
            {
                pathSet.Add((point.X, point.Y));
            }

            var triangles = CalculateFaceCount(maze, pathSet);
            var vertices = CalculateVertexCount(maze, pathSet);
            
            return (triangles, vertices);
        }

        public string GenerateFilenameWithStats(int mazeSize, int? seed, int triangles, int vertices)
        {
            var usedSeed = seed ?? 1337;
            return $"maze_coaster_{mazeSize}x{mazeSize}_seed{usedSeed}_{triangles}tri_{vertices}vert.3mf";
        }

        public string Generate3MFCoasterWithStats(int mazeSize, int? seed = null)
        {
            Console.WriteLine($"Generating {mazeSize}x{mazeSize} maze...");

            // Generate maze using AlgorithmBacktrack2Deluxe2_AsByte
            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();
            var actionThing = new NoAction();

            var usedSeed = seed ?? 1337;
            var maze = alg.GoGenerate(mazeSize, mazeSize, usedSeed, innerMapFactory, randomFactory, actionThing);

            Console.WriteLine("Finding path through maze...");

            // Find the path with position information
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);

            Console.WriteLine($"Path found with {path.Count} points (seed: {usedSeed})");
            
            // Calculate mesh statistics
            var (triangles, vertices) = CalculateMeshStats(maze.InnerMap, path);
            Console.WriteLine($"Mesh stats: {triangles} triangles, {vertices} vertices");
            
            // Generate filename with stats
            var filename = GenerateFilenameWithStats(mazeSize, seed, triangles, vertices);
            
            Console.WriteLine("Generating 3MF file...");

            // Generate the 3MF file
            Generate3MFFile(maze.InnerMap, path, filename);
            
            return filename;
        }

        private void CreateThumbnailImages(ZipArchive archive, InnerMap maze, List<MazePointPos> path)
        {
            // Generate the base maze image once into a memory stream
            using (var baseImageStream = new MemoryStream())
            {
                WithPath.SaveMazeAsImageDeluxePng(maze, path, baseImageStream);
                baseImageStream.Position = 0;

                // Load the base image once
                using (var baseImage = Image.Load<Argb32>(baseImageStream))
                {
                    // Generate the different thumbnails required by Bambu Studio
                    CreateThumbnailFromBase(archive, baseImage, "Metadata/plate_1.png", 512, 512, false);
                    CreateThumbnailFromBase(archive, baseImage, "Metadata/plate_1_small.png", 128, 128, false);
                    CreateThumbnailFromBase(archive, baseImage, "Metadata/plate_no_light_1.png", 512, 512, true);
                    CreateThumbnailFromBase(archive, baseImage, "Metadata/top_1.png", 512, 512, false);
                }
            }
        }

        private void CreateThumbnailFromBase(ZipArchive archive, Image<Argb32> baseImage, string filename, int width, int height, bool noLight)
        {
            var entry = archive.CreateEntry(filename);
            using (var stream = entry.Open())
            {
                // Clone the base image and resize with nearest neighbor (no interpolation) for hard edges
                using (var resizedImage = baseImage.Clone(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Sampler = KnownResamplers.NearestNeighbor // Keep hard edges for maze
                })))
                {
                    // Apply "no light" effect by darkening the image
                    if (noLight)
                    {
                        resizedImage.Mutate(x => x.Brightness(0.7f));
                    }

                    // Save as PNG with maximum compression
                    resizedImage.SaveAsPng(stream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });
                }
            }
        }
    }
}