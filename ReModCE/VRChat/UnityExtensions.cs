using System;
using System.IO;
using UnityEngine;

namespace ReModCE.VRChat
{
    internal static class UnityExtensions
    {
        public static string GetPath(this Transform current)
        {
            if (current.parent == null)
                return "/" + current.name;
            return current.parent.GetPath() + "/" + current.name;
        }

        public static T[] GetComponentsInDirectChildren<T>(this GameObject gameObject)
        {
            var indexer = 0;

            foreach (var child in gameObject.transform)
            {
                var transform = child.Cast<Transform>();
                if (transform.GetComponent<T>() != null)
                {
                    indexer++;
                }
            }

            var returnArray = new T[indexer];

            indexer = 0;

            foreach (var child in gameObject.transform)
            {
                var transform = child.Cast<Transform>();
                if (transform.GetComponent<T>() != null)
                {
                    returnArray[indexer++] = transform.GetComponent<T>();
                }
            }

            return returnArray;
        }

        /// <summary>
        /// Returns a copy of the float rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the float should be rounded to</param>
        public static float RoundAmount(this float i, float nearestFactor)
        {
            return (float)Math.Round(i / nearestFactor) * nearestFactor;
        }

        /// <summary>
        /// Returns a copy of the vector rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the vector should be rounded to</param>
        public static Vector3 RoundAmount(this Vector3 i, float nearestFactor)
        {
            return new Vector3(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor), i.z.RoundAmount(nearestFactor));
        }

        /// <summary>
        /// Returns a copy of the vector rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the vector should be rounded to</param>
        public static Vector2 RoundAmount(this Vector2 i, float nearestFactor)
        {
            return new Vector2(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor));
        }
    }
}
