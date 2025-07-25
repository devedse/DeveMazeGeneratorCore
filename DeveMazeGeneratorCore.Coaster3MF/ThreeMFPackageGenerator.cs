using DeveMazeGeneratorCore.Coaster3MF.Models;
using DeveMazeGeneratorCore.Imageification;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace DeveMazeGeneratorCore.Coaster3MF
{
    public class ThreeMFPackageGenerator
    {
        public void Create3MFFile(string filename, List<ThreeMFPlate> plates, InnerMap maze, List<MazePointPos> path)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                // Create required 3MF files
                CreateContentTypesFile(archive);
                CreateRelsFile(archive);
                Create3DModelFile(archive, plates);
                Create3DModelRelsFile(archive);
                CreateObjectFiles(archive, plates);
                CreateModelSettingsFile(archive, plates);
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
                writer.Write(BambuStudioMetadata.ContentTypes);
            }
        }

        private void CreateRelsFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("_rels/.rels");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.RootRelationships);
            }
        }

        private void Create3DModelRelsFile(ZipArchive archive)
        {
            var entry = archive.CreateEntry("3D/_rels/3dmodel.model.rels");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.ModelRelationships);
            }
        }

        private static string GetResourceObject(ThreeMFModel model)
        {
            return $"""
                  <object id="{model.ObjectId}" p:UUID="{model.ModelId.ToString().PadLeft(8, '0')}-61cb-4c03-9d28-80fed5dfa1dc" type="model">
                   <components>
                    <component p:path="/3D/Objects/object_{model.ModelId}.model" objectid="{model.PartId}" p:UUID="{model.ModelId.ToString().PadLeft(4, '0')}0000-b206-40ff-9872-83e8017abed1" transform="1 0 0 0 1 0 0 0 1 0 0 0"/>
                   </components>
                  </object>
                """;
        }

        private static string GetBuildItem(List<ThreeMFPlate> plates, ThreeMFModel model)
        {
            var foundPlate = plates.First(p => p.Models.Any(m => m.ModelId == model.ModelId));
            var indexOnPlate = foundPlate.Models.FindIndex(m => m.ModelId == model.ModelId);

            return $"""
                  <item objectid="{model.ObjectId}" p:UUID="{model.ObjectId.ToString().PadLeft(8, '0')}-b1ec-4553-aec9-835e5b724bb4" transform="1 0 0 0 1 0 0 0 1 {GetPlatePosition(indexOnPlate)}" printable="1"/>
                """;
        }

        private static string GetPlatePosition(int indexOnPlate)
        {
            return indexOnPlate switch
            {
                0 => "80 60 0",
                1 => "80 160 0",
                2 => "180 60 0",
                3 => "180 160 0",
                _ => throw new ArgumentOutOfRangeException(nameof(indexOnPlate), "Index on plate must be between 0 and 2.")
            };

        }

        private void Create3DModelFile(ZipArchive archive, List<ThreeMFPlate> plates)
        {
            var entry = archive.CreateEntry("3D/3dmodel.model");

            var threedmodeldotmodelTxt = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <model unit="millimeter" xml:lang="en-US" xmlns="http://schemas.microsoft.com/3dmanufacturing/core/2015/02" xmlns:BambuStudio="http://schemas.bambulab.com/package/2021" xmlns:p="http://schemas.microsoft.com/3dmanufacturing/production/2015/06" requiredextensions="p">
                 <metadata name="Application">BambuStudio-02.01.01.52</metadata>
                 <metadata name="BambuStudio:3mfVersion">1</metadata>
                 <metadata name="Copyright"></metadata>
                 <metadata name="CreationDate">2025-07-25</metadata>
                 <metadata name="Description"></metadata>
                 <metadata name="Designer"></metadata>
                 <metadata name="DesignerCover"></metadata>
                 <metadata name="DesignerUserId">2360007279</metadata>
                 <metadata name="License"></metadata>
                 <metadata name="ModificationDate">2025-07-25</metadata>
                 <metadata name="Origin"></metadata>
                 <metadata name="Thumbnail_Middle">/Metadata/plate_1.png</metadata>
                 <metadata name="Thumbnail_Small">/Metadata/plate_1_small.png</metadata>
                 <metadata name="Title"></metadata>
                 <resources>
                  {string.Join(Environment.NewLine, plates.SelectMany(t => t.Models).Select(GetResourceObject))}
                 </resources>
                 <build p:UUID="2c7c17d8-22b5-4d84-8835-1976022ea369">
                  {string.Join(Environment.NewLine, plates.SelectMany(t => t.Models).Select(m => GetBuildItem(plates, m)))}
                 </build>
                </model>
                """;

            using (var stream = entry.Open())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(threedmodeldotmodelTxt);
                }
            }
        }

        private void WriteModelMetadata(XmlWriter writer)
        {
            var metadata = new Dictionary<string, string>
            {
                ["Application"] = "BambuStudio-02.01.01.52",
                ["BambuStudio:3mfVersion"] = "1",
                ["Copyright"] = "",
                ["CreationDate"] = "2025-01-15",
                ["Description"] = "",
                ["Designer"] = "",
                ["DesignerCover"] = "",
                ["DesignerUserId"] = "2360007279",
                ["License"] = "",
                ["ModificationDate"] = "2025-01-15",
                ["Origin"] = "",
                ["Title"] = ""
            };

            foreach (var (name, value) in metadata)
            {
                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", name);
                writer.WriteString(value);
                writer.WriteEndElement();
            }
        }

        private void CreateObjectFiles(ZipArchive archive, List<ThreeMFPlate> plates)
        {
            var allModels = plates.SelectMany(p => p.Models).ToList();
            foreach (var model in allModels)
            {
                CreateObjectFile(archive, model);
            }
        }

        private void CreateObjectFile(ZipArchive archive, ThreeMFModel model)
        {
            var entry = archive.CreateEntry($"3D/Objects/object_{model.ModelId}.model");
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

                // Resources section
                writer.WriteStartElement("resources");

                // Create a single combined mesh object
                writer.WriteStartElement("object");
                writer.WriteAttributeString("id", "1");
                writer.WriteAttributeString("type", "model");

                writer.WriteStartElement("mesh");

                // Write vertices
                writer.WriteStartElement("vertices");
                foreach (var vertex in model.MeshData.Vertices)
                {
                    writer.WriteStartElement("vertex");
                    writer.WriteAttributeString("x", vertex.X.ToString());
                    writer.WriteAttributeString("y", vertex.Y.ToString());
                    writer.WriteAttributeString("z", vertex.Z.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // vertices

                // Write triangles
                writer.WriteStartElement("triangles");
                foreach (var triangle in model.MeshData.Triangles)
                {
                    writer.WriteStartElement("triangle");
                    writer.WriteAttributeString("v1", triangle.V1.ToString());
                    writer.WriteAttributeString("v2", triangle.V2.ToString());
                    writer.WriteAttributeString("v3", triangle.V3.ToString());
                    if (!string.IsNullOrEmpty(triangle.PaintColor))
                    {
                        writer.WriteAttributeString("paint_color", triangle.PaintColor);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // triangles

                writer.WriteEndElement(); // mesh
                writer.WriteEndElement(); // object

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

        private void CreateModelSettingsFile(ZipArchive archive, List<ThreeMFPlate> plates)
        {
            var entry = archive.CreateEntry("Metadata/model_settings.config");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(BambuStudioMetadata.GetModelSettings(plates));
            }
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