using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.Generators.SpeedOptimization
{
    public readonly struct NoAction : IProgressAction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int step, int total, long x, long y)
        {

        }
    }
}
