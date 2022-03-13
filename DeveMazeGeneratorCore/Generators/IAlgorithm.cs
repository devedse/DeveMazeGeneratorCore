using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;

namespace DeveMazeGeneratorCore.Generators
{
    public interface IAlgorithm<out MazeType>
    {
        MazeType GoGenerate<M, TAction>(int width, int height, int seed, IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, TAction pixelChangedCallback) where M : InnerMap where TAction : struct, IProgressAction;
    }
}
