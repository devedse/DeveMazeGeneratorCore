using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.InnerMaps;
using System;

namespace DeveMazeGeneratorCore.Generators
{
    public abstract class Algorithm
    {
        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public InnerMap Generate<M, R>(int width, int height, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            return Generate<M, R>(width, height, Environment.TickCount, pixelChangedCallback);
        }

        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public InnerMap Generate<M, R>(int width, int height, int seed, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            if (pixelChangedCallback == null)
            {
                pixelChangedCallback = (x, y, z, u) => { };
            }

            var innerMapFactory = new InnerMapFactory<M>(width, height);
            var randomFactory = new RandomFactory<R>(seed);

            var generatedMap = GoGenerate(innerMapFactory, randomFactory, pixelChangedCallback);

            return generatedMap;
        }

        public abstract InnerMap GoGenerate<M>(IInnerMapFactory<M> mapFactory, IRandomFactory randomFactory, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap;
    }
}
