using DeveMazeGeneratorCore.MonoGame.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DeveMazeGeneratorCore.MonoGame.WindowsUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private readonly TheGame _game;

        public GamePage()
        {
            InitializeComponent();

            // Create the game.
            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<TheGame>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
