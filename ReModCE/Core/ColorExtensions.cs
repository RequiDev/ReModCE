using UnityEngine;

namespace ReModCE.Core
{
    internal static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
