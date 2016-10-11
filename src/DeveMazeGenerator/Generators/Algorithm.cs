using DeveMazeGenerator.Factories;
using DeveMazeGenerator.Generators.Helpers;
using DeveMazeGenerator.InnerMaps;
using System;

namespace DeveMazeGenerator.Generators
{
    public abstract class Algorithm
    {
        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public M Generate<M, R>(int width, int height, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            return Generate<M, R>(width, height, Environment.TickCount, pixelChangedCallback);
        }

        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public M Generate<M, R>(int width, int height, int seed, Action<int, int, long, long> pixelChangedCallback) where M : InnerMap where R : IRandom
        {
            if (pixelChangedCallback == null)
            {
                pixelChangedCallback = (x, y, z, u) => { };
            }

            var random = RandomFactory.Create<R>(seed);
            var map = InnerMapFactory.Create<M>(width, height);

            GoGenerate(map, random, pixelChangedCallback);

            return map;
        }

        public abstract void GoGenerate(InnerMap map, IRandom random, Action<int, int, long, long> pixelChangedCallback);
    }
}
