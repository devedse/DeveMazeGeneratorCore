using DeveMazeGeneratorCore.MonoGame.Core.Data;
using Microsoft.Xna.Framework.Content;
using System.Reflection;

namespace DeveMazeGeneratorCore.MonoGame.Blazor
{
    public class CustomEmbeddedResourceLoader : IContentManagerExtension
    {
        public Stream OpenStream(ContentManager content, string assetName)
        {
            Assembly asm = this.GetType().Assembly;
            var asmName = asm.GetName().Name;

            var assetFullPath = Path.Combine(asmName, content.RootDirectory, assetName);
            assetFullPath = assetFullPath.Replace('/', '.');

            Stream stream = asm.GetManifestResourceStream(assetFullPath + ".xnb");

            return stream;
        }
    }
}
