using DeveMazeGeneratorCore.MonoGame.Core;
using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using DeveMazeGeneratorMonoGame;
using System;
using System.Diagnostics;

namespace DeveMazeGeneratorCore.MonoGame.WindowsDX
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
