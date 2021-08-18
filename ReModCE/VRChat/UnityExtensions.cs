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
    }
}
