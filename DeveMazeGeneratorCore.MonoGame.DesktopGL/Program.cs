using DeveMazeGeneratorMonoGame;
using System;

namespace DeveBlockStacker.DesktopGL
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var game = new TheGame();

            game.Run();
        }
    }
}
