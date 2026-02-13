using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Structures;
using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Mazes;

namespace DeveMazeGeneratorCore.Generators.Helpers
{
    internal static class AlgorithmBacktrack2Deluxe2_Entry
    {
        public static Maze GoGenerate<M, TAction>(
            int width, 
            int height, 
            int seed, 
            IInnerMapFactory<M> mapFactory, 
            IRandomFactory randomFactory, 
            TAction pixelChangedCallback)
            where M : InnerMap
            where TAction : struct, IProgressAction
        {
            var innerMap = mapFactory.Create(width, height);
            var random = randomFactory.Create(seed);

            // This dispatch logic is isolated here to keep the main algorithm clean.
            // It selects the most optimized accessor for the specific map type.
            if (innerMap is BitArreintjeFastInnerMapUnsafeV2 fastMap)
            {
                return AlgorithmBacktrack2Deluxe2_AsByte.GoGenerateInternal<BitArreintjeFastInnerMapUnsafeV2, FastV2Accessor, TAction>(fastMap, random, pixelChangedCallback);
            }

            return AlgorithmBacktrack2Deluxe2_AsByte.GoGenerateInternal<M, GenericAccessor<M>, TAction>(innerMap, random, pixelChangedCallback);
        }
    }
}