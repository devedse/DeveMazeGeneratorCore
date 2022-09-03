using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using DeveMazeGeneratorMonoGame;
using System;

namespace DeveMazeGeneratorCore.MonoGame.DesktopGL
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            using (var game = new TheGame(new(2460, 1340), Platform.Desktop))
            {
                game.Run();
            }
        }
    }
}
