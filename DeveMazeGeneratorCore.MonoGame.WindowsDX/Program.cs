using DeveMazeGeneratorCore.MonoGame.Core;
using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using DeveMazeGeneratorMonoGame;
using System;

namespace DeveMazeGeneratorCore.MonoGame.WindowsDX
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            using (var game = new TheGame(new(720 / 2, 1280 / 2), Platform.Desktop))
            {
                game.Run();
            }
        }
    }
}
