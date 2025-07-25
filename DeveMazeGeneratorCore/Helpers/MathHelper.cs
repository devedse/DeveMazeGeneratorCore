namespace DeveMazeGeneratorCore.Helpers
{
    public static class MathHelper
    {
        public static int RoundUpToNextEven(int x) => (x + 1) & ~1;
    }
}
