using DeveMazeGeneratorCore.Factories;
using DeveMazeGeneratorCore.Generators;
using DeveMazeGeneratorCore.Generators.Helpers;
using DeveMazeGeneratorCore.Generators.SpeedOptimization;
using DeveMazeGeneratorCore.InnerMaps;
using DeveMazeGeneratorCore.Mazes;
using System;

namespace DeveMazeGeneratorCore
{
    public static class MazeGenerator
    {
        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="width">The width of the maze to generate</param>
        /// <param name="height">The height of the maze to generate</param>
        /// <param name="map">The inner map to generate the maze in</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public static Maze Generate<AlgorithmType, InnerMapType, RandomType>(int width, int height, Action<int, int, long, long> pixelChangedCallback)
            where AlgorithmType : IAlgorithm<Maze>, new()
            where InnerMapType : InnerMap
            where RandomType : IRandom
        {
            return Generate<AlgorithmType, InnerMapType, RandomType>(width, height, Environment.TickCount, pixelChangedCallback);
        }


        /// <summary>
        /// Generate a Maze
        /// </summary>
        /// <param name="width">The width of the maze to generate</param>
        /// <param name="height">The height of the maze to generate</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public static Maze Generate<AlgorithmType, InnerMapType, RandomType>(int width, int height, int seed, Action<int, int, long, long> pixelChangedCallback)
            where AlgorithmType : IAlgorithm<Maze>, new()
            where InnerMapType : InnerMap
            where RandomType : IRandom
        {
            var innerMapFactory = new InnerMapFactory<InnerMapType>();
            var randomFactory = new RandomFactory<RandomType>();

            var alg = new AlgorithmType();

            if (pixelChangedCallback == null)
            {
                var test = alg.GoGenerate(width, height, seed, innerMapFactory, randomFactory, new NoAction());
                return test;
            }
            else
            {
                return alg.GoGenerate(width, height, seed, innerMapFactory, randomFactory, new ProgressAction(pixelChangedCallback));
            }
        }

        /// <summary>
        /// Generate a Maze using optimized struct-based map accessors for better performance
        /// </summary>
        /// <param name="width">The width of the maze to generate</param>
        /// <param name="height">The height of the maze to generate</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public static Maze GenerateOptimized<InnerMapType>(int width, int height, Action<int, int, long, long> pixelChangedCallback)
            where InnerMapType : InnerMap
        {
            return GenerateOptimized<InnerMapType>(width, height, Environment.TickCount, pixelChangedCallback);
        }

        /// <summary>
        /// Generate a Maze using optimized struct-based map accessors for better performance
        /// </summary>
        /// <param name="width">The width of the maze to generate</param>
        /// <param name="height">The height of the maze to generate</param>
        /// <param name="seed">The seed that is used to generate a maze</param>
        /// <param name="pixelChangedCallback">When a pixel is changed you can define a callback here to for example draw the maze while its being generated, add null if you don't want this. Last 2 longs are for the current step and the total steps (can be used to calculate how far the maze is done being generated)</param>
        public static Maze GenerateOptimized<InnerMapType>(int width, int height, int seed, Action<int, int, long, long> pixelChangedCallback)
            where InnerMapType : InnerMap
        {
            var innerMapFactory = new InnerMapFactory<InnerMapType>();
            var mapAccessorFactory = new InnerMapAccessorFactory<InnerMapType>(innerMapFactory);
            var randomFactory = new RandomFactory<XorShiftRandom>();

            var alg = new AlgorithmBacktrack2Deluxe2_AsByte();

            if (pixelChangedCallback == null)
            {
                return alg.GoGenerateOptimized(width, height, seed, mapAccessorFactory, randomFactory, new NoAction());
            }
            else
            {
                return alg.GoGenerateOptimized(width, height, seed, mapAccessorFactory, randomFactory, new ProgressAction(pixelChangedCallback));
            }
        }
    }
}
