using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.Generators.SpeedOptimization
{
    public readonly struct NoAction : IProgressAction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int step, int total, long x, long y)
        {

        }
    }
}
