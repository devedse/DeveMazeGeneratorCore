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
            // Create a 3D representation of voxels with different materials
            // We have three layers: ground (already handled), walls, and paths
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
                    var relativePos = pathPositions[(x, y)];
                    var paintColor = relativePos < 128 ? Colors[2] : Colors[3];
                    voxels[(x, y, 1)] = new VoxelData { Material = paintColor, Type = VoxelType.Path };
                }
            }

            // Generate optimized faces using greedy meshing
            GenerateOptimizedFaces(vertices, triangles, voxels, maze);
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