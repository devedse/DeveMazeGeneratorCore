namespace DeveMazeGenerator.Generators.SpeedOptimization
{
    public interface IProgressAction
    {
        void Invoke(int step, int total, long x, long y);
    }
}
