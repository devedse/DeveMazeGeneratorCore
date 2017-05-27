using DeveMazeGenerator.Factories;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.InnerMaps;
using System;

namespace DeveMazeGenerator.Generators
{
    public abstract class AlgorithmWithPath : Algorithm
    {
        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public (InnerMap Maze, InnerMap PathMap) GenerateWithPath<M, R>(int width, int height, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            return GenerateWithPath<M, R>(width, height, Environment.TickCount, pixelChangedCallback);
        }

        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public (InnerMap Maze, InnerMap PathMap) GenerateWithPath<M, R>(int width, int height, int seed, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            if (pixelChangedCallback == null)
            {
                pixelChangedCallback = (x, y, z, u) => { };
            }

            var innerMapFactory = new InnerMapFactory<M>(width, height);
            var randomFactory = new RandomFactory<R>(seed);

            var generatedMap = GoGenerateWithPath(innerMapFactory, randomFactory, pixelChangedCallback);

            return generatedMap;
        }

        public override InnerMap GoGenerate<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback)
        {
            return GoGenerateWithPath(mapFactory, randomFactory, pixelChangedCallback).Maze;
        }

        public abstract (InnerMap Maze, InnerMap PathMap) GoGenerateWithPath<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap;
    }
}
