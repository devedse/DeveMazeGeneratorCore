using DeveMazeGenerator.InnerMaps;
using System;

namespace DeveMazeGenerator.Generators
{
    public interface Algorithm
    {
        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        void Generate(InnerMap map, Action<int, int, long, long> pixelChangedCallback);

        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        void Generate(InnerMap map, int seed, Action<int, int, long, long> pixelChangedCallback);
    }
}
