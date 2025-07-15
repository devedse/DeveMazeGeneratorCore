using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.PathFinders;
using DeveMazeGeneratorCore.Structures;
using System.IO.Compression;
using System.Xml;
using System.Text;

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
                CreateModelSettingsFile(archive);
                CreateMetadataFiles(archive);
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
                        <Relationship Type="http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel" Target="/3D/3dmodel.model" Id="rel0"/>
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

        private void CreateMaterials(XmlWriter writer)
        {
            // Base material group
            writer.WriteStartElement("basematerials");
            writer.WriteAttributeString("id", "1");

            // White for ground
            writer.WriteStartElement("base");
            writer.WriteAttributeString("name", "White");
            writer.WriteAttributeString("displaycolor", "#FFFFFF");
            writer.WriteEndElement();

            // Black for walls
            writer.WriteStartElement("base");
            writer.WriteAttributeString("name", "Black");
            writer.WriteAttributeString("displaycolor", "#000000");
            writer.WriteEndElement();

            // Green for path start
            writer.WriteStartElement("base");
            writer.WriteAttributeString("name", "Green");
            writer.WriteAttributeString("displaycolor", "#00FF00");
            writer.WriteEndElement();

            // Red for path end
            writer.WriteStartElement("base");
            writer.WriteAttributeString("name", "Red");
            writer.WriteAttributeString("displaycolor", "#FF0000");
            writer.WriteEndElement();

            writer.WriteEndElement(); // basematerials
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

                // Create materials
                CreateMaterials(writer);

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
            var triangles = new List<(int v1, int v2, int v3, int materialIndex)>();

            // Ground plane vertices and triangles
            AddGroundPlane(vertices, triangles, maze);

            // Wall cubes - exclude the rightmost and bottommost edge (following image generation convention)
            for (int y = 0; y < maze.Height - 1; y++)
            {
                for (int x = 0; x < maze.Width - 1; x++)
                {
                    if (!maze[x, y] && !pathSet.Contains((x, y))) // Wall position (false = wall)
                    {
                        AddCube(vertices, triangles, x, y, GroundHeight, GroundHeight + WallHeight, 1); // Black material (index 1)
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
                    var materialIndex = relativePos < 128 ? 2 : 3; // Green (2) for first half, Red (3) for second half
                    
                    AddCube(vertices, triangles, x, y, GroundHeight, GroundHeight + PathHeight, materialIndex);
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
            foreach (var (v1, v2, v3, materialIndex) in triangles)
            {
                writer.WriteStartElement("triangle");
                writer.WriteAttributeString("v1", v1.ToString());
                writer.WriteAttributeString("v2", v2.ToString());
                writer.WriteAttributeString("v3", v3.ToString());
                writer.WriteAttributeString("pid", "1");
                writer.WriteAttributeString("p1", materialIndex.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // triangles

            writer.WriteEndElement(); // mesh
            writer.WriteEndElement(); // object
        }

        private void AddGroundPlane(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, int materialIndex)> triangles, InnerMap maze)
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

            // Bottom face (z = 0)
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, 0)); // White material
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, 0));

            // Top face (z = GroundHeight)
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, 0));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, 0));

            // Side faces
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, 0)); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, 0));
            
            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, 0)); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, 0));
            
            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, 0)); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, 0));
            
            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, 0)); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, 0));
        }

        private void AddCube(List<(float x, float y, float z)> vertices, List<(int v1, int v2, int v3, int materialIndex)> triangles, int x, int y, float zBottom, float zTop, int materialIndex)
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
            triangles.Add((baseIndex + 0, baseIndex + 2, baseIndex + 1, materialIndex));
            triangles.Add((baseIndex + 0, baseIndex + 3, baseIndex + 2, materialIndex));

            // Top face
            triangles.Add((baseIndex + 4, baseIndex + 5, baseIndex + 6, materialIndex));
            triangles.Add((baseIndex + 4, baseIndex + 6, baseIndex + 7, materialIndex));

            // Side faces
            triangles.Add((baseIndex + 0, baseIndex + 1, baseIndex + 5, materialIndex)); // Front
            triangles.Add((baseIndex + 0, baseIndex + 5, baseIndex + 4, materialIndex));

            triangles.Add((baseIndex + 1, baseIndex + 2, baseIndex + 6, materialIndex)); // Right
            triangles.Add((baseIndex + 1, baseIndex + 6, baseIndex + 5, materialIndex));

            triangles.Add((baseIndex + 2, baseIndex + 3, baseIndex + 7, materialIndex)); // Back
            triangles.Add((baseIndex + 2, baseIndex + 7, baseIndex + 6, materialIndex));

            triangles.Add((baseIndex + 3, baseIndex + 0, baseIndex + 4, materialIndex)); // Left
            triangles.Add((baseIndex + 3, baseIndex + 4, baseIndex + 7, materialIndex));
        }

        private void CreateModelSettingsFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("Metadata/model_settings.config");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <config>
                      <object id="2">
                        <metadata key="name" value="Maze_Coaster"/>
                        <metadata key="extruder" value="1"/>
                        <part id="1" subtype="normal_part">
                          <metadata key="name" value="Maze_Coaster"/>
                          <metadata key="matrix" value="1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"/>
                        </part>
                      </object>
                      <plate>
                        <metadata key="plater_id" value="1"/>
                        <metadata key="plater_name" value=""/>
                        <metadata key="locked" value="false"/>
                        <metadata key="filament_map_mode" value="Auto For Flush"/>
                        <metadata key="filament_maps" value="1 1 1 1"/>
                        <model_instance>
                          <metadata key="object_id" value="2"/>
                          <metadata key="instance_id" value="0"/>
                          <metadata key="identify_id" value="1"/>
                        </model_instance>
                      </plate>
                      <assemble>
                       <assemble_item object_id="2" instance_id="0" transform="1 0 0 0 1 0 0 0 1 0 0 0" offset="0 0 0" />
                      </assemble>
                    </config>
                    """);
            }
        }
    }
}