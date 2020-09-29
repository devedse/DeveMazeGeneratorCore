using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGenerator.Generators.SpeedOptimization
{
    public readonly struct ProgressAction : IProgressAction
    {
        private readonly Action<int, int, long, long> _pixelChangedCallback;

        public ProgressAction(Action<int, int, long, long> pixelChangedCallback)
        {
            _pixelChangedCallback = pixelChangedCallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int step, int total, long x, long y)
        {
            _pixelChangedCallback(step, total, x, y);
        }
    }
}
