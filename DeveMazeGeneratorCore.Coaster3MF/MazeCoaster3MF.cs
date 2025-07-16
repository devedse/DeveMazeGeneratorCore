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

        public string Generate3MFCoaster(string filename, int mazeSize, int? seed = null)
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
            Console.WriteLine("Generating mesh data to determine vertex and triangle counts...");

            // Generate mesh data to get vertex and triangle counts
            var (vertexCount, triangleCount) = CalculateMeshCounts(maze.InnerMap, path);
            
            // Create filename with mesh statistics
            var actualFilename = $"maze_coaster_{mazeSize}x{mazeSize}_seed{usedSeed}_{triangleCount}tri_{vertexCount}vert.3mf";
            Console.WriteLine($"Creating {actualFilename} ({vertexCount} vertices, {triangleCount} triangles)...");

            // Generate the 3MF file
            Generate3MFFile(maze.InnerMap, path, actualFilename);
            
            return actualFilename;
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
        private (int vertexCount, int triangleCount) CalculateMeshCounts(InnerMap maze, List<MazePointPos> path)
        {
            // Convert path to a HashSet for quick lookup
            var pathSet = new HashSet<(int x, int y)>();
            var pathPositions = new Dictionary<(int x, int y), byte>();

            foreach (var point in path)
            {
                pathSet.Add((point.X, point.Y));
                pathPositions[(point.X, point.Y)] = point.RelativePos;
            }

            var vertices = new List<(float x, float y, float z)>();
            var triangles = new List<(int v1, int v2, int v3, string paintColor)>();

            // Ground plane vertices and triangles
            AddGroundPlane(vertices, triangles, maze);

            // Generate optimized mesh using greedy meshing algorithm
            GenerateOptimizedMesh(vertices, triangles, maze, pathSet, pathPositions);

            return (vertices.Count, triangles.Count);
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

            // Generate optimized mesh using greedy meshing algorithm
            GenerateOptimizedMesh(vertices, triangles, maze, pathSet, pathPositions);

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

        private void GenerateOptimizedMesh(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, InnerMap maze, HashSet<(int x, int y)> pathSet, Dictionary<(int x, int y), byte> pathPositions)
        {
            Console.WriteLine("Using rectangle-first optimization approach...");
            
            // Create material map for the maze
            var materialMap = new string[maze.Width - 1, maze.Height - 1];
            var heightMap = new float[maze.Width - 1, maze.Height - 1];
            
            // Fill material and height maps
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (pathSet.Contains((x, y)) && maze[x, y])
                    {
                        // Path position
                        var relativePos = pathPositions[(x, y)];
                        materialMap[x, y] = relativePos < 128 ? Colors[2] : Colors[3];
                        heightMap[x, y] = PathHeight;
                    }
                    else if (!maze[x, y])
                    {
                        // Wall position
                        materialMap[x, y] = Colors[0];
                        heightMap[x, y] = WallHeight;
                    }
                    else
                    {
                        // Empty space (no cube)
                        materialMap[x, y] = "";  // Use empty string instead of null
                        heightMap[x, y] = 0;
                    }
                }
            }
            
            // Generate optimized rectangles first, then create geometry
            GenerateOptimizedRectangles(vertices, triangles, materialMap, heightMap, maze.Width - 1, maze.Height - 1);
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

            // Bottom face (z = zBottom)
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, paintColor));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, paintColor));

            // Top face (z = zTop)
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

        private void GenerateOptimizedRectangles(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, int width, int height)
        {
            // Create vertex cache for sharing vertices between adjacent rectangles
            var vertexCache = new Dictionary<(float x, float y, float z), int>();
            
            // Find all unique materials
            var materials = new HashSet<string>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!string.IsNullOrEmpty(materialMap[x, y]))
                    {
                        materials.Add(materialMap[x, y]);
                    }
                }
            }

            // Process each material separately with rectangle-first optimization
            foreach (var material in materials)
            {
                OptimizeRectanglesForMaterial(vertices, triangles, materialMap, heightMap, width, height, material, vertexCache);
            }
        }

        private void OptimizeRectanglesForMaterial(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, int width, int height, string targetMaterial, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            Console.WriteLine($"  Optimizing rectangles for material {targetMaterial}...");
            
            // Step 1: Find all connected components for this material
            var components = FindConnectedComponents(materialMap, heightMap, width, height, targetMaterial);
            
            int totalRectangles = 0;
            int totalSquares = 0;
            
            foreach (var component in components)
            {
                totalSquares += component.Count;
                
                // Step 2: Convert component to rectangle set and optimize
                var optimizedRectangles = OptimizeComponentToRectangles(component, heightMap);
                totalRectangles += optimizedRectangles.Count;
                
                // Step 3: Create geometry for optimized rectangles
                foreach (var rect in optimizedRectangles)
                {
                    CreateRectangleGeometryShared(vertices, triangles, rect.x, rect.y, rect.width, rect.height, targetMaterial, rect.cubeHeight, vertexCache);
                }
            }
            
            Console.WriteLine($"  Material {targetMaterial}: {totalSquares} squares -> {totalRectangles} rectangles ({100 - (totalRectangles * 100 / Math.Max(1, totalSquares)):F1}% reduction)");
        }

        private List<(int x, int y, int width, int height, float cubeHeight)> OptimizeComponentToRectangles(List<(int x, int y)> component, float[,] heightMap)
        {
            if (component.Count == 0) return new List<(int x, int y, int width, int height, float cubeHeight)>();
            
            // Get the height for this component (all squares should have the same height)
            var targetHeight = heightMap[component[0].x, component[0].y];
            
            // Start with individual unit rectangles for each square
            var rectangles = component.Select(sq => (sq.x, sq.y, width: 1, height: 1, cubeHeight: targetHeight)).ToList();
            
            // Apply multiple optimization passes
            rectangles = ApplyAggressiveRectangleMerging(rectangles, component);
            
            return rectangles;
        }

        private List<(int x, int y, int width, int height, float cubeHeight)> ApplyAggressiveRectangleMerging(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, List<(int x, int y)> component)
        {
            var componentSet = new HashSet<(int x, int y)>(component);
            bool improved = true;
            int iteration = 0;
            const int maxIterations = 50;
            
            while (improved && iteration < maxIterations)
            {
                improved = false;
                iteration++;
                int beforeCount = rectangles.Count;
                
                // Strategy 1: Simple adjacent merging (horizontal and vertical)
                rectangles = MergeAdjacentRectangles(rectangles);
                
                // Strategy 2: Complex shape merging using component validation
                rectangles = MergeComplexShapes(rectangles, componentSet);
                
                // Strategy 3: Multi-rectangle merging for large shapes
                if (iteration <= 25) // Only try this in early iterations to avoid infinite loops
                {
                    rectangles = MergeMultipleRectangles(rectangles, componentSet);
                }
                
                int afterCount = rectangles.Count;
                if (afterCount < beforeCount)
                {
                    improved = true;
                    Console.WriteLine($"    Iteration {iteration}: {beforeCount} -> {afterCount} rectangles");
                }
            }
            
            return rectangles;
        }

        private List<(int x, int y, int width, int height, float cubeHeight)> MergeAdjacentRectangles(List<(int x, int y, int width, int height, float cubeHeight)> rectangles)
        {
            var result = new List<(int x, int y, int width, int height, float cubeHeight)>();
            var processed = new HashSet<int>();
            
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var rect1 = rectangles[i];
                bool merged = false;
                
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var rect2 = rectangles[j];
                    if (rect1.cubeHeight != rect2.cubeHeight) continue;
                    
                    var mergedRect = TryMergeRectangles(rect1, rect2);
                    if (mergedRect.HasValue)
                    {
                        result.Add(mergedRect.Value);
                        processed.Add(i);
                        processed.Add(j);
                        merged = true;
                        break;
                    }
                }
                
                if (!merged)
                {
                    result.Add(rect1);
                    processed.Add(i);
                }
            }
            
            return result;
        }

        private List<(int x, int y, int width, int height, float cubeHeight)> MergeComplexShapes(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, HashSet<(int x, int y)> componentSet)
        {
            var result = new List<(int x, int y, int width, int height, float cubeHeight)>();
            var processed = new HashSet<int>();
            
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var rect1 = rectangles[i];
                bool merged = false;
                
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var rect2 = rectangles[j];
                    if (rect1.cubeHeight != rect2.cubeHeight) continue;
                    
                    // Try complex merge (checks if bounding box is fully covered by component)
                    var complexMerge = TryComplexMerge(rect1, rect2, componentSet);
                    if (complexMerge.HasValue)
                    {
                        result.Add(complexMerge.Value);
                        processed.Add(i);
                        processed.Add(j);
                        merged = true;
                        break;
                    }
                }
                
                if (!merged)
                {
                    result.Add(rect1);
                    processed.Add(i);
                }
            }
            
            return result;
        }

        private List<(int x, int y, int width, int height, float cubeHeight)> MergeMultipleRectangles(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, HashSet<(int x, int y)> componentSet)
        {
            var result = new List<(int x, int y, int width, int height, float cubeHeight)>();
            var processed = new HashSet<int>();
            
            // Try to merge 3 or more rectangles into larger shapes
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var candidates = new List<int> { i };
                var baseRect = rectangles[i];
                
                // Find all rectangles that could potentially be merged with this one
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var testRect = rectangles[j];
                    if (testRect.cubeHeight != baseRect.cubeHeight) continue;
                    
                    // Check if this rectangle is adjacent or could form a larger shape
                    if (AreRectanglesConnected(baseRect, testRect) || CanFormLargerShape(candidates.Select(idx => rectangles[idx]).ToList(), testRect, componentSet))
                    {
                        candidates.Add(j);
                    }
                }
                
                // If we have multiple candidates, try to merge them
                if (candidates.Count > 1)
                {
                    var mergedRect = TryMergeMultipleRectangles(candidates.Select(idx => rectangles[idx]).ToList(), componentSet);
                    if (mergedRect.HasValue)
                    {
                        result.Add(mergedRect.Value);
                        foreach (var idx in candidates)
                        {
                            processed.Add(idx);
                        }
                        continue;
                    }
                }
                
                result.Add(rectangles[i]);
                processed.Add(i);
            }
            
            return result;
        }

        private bool AreRectanglesConnected((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            // Check if rectangles are touching (adjacent)
            return (rect1.x + rect1.width == rect2.x && OverlapsVertically(rect1, rect2)) ||
                   (rect2.x + rect2.width == rect1.x && OverlapsVertically(rect1, rect2)) ||
                   (rect1.y + rect1.height == rect2.y && OverlapsHorizontally(rect1, rect2)) ||
                   (rect2.y + rect2.height == rect1.y && OverlapsHorizontally(rect1, rect2));
        }

        private bool OverlapsVertically((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            return !(rect1.y + rect1.height <= rect2.y || rect2.y + rect2.height <= rect1.y);
        }

        private bool OverlapsHorizontally((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            return !(rect1.x + rect1.width <= rect2.x || rect2.x + rect2.width <= rect1.x);
        }

        private bool CanFormLargerShape(List<(int x, int y, int width, int height, float cubeHeight)> existingRects, (int x, int y, int width, int height, float cubeHeight) newRect, HashSet<(int x, int y)> componentSet)
        {
            var allRects = new List<(int x, int y, int width, int height, float cubeHeight)>(existingRects) { newRect };
            
            // Check if the combined bounding box would be fully covered
            int minX = allRects.Min(r => r.x);
            int maxX = allRects.Max(r => r.x + r.width);
            int minY = allRects.Min(r => r.y);
            int maxY = allRects.Max(r => r.y + r.height);
            
            int boundingArea = (maxX - minX) * (maxY - minY);
            int actualArea = allRects.Sum(r => r.width * r.height);
            
            if (boundingArea != actualArea) return false;
            
            // Verify all positions are in the component
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (!componentSet.Contains((x, y)))
                        return false;
                }
            }
            
            return true;
        }

        private (int x, int y, int width, int height, float cubeHeight)? TryMergeMultipleRectangles(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, HashSet<(int x, int y)> componentSet)
        {
            if (rectangles.Count < 2) return null;
            
            int minX = rectangles.Min(r => r.x);
            int maxX = rectangles.Max(r => r.x + r.width);
            int minY = rectangles.Min(r => r.y);
            int maxY = rectangles.Max(r => r.y + r.height);
            
            int combinedWidth = maxX - minX;
            int combinedHeight = maxY - minY;
            int requiredArea = combinedWidth * combinedHeight;
            int actualArea = rectangles.Sum(r => r.width * r.height);
            
            if (actualArea != requiredArea) return null;
            
            // Verify all positions in the bounding box are in the component
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (!componentSet.Contains((x, y)))
                        return null;
                }
            }
            
            return (minX, minY, combinedWidth, combinedHeight, rectangles[0].cubeHeight);
        }

        private (int x, int y, int width, int height, float cubeHeight)? TryComplexMerge((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2, HashSet<(int x, int y)> componentSet)
        {
            // Check if the two rectangles can form a valid merged rectangle by checking if all intermediate positions are in the component
            int minX = Math.Min(rect1.x, rect2.x);
            int maxX = Math.Max(rect1.x + rect1.width, rect2.x + rect2.width);
            int minY = Math.Min(rect1.y, rect2.y);
            int maxY = Math.Max(rect1.y + rect1.height, rect2.y + rect2.height);
            
            int combinedWidth = maxX - minX;
            int combinedHeight = maxY - minY;
            int requiredArea = combinedWidth * combinedHeight;
            
            // Check if all positions in the combined rectangle are in the component
            int actualArea = 0;
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (componentSet.Contains((x, y)))
                    {
                        actualArea++;
                    }
                }
            }
            
            // If all positions are covered, we can merge
            if (actualArea == requiredArea)
            {
                return (minX, minY, combinedWidth, combinedHeight, rect1.cubeHeight);
            }
            
            return null;
        }

        private void GenerateOptimizedRegions(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, int width, int height)
        {
            // Legacy method - now redirects to new rectangle-first approach
            GenerateOptimizedRectangles(vertices, triangles, materialMap, heightMap, width, height);
        }

        private void FindOptimalSamePlaneRegions(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            Console.WriteLine($"  Applying enhanced same-plane optimization for material {targetMaterial}...");
            
            // Use flood-fill approach to find connected components
            var components = FindConnectedComponents(materialMap, heightMap, width, height, targetMaterial);
            
            int totalRectangles = 0;
            
            foreach (var component in components)
            {
                // Process each connected component separately for optimal results
                var componentRectangles = OptimizeConnectedComponent(component, materialMap, heightMap, processed, targetMaterial);
                totalRectangles += componentRectangles.Count;
                
                // Create geometry for all rectangles in this component
                foreach (var rect in componentRectangles)
                {
                    CreateRectangleGeometryShared(vertices, triangles, rect.x, rect.y, rect.width, rect.height, targetMaterial, rect.cubeHeight, vertexCache);
                }
            }
            
            Console.WriteLine($"  Material {targetMaterial}: optimized into {totalRectangles} rectangles across {components.Count} connected components");
        }

        private List<List<(int x, int y)>> FindConnectedComponents(string[,] materialMap, float[,] heightMap, int width, int height, string targetMaterial)
        {
            var visited = new bool[width, height];
            var components = new List<List<(int x, int y)>>();
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!visited[x, y] && materialMap[x, y] == targetMaterial && !string.IsNullOrEmpty(materialMap[x, y]))
                    {
                        var component = new List<(int x, int y)>();
                        var targetHeight = heightMap[x, y];
                        FloodFill(materialMap, heightMap, visited, x, y, width, height, targetMaterial, targetHeight, component);
                        
                        if (component.Count > 0)
                        {
                            components.Add(component);
                        }
                    }
                }
            }
            
            return components;
        }
        
        private void FloodFill(string[,] materialMap, float[,] heightMap, bool[,] visited, int startX, int startY, int width, int height, string targetMaterial, float targetHeight, List<(int x, int y)> component)
        {
            var stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));
            
            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();
                
                if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y] ||
                    materialMap[x, y] != targetMaterial || heightMap[x, y] != targetHeight)
                {
                    continue;
                }
                
                visited[x, y] = true;
                component.Add((x, y));
                
                // Add all 4 adjacent cells
                stack.Push((x + 1, y));
                stack.Push((x - 1, y));
                stack.Push((x, y + 1));
                stack.Push((x, y - 1));
            }
        }
        
        private List<(int x, int y, int width, int height, float cubeHeight)> OptimizeConnectedComponent(List<(int x, int y)> component, string[,] materialMap, float[,] heightMap, bool[,] processed, string targetMaterial)
        {
            var rectangles = new List<(int x, int y, int width, int height, float cubeHeight)>();
            var componentProcessed = new HashSet<(int x, int y)>();
            
            // Sort component by x, then y for consistent processing
            component.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            
            foreach (var (x, y) in component)
            {
                if (componentProcessed.Contains((x, y)))
                    continue;
                    
                // Find the largest rectangle starting from this position within the component
                var rect = FindLargestRectangleInComponent(component, componentProcessed, x, y, heightMap[x, y]);
                
                if (rect.width > 0 && rect.height > 0)
                {
                    rectangles.Add((rect.x, rect.y, rect.width, rect.height, heightMap[x, y]));
                    
                    // Mark all cells in this rectangle as processed
                    for (int ry = rect.y; ry < rect.y + rect.height; ry++)
                    {
                        for (int rx = rect.x; rx < rect.x + rect.width; rx++)
                        {
                            componentProcessed.Add((rx, ry));
                            processed[rx, ry] = true;
                        }
                    }
                }
            }
            
            // Apply ultra-aggressive merging with multiple strategies
            int initialCount = rectangles.Count;
            bool merged = true;
            int iterations = 0;
            const int maxIterations = 30; // Even more aggressive iterations
            
            while (merged && iterations < maxIterations)
            {
                merged = false;
                iterations++;
                int beforeMergeCount = rectangles.Count;
                
                // Strategy 1: Try all possible simple adjacency merges
                for (int i = 0; i < rectangles.Count && !merged; i++)
                {
                    for (int j = i + 1; j < rectangles.Count && !merged; j++)
                    {
                        var rect1 = rectangles[i];
                        var rect2 = rectangles[j];
                        
                        if (rect1.cubeHeight != rect2.cubeHeight) continue;
                        
                        var mergedRect = TryMergeRectangles(rect1, rect2);
                        if (mergedRect.HasValue)
                        {
                            rectangles[i] = mergedRect.Value;
                            rectangles.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                }
                
                // Strategy 2: Try complex merge patterns if simple merging didn't work
                if (!merged)
                {
                    for (int i = 0; i < rectangles.Count && !merged; i++)
                    {
                        for (int j = i + 1; j < rectangles.Count && !merged; j++)
                        {
                            var rect1 = rectangles[i];
                            var rect2 = rectangles[j];
                            
                            if (rect1.cubeHeight != rect2.cubeHeight) continue;
                            
                            var complexMerge = TryComplexMerge(rect1, rect2, component);
                            if (complexMerge.HasValue)
                            {
                                rectangles[i] = complexMerge.Value;
                                rectangles.RemoveAt(j);
                                merged = true;
                                break;
                            }
                        }
                    }
                }
                
                // Strategy 3: Try to merge multiple rectangles that can form a larger shape
                if (!merged && iterations < 15) // Only try this in earlier iterations to avoid infinite loops
                {
                    merged = TryMultiRectangleMerge(rectangles, component);
                }
                
                int afterMergeCount = rectangles.Count;
                if (beforeMergeCount > afterMergeCount)
                {
                    Console.WriteLine($"    Component iteration {iterations}: Merged {beforeMergeCount - afterMergeCount} rectangles");
                }
            }
            
            return rectangles;
        }
        
        private bool TryMultiRectangleMerge(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, List<(int x, int y)> component)
        {
            // Try to find 3 or more rectangles that can be merged into a larger shape
            for (int i = 0; i < rectangles.Count; i++)
            {
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    for (int k = j + 1; k < rectangles.Count; k++)
                    {
                        var rect1 = rectangles[i];
                        var rect2 = rectangles[j];
                        var rect3 = rectangles[k];
                        
                        if (rect1.cubeHeight != rect2.cubeHeight || rect1.cubeHeight != rect3.cubeHeight) continue;
                        
                        // Check if these three rectangles can form a larger rectangle
                        var mergedRect = TryMergeThreeRectangles(rect1, rect2, rect3, component);
                        if (mergedRect.HasValue)
                        {
                            // Replace the three rectangles with the merged one
                            rectangles[i] = mergedRect.Value;
                            rectangles.RemoveAt(Math.Max(j, k)); // Remove larger index first
                            rectangles.RemoveAt(Math.Min(j, k));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private (int x, int y, int width, int height, float cubeHeight)? TryMergeThreeRectangles(
            (int x, int y, int width, int height, float cubeHeight) rect1,
            (int x, int y, int width, int height, float cubeHeight) rect2,
            (int x, int y, int width, int height, float cubeHeight) rect3,
            List<(int x, int y)> component)
        {
            // Find the bounding box of all three rectangles
            int minX = Math.Min(Math.Min(rect1.x, rect2.x), rect3.x);
            int maxX = Math.Max(Math.Max(rect1.x + rect1.width, rect2.x + rect2.width), rect3.x + rect3.width);
            int minY = Math.Min(Math.Min(rect1.y, rect2.y), rect3.y);
            int maxY = Math.Max(Math.Max(rect1.y + rect1.height, rect2.y + rect2.height), rect3.y + rect3.height);
            
            int combinedWidth = maxX - minX;
            int combinedHeight = maxY - minY;
            int requiredArea = combinedWidth * combinedHeight;
            
            // Check if all three rectangles together fill the bounding box exactly
            int rect1Area = rect1.width * rect1.height;
            int rect2Area = rect2.width * rect2.height;
            int rect3Area = rect3.width * rect3.height;
            int totalArea = rect1Area + rect2Area + rect3Area;
            
            if (totalArea != requiredArea) return null; // Not a perfect fit
            
            // Verify that all positions in the bounding box are in the component
            var componentSet = new HashSet<(int x, int y)>(component);
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (!componentSet.Contains((x, y)))
                    {
                        return null; // Gap found
                    }
                }
            }
            
            return (minX, minY, combinedWidth, combinedHeight, rect1.cubeHeight);
        }
        
        private (int x, int y, int width, int height) FindLargestRectangleInComponent(List<(int x, int y)> component, HashSet<(int x, int y)> processed, int startX, int startY, float targetHeight)
        {
            // Create a set for fast lookup
            var componentSet = new HashSet<(int x, int y)>(component);
            
            // Find maximum width for this row
            int maxWidth = 0;
            for (int x = startX; ; x++)
            {
                if (processed.Contains((x, startY)) || !componentSet.Contains((x, startY)))
                    break;
                maxWidth++;
            }
            
            if (maxWidth == 0) return (0, 0, 0, 0);
            
            // Find maximum height maintaining the same width
            int maxHeight = 1;
            for (int y = startY + 1; ; y++)
            {
                bool canExtend = true;
                for (int x = startX; x < startX + maxWidth; x++)
                {
                    if (processed.Contains((x, y)) || !componentSet.Contains((x, y)))
                    {
                        canExtend = false;
                        break;
                    }
                }
                
                if (!canExtend) break;
                maxHeight++;
            }
            
            return (startX, startY, maxWidth, maxHeight);
        }
        
        private (int x, int y, int width, int height, float cubeHeight)? TryComplexMerge((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2, List<(int x, int y)> component)
        {
            // Check if the two rectangles can form a valid merged rectangle by checking if all intermediate positions are in the component
            int minX = Math.Min(rect1.x, rect2.x);
            int maxX = Math.Max(rect1.x + rect1.width, rect2.x + rect2.width);
            int minY = Math.Min(rect1.y, rect2.y);
            int maxY = Math.Max(rect1.y + rect1.height, rect2.y + rect2.height);
            
            int combinedWidth = maxX - minX;
            int combinedHeight = maxY - minY;
            int requiredArea = combinedWidth * combinedHeight;
            
            // Create a set for fast lookup
            var componentSet = new HashSet<(int x, int y)>(component);
            
            // Check if all positions in the combined rectangle are in the component
            int actualArea = 0;
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (componentSet.Contains((x, y)))
                    {
                        actualArea++;
                    }
                }
            }
            
            // If all positions are covered, we can merge
            if (actualArea == requiredArea)
            {
                return (minX, minY, combinedWidth, combinedHeight, rect1.cubeHeight);
            }
            
            return null;
        }

        // Legacy methods (kept for reference but not used)
        private void FindOptimalPerimeterRectangles(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            var rectangles = new List<(int x, int y, int width, int height, float cubeHeight)>();
            
            Console.WriteLine($"  Applying perimeter optimization for material {targetMaterial}...");
            
            // Phase 1: Handle outer perimeter walls first (they often form long strips)
            ProcessPerimeterWalls(rectangles, materialMap, heightMap, processed, width, height, targetMaterial);
            
            // Phase 2: Handle remaining interior walls with regular algorithm
            FindOptimalRectanglesBase(rectangles, materialMap, heightMap, processed, width, height, targetMaterial);
            
            int initialCount = rectangles.Count;
            
            // Phase 3: Apply aggressive merging for wall segments
            bool merged = true;
            int iterations = 0;
            const int maxIterations = 15; // More iterations for walls
            
            while (merged && iterations < maxIterations)
            {
                merged = false;
                iterations++;
                int beforeMergeCount = rectangles.Count;
                
                // Try merging all possible combinations
                for (int i = 0; i < rectangles.Count; i++)
                {
                    for (int j = i + 1; j < rectangles.Count; j++)
                    {
                        var rect1 = rectangles[i];
                        var rect2 = rectangles[j];
                        
                        if (rect1.cubeHeight != rect2.cubeHeight) continue;
                        
                        var mergedRect = TryMergeRectangles(rect1, rect2);
                        if (mergedRect.HasValue)
                        {
                            rectangles[i] = mergedRect.Value;
                            rectangles.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                    if (merged) break;
                }
                
                int afterMergeCount = rectangles.Count;
                if (beforeMergeCount > afterMergeCount)
                {
                    Console.WriteLine($"    Iteration {iterations}: Merged {beforeMergeCount - afterMergeCount} wall rectangles");
                }
            }
            
            Console.WriteLine($"  Wall material {targetMaterial}: {initialCount} -> {rectangles.Count} rectangles after {iterations} iterations");
            
            // Phase 4: Create optimized geometry
            foreach (var rect in rectangles)
            {
                CreateRectangleGeometryShared(vertices, triangles, rect.x, rect.y, rect.width, rect.height, targetMaterial, rect.cubeHeight, vertexCache);
            }
        }

        private void ProcessPerimeterWalls(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial)
        {
            // Process top edge
            ProcessPerimeterEdge(rectangles, materialMap, heightMap, processed, 0, 0, width, 1, targetMaterial, true);
            
            // Process bottom edge
            ProcessPerimeterEdge(rectangles, materialMap, heightMap, processed, 0, height - 1, width, 1, targetMaterial, true);
            
            // Process left edge (excluding corners already processed)
            ProcessPerimeterEdge(rectangles, materialMap, heightMap, processed, 0, 1, 1, height - 2, targetMaterial, false);
            
            // Process right edge (excluding corners already processed)
            ProcessPerimeterEdge(rectangles, materialMap, heightMap, processed, width - 1, 1, 1, height - 2, targetMaterial, false);
        }

        private void ProcessPerimeterEdge(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int startX, int startY, int maxWidth, int maxHeight, string targetMaterial, bool horizontal)
        {
            if (horizontal)
            {
                // Process horizontal edge - look for long horizontal strips
                for (int y = startY; y < startY + maxHeight; y++)
                {
                    int x = startX;
                    while (x < startX + maxWidth)
                    {
                        if (!processed[x, y] && materialMap[x, y] == targetMaterial && !string.IsNullOrEmpty(materialMap[x, y]))
                        {
                            // Find the longest horizontal strip
                            int stripWidth = 0;
                            float stripHeight = heightMap[x, y];
                            
                            for (int checkX = x; checkX < startX + maxWidth; checkX++)
                            {
                                if (processed[checkX, y] || materialMap[checkX, y] != targetMaterial || heightMap[checkX, y] != stripHeight)
                                    break;
                                stripWidth++;
                            }
                            
                            if (stripWidth > 0)
                            {
                                rectangles.Add((x, y, stripWidth, 1, stripHeight));
                                
                                // Mark as processed
                                for (int markX = x; markX < x + stripWidth; markX++)
                                {
                                    processed[markX, y] = true;
                                }
                                
                                x += stripWidth;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        else
                        {
                            x++;
                        }
                    }
                }
            }
            else
            {
                // Process vertical edge - look for long vertical strips
                for (int x = startX; x < startX + maxWidth; x++)
                {
                    int y = startY;
                    while (y < startY + maxHeight)
                    {
                        if (!processed[x, y] && materialMap[x, y] == targetMaterial && !string.IsNullOrEmpty(materialMap[x, y]))
                        {
                            // Find the longest vertical strip
                            int stripHeight = 0;
                            float stripHeightValue = heightMap[x, y];
                            
                            for (int checkY = y; checkY < startY + maxHeight; checkY++)
                            {
                                if (processed[x, checkY] || materialMap[x, checkY] != targetMaterial || heightMap[x, checkY] != stripHeightValue)
                                    break;
                                stripHeight++;
                            }
                            
                            if (stripHeight > 0)
                            {
                                rectangles.Add((x, y, 1, stripHeight, stripHeightValue));
                                
                                // Mark as processed
                                for (int markY = y; markY < y + stripHeight; markY++)
                                {
                                    processed[x, markY] = true;
                                }
                                
                                y += stripHeight;
                            }
                            else
                            {
                                y++;
                            }
                        }
                        else
                        {
                            y++;
                        }
                    }
                }
            }
        }

        private void FindOptimalRectanglesIterative(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            var rectangles = new List<(int x, int y, int width, int height, float cubeHeight)>();
            
            // Phase 1: Find initial rectangles using the existing algorithm
            FindOptimalRectanglesBase(rectangles, materialMap, heightMap, processed, width, height, targetMaterial);
            int initialCount = rectangles.Count;
            
            // Phase 2: Iteratively merge adjacent rectangles to catch more optimization opportunities
            bool merged = true;
            int iterations = 0;
            const int maxIterations = 10; // Increase iterations to allow more merging
            
            while (merged && iterations < maxIterations)
            {
                merged = false;
                iterations++;
                int beforeMergeCount = rectangles.Count;
                
                for (int i = 0; i < rectangles.Count; i++)
                {
                    for (int j = i + 1; j < rectangles.Count; j++)
                    {
                        var rect1 = rectangles[i];
                        var rect2 = rectangles[j];
                        
                        // Only merge rectangles with same height
                        if (rect1.cubeHeight != rect2.cubeHeight) continue;
                        
                        var mergedRect = TryMergeRectangles(rect1, rect2);
                        if (mergedRect.HasValue)
                        {
                            // Replace the two rectangles with the merged one
                            rectangles[i] = mergedRect.Value;
                            rectangles.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                    if (merged) break;
                }
                
                int afterMergeCount = rectangles.Count;
                if (beforeMergeCount > afterMergeCount)
                {
                    Console.WriteLine($"  Iteration {iterations}: Merged {beforeMergeCount - afterMergeCount} rectangles for material {targetMaterial}");
                }
            }
            
            Console.WriteLine($"  Material {targetMaterial}: {initialCount} -> {rectangles.Count} rectangles after {iterations} iterations");
            
            // Phase 3: Create optimized geometry for all rectangles
            foreach (var rect in rectangles)
            {
                CreateRectangleGeometryShared(vertices, triangles, rect.x, rect.y, rect.width, rect.height, targetMaterial, rect.cubeHeight, vertexCache);
            }
        }

        private void FindOptimalRectanglesBase(List<(int x, int y, int width, int height, float cubeHeight)> rectangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!processed[x, y] && materialMap[x, y] == targetMaterial && !string.IsNullOrEmpty(materialMap[x, y]))
                    {
                        // Find the largest rectangle starting from this position
                        var rect = FindLargestRectangle(materialMap, heightMap, processed, x, y, width, height, targetMaterial);
                        
                        if (rect.width > 0 && rect.height > 0)
                        {
                            // Add rectangle to list instead of creating geometry immediately
                            rectangles.Add((rect.x, rect.y, rect.width, rect.height, heightMap[x, y]));
                            
                            // Mark all cells in this rectangle as processed
                            MarkRectangleAsProcessed(processed, rect.x, rect.y, rect.width, rect.height);
                        }
                    }
                }
            }
        }

        private (int x, int y, int width, int height, float cubeHeight)? TryMergeRectangles((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            // Check if rectangles are adjacent and can be merged
            
            // Horizontal adjacency (same Y and height, adjacent X)
            if (rect1.y == rect2.y && rect1.height == rect2.height)
            {
                if (rect1.x + rect1.width == rect2.x)
                {
                    // rect1 is to the left of rect2
                    return (rect1.x, rect1.y, rect1.width + rect2.width, rect1.height, rect1.cubeHeight);
                }
                else if (rect2.x + rect2.width == rect1.x)
                {
                    // rect2 is to the left of rect1
                    return (rect2.x, rect2.y, rect2.width + rect1.width, rect2.height, rect2.cubeHeight);
                }
            }
            
            // Vertical adjacency (same X and width, adjacent Y)
            if (rect1.x == rect2.x && rect1.width == rect2.width)
            {
                if (rect1.y + rect1.height == rect2.y)
                {
                    // rect1 is above rect2
                    return (rect1.x, rect1.y, rect1.width, rect1.height + rect2.height, rect1.cubeHeight);
                }
                else if (rect2.y + rect2.height == rect1.y)
                {
                    // rect2 is above rect1
                    return (rect2.x, rect2.y, rect2.width, rect2.height + rect1.height, rect2.cubeHeight);
                }
            }
            
            // Try diagonal merging for L-shapes and complex patterns
            // This is an experimental approach to catch patterns that pure rectangle merging misses
            if (CanFormLargerRectangle(rect1, rect2))
            {
                return FormLargerRectangle(rect1, rect2);
            }
            
            return null; // Cannot merge
        }
        
        private bool CanFormLargerRectangle((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            // Check if two rectangles can form a larger rectangle when combined
            // This handles cases where rectangles are not directly adjacent but can form a larger shape
            
            int minX = Math.Min(rect1.x, rect2.x);
            int maxX = Math.Max(rect1.x + rect1.width, rect2.x + rect2.width);
            int minY = Math.Min(rect1.y, rect2.y);
            int maxY = Math.Max(rect1.y + rect1.height, rect2.y + rect2.height);
            
            int combinedWidth = maxX - minX;
            int combinedHeight = maxY - minY;
            int combinedArea = combinedWidth * combinedHeight;
            int actualArea = rect1.width * rect1.height + rect2.width * rect2.height;
            
            // If the combined area equals the sum of individual areas, they form a perfect rectangle
            return combinedArea == actualArea;
        }
        
        private (int x, int y, int width, int height, float cubeHeight) FormLargerRectangle((int x, int y, int width, int height, float cubeHeight) rect1, (int x, int y, int width, int height, float cubeHeight) rect2)
        {
            int minX = Math.Min(rect1.x, rect2.x);
            int maxX = Math.Max(rect1.x + rect1.width, rect2.x + rect2.width);
            int minY = Math.Min(rect1.y, rect2.y);
            int maxY = Math.Max(rect1.y + rect1.height, rect2.y + rect2.height);
            
            return (minX, minY, maxX - minX, maxY - minY, rect1.cubeHeight);
        }

        private void FindOptimalRectangles(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, string[,] materialMap, float[,] heightMap, bool[,] processed, int width, int height, string targetMaterial, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!processed[x, y] && materialMap[x, y] == targetMaterial && !string.IsNullOrEmpty(materialMap[x, y]))
                    {
                        // Find the largest rectangle starting from this position
                        var rect = FindLargestRectangle(materialMap, heightMap, processed, x, y, width, height, targetMaterial);
                        
                        if (rect.width > 0 && rect.height > 0)
                        {
                            // Create optimized geometry for this rectangle with vertex sharing
                            CreateRectangleGeometryShared(vertices, triangles, rect.x, rect.y, rect.width, rect.height, targetMaterial, heightMap[x, y], vertexCache);
                            
                            // Mark all cells in this rectangle as processed
                            MarkRectangleAsProcessed(processed, rect.x, rect.y, rect.width, rect.height);
                        }
                    }
                }
            }
        }

        private (int x, int y, int width, int height) FindLargestRectangle(string[,] materialMap, float[,] heightMap, bool[,] processed, int startX, int startY, int totalWidth, int totalHeight, string targetMaterial)
        {
            var targetHeight = heightMap[startX, startY];
            
            // Find maximum width for this row
            int maxWidth = 0;
            for (int x = startX; x < totalWidth; x++)
            {
                if (processed[x, startY] || materialMap[x, startY] != targetMaterial || heightMap[x, startY] != targetHeight)
                    break;
                maxWidth++;
            }
            
            if (maxWidth == 0) return (0, 0, 0, 0);
            
            // Find maximum height maintaining the same width
            int maxHeight = 1;
            for (int y = startY + 1; y < totalHeight; y++)
            {
                bool canExtend = true;
                for (int x = startX; x < startX + maxWidth; x++)
                {
                    if (processed[x, y] || materialMap[x, y] != targetMaterial || heightMap[x, y] != targetHeight)
                    {
                        canExtend = false;
                        break;
                    }
                }
                
                if (!canExtend) break;
                maxHeight++;
            }
            
            return (startX, startY, maxWidth, maxHeight);
        }

        private void MarkRectangleAsProcessed(bool[,] processed, int x, int y, int width, int height)
        {
            for (int ry = y; ry < y + height; ry++)
            {
                for (int rx = x; rx < x + width; rx++)
                {
                    processed[rx, ry] = true;
                }
            }
        }

        private void CreateRectangleGeometry(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, int rectX, int rectY, int rectWidth, int rectHeight, string material, float cubeHeight)
        {
            int baseIndex = vertices.Count;
            float zBottom = GroundHeight;
            float zTop = GroundHeight + cubeHeight;
            
            // Create vertices for the rectangular region (more efficient than individual cubes)
            // Bottom vertices (usually not needed since ground plane covers this)
            vertices.Add((rectX, rectY, zBottom));
            vertices.Add((rectX + rectWidth, rectY, zBottom));
            vertices.Add((rectX + rectWidth, rectY + rectHeight, zBottom));
            vertices.Add((rectX, rectY + rectHeight, zBottom));
            
            // Top vertices
            vertices.Add((rectX, rectY, zTop));
            vertices.Add((rectX + rectWidth, rectY, zTop));
            vertices.Add((rectX + rectWidth, rectY + rectHeight, zTop));
            vertices.Add((rectX, rectY + rectHeight, zTop));

            // Top face (the most important face for visual appearance)
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, material));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, material));

            // Side faces (for walls/paths height)
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, material)); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, material));

            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, material)); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, material));

            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, material)); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, material));

            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, material)); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, material));
        }

        private void CreateRectangleGeometryShared(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, int rectX, int rectY, int rectWidth, int rectHeight, string material, float cubeHeight, Dictionary<(float x, float y, float z), int> vertexCache)
        {
            float zBottom = GroundHeight;
            float zTop = GroundHeight + cubeHeight;
            
            // Helper function to get or create vertex index
            int GetOrCreateVertex(float x, float y, float z)
            {
                var vertexKey = (x, y, z);
                if (vertexCache.TryGetValue(vertexKey, out int existingIndex))
                {
                    return existingIndex;
                }
                
                int newIndex = vertices.Count;
                vertices.Add((x, y, z));
                vertexCache[vertexKey] = newIndex;
                return newIndex;
            }
            
            // Create vertices for the rectangular region with sharing
            var v0 = GetOrCreateVertex(rectX, rectY, zBottom);
            var v1 = GetOrCreateVertex(rectX + rectWidth, rectY, zBottom);
            var v2 = GetOrCreateVertex(rectX + rectWidth, rectY + rectHeight, zBottom);
            var v3 = GetOrCreateVertex(rectX, rectY + rectHeight, zBottom);
            
            var v4 = GetOrCreateVertex(rectX, rectY, zTop);
            var v5 = GetOrCreateVertex(rectX + rectWidth, rectY, zTop);
            var v6 = GetOrCreateVertex(rectX + rectWidth, rectY + rectHeight, zTop);
            var v7 = GetOrCreateVertex(rectX, rectY + rectHeight, zTop);

            // Top face (the most important face for visual appearance)
            triangles.Add((v4, v5, v6, material));
            triangles.Add((v4, v6, v7, material));

            // Side faces (for walls/paths height)
            triangles.Add((v0, v1, v5, material)); // Front
            triangles.Add((v0, v5, v4, material));

            triangles.Add((v1, v2, v6, material)); // Right
            triangles.Add((v1, v6, v5, material));

            triangles.Add((v2, v3, v7, material)); // Back
            triangles.Add((v2, v7, v6, material));

            triangles.Add((v3, v0, v4, material)); // Left
            triangles.Add((v3, v4, v7, material));
        }

        private enum VoxelType
        {
            Wall,
            Path
        }

        private class VoxelData
        {
            public string Material { get; set; } = "";
            public VoxelType Type { get; set; }
        }

        private class Face
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
            public int Direction { get; set; } // 0=+X, 1=-X, 2=+Y, 3=-Y, 4=+Z, 5=-Z
            public string Material { get; set; } = "";
            public VoxelType VoxelType { get; set; }
        }

        private void GenerateOptimizedFaces(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, Dictionary<(int x, int y, int z), VoxelData> voxels, InnerMap maze)
        {
            var faces = new List<Face>();

            // Generate faces for each voxel
            foreach (var kvp in voxels)
            {
                var (x, y, z) = kvp.Key;
                var voxelData = kvp.Value;
                
                // Check each direction for exposed faces
                var directions = new[]
                {
                    (1, 0, 0, 0),   // +X
                    (-1, 0, 0, 1),  // -X
                    (0, 1, 0, 2),   // +Y
                    (0, -1, 0, 3),  // -Y
                    (0, 0, 1, 4),   // +Z
                    (0, 0, -1, 5)   // -Z
                };

                foreach (var (dx, dy, dz, dir) in directions)
                {
                    var neighborPos = (x + dx, y + dy, z + dz);
                    
                    // Check if face should be generated
                    bool shouldGenerateFace = false;
                    
                    if (!voxels.ContainsKey(neighborPos))
                    {
                        // No neighbor voxel - this is a boundary face
                        shouldGenerateFace = true;
                    }
                    else
                    {
                        // Neighbor voxel exists - generate face if different material or type
                        var neighborVoxel = voxels[neighborPos];
                        shouldGenerateFace = neighborVoxel.Material != voxelData.Material || 
                                           neighborVoxel.Type != voxelData.Type;
                    }
                    
                    // Special handling for bottom faces (don't generate if there's ground below)
                    if (dir == 5 && z == 1) // -Z direction at z=1 (bottom face touching ground)
                    {
                        shouldGenerateFace = false; // Ground plane already handles this
                    }

                    if (shouldGenerateFace)
                    {
                        var face = new Face
                        {
                            Direction = dir,
                            Material = voxelData.Material,
                            VoxelType = voxelData.Type,
                            Width = 1.0f,
                            Height = 1.0f
                        };

                        // Calculate face position based on direction
                        SetFacePosition(face, x, y, z, voxelData.Type);
                        faces.Add(face);
                    }
                }
            }

            // Apply greedy meshing to merge adjacent faces
            var optimizedFaces = GreedyMeshFaces(faces);

            // Generate triangles from optimized faces
            foreach (var face in optimizedFaces)
            {
                AddFaceTriangles(vertices, triangles, face);
            }
        }

        private void SetFacePosition(Face face, int x, int y, int z, VoxelType voxelType)
        {
            float zBottom = GroundHeight;
            float zTop = GroundHeight + (voxelType == VoxelType.Wall ? WallHeight : PathHeight);

            switch (face.Direction)
            {
                case 0: // +X
                    face.X = x + 1;
                    face.Y = y + 0.5f;
                    face.Z = zBottom + (zTop - zBottom) / 2;
                    face.Width = zTop - zBottom;
                    face.Height = 1.0f;
                    break;
                case 1: // -X
                    face.X = x;
                    face.Y = y + 0.5f;
                    face.Z = zBottom + (zTop - zBottom) / 2;
                    face.Width = zTop - zBottom;
                    face.Height = 1.0f;
                    break;
                case 2: // +Y
                    face.X = x + 0.5f;
                    face.Y = y + 1;
                    face.Z = zBottom + (zTop - zBottom) / 2;
                    face.Width = 1.0f;
                    face.Height = zTop - zBottom;
                    break;
                case 3: // -Y
                    face.X = x + 0.5f;
                    face.Y = y;
                    face.Z = zBottom + (zTop - zBottom) / 2;
                    face.Width = 1.0f;
                    face.Height = zTop - zBottom;
                    break;
                case 4: // +Z
                    face.X = x + 0.5f;
                    face.Y = y + 0.5f;
                    face.Z = zTop;
                    face.Width = 1.0f;
                    face.Height = 1.0f;
                    break;
                case 5: // -Z (should not be called for z=1, but handle anyway)
                    face.X = x + 0.5f;
                    face.Y = y + 0.5f;
                    face.Z = zBottom;
                    face.Width = 1.0f;
                    face.Height = 1.0f;
                    break;
            }
        }

        private List<Face> GreedyMeshFaces(List<Face> faces)
        {
            var optimizedFaces = new List<Face>();
            var processed = new HashSet<Face>();

            foreach (var face in faces)
            {
                if (processed.Contains(face))
                    continue;

                var mergedFace = new Face
                {
                    X = face.X,
                    Y = face.Y,
                    Z = face.Z,
                    Width = face.Width,
                    Height = face.Height,
                    Direction = face.Direction,
                    Material = face.Material,
                    VoxelType = face.VoxelType
                };

                processed.Add(face);

                // Try to merge with other faces
                bool merged = true;
                while (merged)
                {
                    merged = false;
                    
                    foreach (var otherFace in faces)
                    {
                        if (processed.Contains(otherFace) || otherFace.Direction != mergedFace.Direction || 
                            otherFace.Material != mergedFace.Material || otherFace.VoxelType != mergedFace.VoxelType)
                            continue;

                        // Try to merge faces based on direction
                        if (TryMergeFaces(mergedFace, otherFace))
                        {
                            processed.Add(otherFace);
                            merged = true;
                        }
                    }
                }

                optimizedFaces.Add(mergedFace);
            }

            return optimizedFaces;
        }

        private bool TryMergeFaces(Face face1, Face face2)
        {
            const float tolerance = 0.001f;

            switch (face1.Direction)
            {
                case 0: case 1: // X faces
                    // Check if they're aligned and adjacent in Y direction
                    if (Math.Abs(face1.X - face2.X) < tolerance && Math.Abs(face1.Z - face2.Z) < tolerance &&
                        Math.Abs(face1.Width - face2.Width) < tolerance)
                    {
                        if (Math.Abs(face1.Y + face1.Height / 2 - (face2.Y - face2.Height / 2)) < tolerance)
                        {
                            // Merge in Y direction
                            var newY = Math.Min(face1.Y - face1.Height / 2, face2.Y - face2.Height / 2);
                            var newHeight = face1.Height + face2.Height;
                            face1.Y = newY + newHeight / 2;
                            face1.Height = newHeight;
                            return true;
                        }
                    }
                    break;

                case 2: case 3: // Y faces
                    // Check if they're aligned and adjacent in X direction
                    if (Math.Abs(face1.Y - face2.Y) < tolerance && Math.Abs(face1.Z - face2.Z) < tolerance &&
                        Math.Abs(face1.Height - face2.Height) < tolerance)
                    {
                        if (Math.Abs(face1.X + face1.Width / 2 - (face2.X - face2.Width / 2)) < tolerance)
                        {
                            // Merge in X direction
                            var newX = Math.Min(face1.X - face1.Width / 2, face2.X - face2.Width / 2);
                            var newWidth = face1.Width + face2.Width;
                            face1.X = newX + newWidth / 2;
                            face1.Width = newWidth;
                            return true;
                        }
                    }
                    break;

                case 4: case 5: // Z faces
                    // Check if they're aligned and adjacent in X direction
                    if (Math.Abs(face1.Z - face2.Z) < tolerance && Math.Abs(face1.Y - face2.Y) < tolerance &&
                        Math.Abs(face1.Height - face2.Height) < tolerance)
                    {
                        if (Math.Abs(face1.X + face1.Width / 2 - (face2.X - face2.Width / 2)) < tolerance)
                        {
                            // Merge in X direction
                            var newX = Math.Min(face1.X - face1.Width / 2, face2.X - face2.Width / 2);
                            var newWidth = face1.Width + face2.Width;
                            face1.X = newX + newWidth / 2;
                            face1.Width = newWidth;
                            return true;
                        }
                    }
                    // Check if they're aligned and adjacent in Y direction
                    else if (Math.Abs(face1.Z - face2.Z) < tolerance && Math.Abs(face1.X - face2.X) < tolerance &&
                        Math.Abs(face1.Width - face2.Width) < tolerance)
                    {
                        if (Math.Abs(face1.Y + face1.Height / 2 - (face2.Y - face2.Height / 2)) < tolerance)
                        {
                            // Merge in Y direction
                            var newY = Math.Min(face1.Y - face1.Height / 2, face2.Y - face2.Height / 2);
                            var newHeight = face1.Height + face2.Height;
                            face1.Y = newY + newHeight / 2;
                            face1.Height = newHeight;
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        private void AddFaceTriangles(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, string paintColor)> triangles, Face face)
        {
            int baseIndex = vertices.Count;
            var corners = GetFaceCorners(face);

            // Add vertices
            foreach (var corner in corners)
            {
                vertices.Add(corner);
            }

            // Add triangles (2 triangles per face)
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 2, face.Material));
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 3, face.Material));
        }

        private List<(float x, float y, float z)> GetFaceCorners(Face face)
        {
            var corners = new List<(float x, float y, float z)>();
            float halfWidth = face.Width / 2;
            float halfHeight = face.Height / 2;

            switch (face.Direction)
            {
                case 0: // +X face (facing positive X)
                    corners.Add((face.X, face.Y - halfHeight, face.Z - halfWidth));
                    corners.Add((face.X, face.Y + halfHeight, face.Z - halfWidth));
                    corners.Add((face.X, face.Y + halfHeight, face.Z + halfWidth));
                    corners.Add((face.X, face.Y - halfHeight, face.Z + halfWidth));
                    break;
                case 1: // -X face (facing negative X)
                    corners.Add((face.X, face.Y - halfHeight, face.Z + halfWidth));
                    corners.Add((face.X, face.Y + halfHeight, face.Z + halfWidth));
                    corners.Add((face.X, face.Y + halfHeight, face.Z - halfWidth));
                    corners.Add((face.X, face.Y - halfHeight, face.Z - halfWidth));
                    break;
                case 2: // +Y face (facing positive Y)
                    corners.Add((face.X - halfWidth, face.Y, face.Z - halfHeight));
                    corners.Add((face.X - halfWidth, face.Y, face.Z + halfHeight));
                    corners.Add((face.X + halfWidth, face.Y, face.Z + halfHeight));
                    corners.Add((face.X + halfWidth, face.Y, face.Z - halfHeight));
                    break;
                case 3: // -Y face (facing negative Y)
                    corners.Add((face.X - halfWidth, face.Y, face.Z + halfHeight));
                    corners.Add((face.X - halfWidth, face.Y, face.Z - halfHeight));
                    corners.Add((face.X + halfWidth, face.Y, face.Z - halfHeight));
                    corners.Add((face.X + halfWidth, face.Y, face.Z + halfHeight));
                    break;
                case 4: // +Z face (facing positive Z)
                    corners.Add((face.X - halfWidth, face.Y - halfHeight, face.Z));
                    corners.Add((face.X + halfWidth, face.Y - halfHeight, face.Z));
                    corners.Add((face.X + halfWidth, face.Y + halfHeight, face.Z));
                    corners.Add((face.X - halfWidth, face.Y + halfHeight, face.Z));
                    break;
                case 5: // -Z face (facing negative Z)
                    corners.Add((face.X - halfWidth, face.Y + halfHeight, face.Z));
                    corners.Add((face.X + halfWidth, face.Y + halfHeight, face.Z));
                    corners.Add((face.X + halfWidth, face.Y - halfHeight, face.Z));
                    corners.Add((face.X - halfWidth, face.Y - halfHeight, face.Z));
                    break;
            }

            return corners;
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
            // Ground plane: 2 triangles per face
            int groundFaces = 2;

            // Create voxel representation to calculate exposed faces
            var voxels = new Dictionary<(int x, int y, int z), VoxelData>();
            
            // Add wall voxels
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y))) // Wall position
                    {
                        voxels[(x, y, 1)] = new VoxelData { Material = Colors[0], Type = VoxelType.Wall };
                    }
                }
            }

            // Add path voxels
            foreach (var (x, y) in pathSet)
            {
                if (x < maze.Width - 1 && y < maze.Height - 1 && maze[x, y])
                {
                    var paintColor = Colors[2]; // Simplified - use first path color
                    voxels[(x, y, 1)] = new VoxelData { Material = paintColor, Type = VoxelType.Path };
                }
            }

            // Count exposed faces (approximation - each voxel contributes roughly 4-5 exposed faces on average)
            // This is a simplification since the actual greedy meshing would merge many faces
            int exposedFaces = voxels.Count * 5; // Approximate average exposed faces per voxel
            int triangleCount = exposedFaces * 2; // 2 triangles per face

            return groundFaces + triangleCount;
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