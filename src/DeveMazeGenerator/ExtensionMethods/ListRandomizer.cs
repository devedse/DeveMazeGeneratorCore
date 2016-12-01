using DeveMazeGenerator.Generators.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeveMazeGenerator.ExtensionMethods
{
    public static class ListRandomizer
    {
        /// <summary>
        /// Method to randomly sort a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static List<T> RandomPermutation<T>(this List<T> sequence, IRandom random)
        {
            T[] retArray = sequence.ToArray();

            for (int i = 0; i < retArray.Length - 1; i += 1)
            {
                int swapIndex = random.Next(i + 1, retArray.Length);
                T temp = retArray[i];
                retArray[i] = retArray[swapIndex];
                retArray[swapIndex] = temp;
            }

            return retArray.ToList();
        }
    }
}