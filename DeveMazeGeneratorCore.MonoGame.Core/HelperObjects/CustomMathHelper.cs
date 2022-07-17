namespace DeveMazeGeneratorCore.MonoGame.Core.HelperObjects
{
    public static class CustomMathHelper
    {
        public static bool SameSign(float num1, float num2)
        {
            if (num1 > 0 && num2 < 0)
                return false;
            if (num1 < 0 && num2 > 0)
                return false;
            return true;
        }
    }
}
