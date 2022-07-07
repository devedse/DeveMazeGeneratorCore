using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace DeveMazeGeneratorCore.MonoGame.Core.Data
{
    public class ExtendibleContentManager : ContentManager
    {
        private readonly IContentManagerExtension _contentManagerExtension;

        /// <summary>
        /// This class is used within Blazor since it loads assets in a custom way
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contentManagerExtension"></param>
        public ExtendibleContentManager(GameServiceContainer serviceProvider, IContentManagerExtension contentManagerExtension) : base(serviceProvider)
        {
            _contentManagerExtension = contentManagerExtension;
        }

        protected override Stream OpenStream(string assetName)
        {
            if (_contentManagerExtension == null)
            {
                return base.OpenStream(assetName);
            }
            else
            {
                return _contentManagerExtension.OpenStream(this, assetName);
            }
        }
    }
}
