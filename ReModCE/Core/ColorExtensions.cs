using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReModCE.Core
{
    internal static class ColorExtensions
    {
        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        public static Color ToColor(this Vector4 v4)
        {
            return new Color(v4.x, v4.y, v4.z, v4.w);
        }

        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
