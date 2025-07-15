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
        private const int MazeSize = 20;
        private const float CoasterSize = 5.0f; // Total height in mm
        private const float GroundHeight = 2.5f; // White ground base height in mm
        private const float WallHeight = 2.5f; // Additional height for walls (black) in mm
        private const float PathHeight = 1.25f; // Additional height for path in mm

        public void Generate3MFCoaster(string filename, int? seed = null)
        {
            Console.WriteLine($"Generating {MazeSize}x{MazeSize} maze...");
            
            // Generate maze using AlgorithmBacktrack2Deluxe2_AsByte
            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();
            var innerMapFactory = new InnerMapFactory<BitArreintjeFastInnerMap>();
            var randomFactory = new RandomFactory<XorShiftRandom>();
            var actionThing = new NoAction();
            
            var usedSeed = seed ?? 1337;
            var maze = alg.GoGenerate(MazeSize, MazeSize, usedSeed, innerMapFactory, randomFactory, actionThing);
            
            Console.WriteLine("Finding path through maze...");
            
            // Find the path with position information
            var path = PathFinderDepthFirstSmartWithPos.GoFind(maze.InnerMap, null);
            
            Console.WriteLine($"Path found with {path.Count} points (seed: {usedSeed})");
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

            // Wall cubes - exclude the rightmost and bottommost edge (following image generation convention)
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y))) // Wall position (false = wall)
                    {
                        AddCube(vertices, triangles, x, y, GroundHeight, GroundHeight + WallHeight, "4"); // Black walls
                    }
                }
            }

            // Path cubes - only within the valid maze area (excluding rightmost and bottommost edge)
            foreach (var (x, y) in pathSet)
            {
                if (x < maze.Width - 1 && y < maze.Height - 1 && maze[x, y]) // Open space that's part of the path and within valid area
                {
                    // Determine color based on position in path (0-255)
                    var relativePos = pathPositions[(x, y)];
                    var paintColor = relativePos < 128 ? "8" : "1C"; // Green (8) for first half, Red (1C) for second half
                    
                    AddCube(vertices, triangles, x, y, GroundHeight, GroundHeight + PathHeight, paintColor);
                }
            }

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
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, "4"));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, "4"));

            // Top face (z = GroundHeight) - white
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, "0C"));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, "0C"));

            // Side faces - black
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, "4")); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, "4"));
            
            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, "4")); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, "4"));
            
            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, "4")); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, "4"));
            
            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, "4")); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, "4"));
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