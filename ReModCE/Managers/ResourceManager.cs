using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ReModCE.Managers
{
    internal static class ResourceManager
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static Texture2D GetTexture(string resourceName)
        {
            if (Textures.ContainsKey(resourceName))
            {
                return Textures[resourceName];
            }

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ReModCE.Resources.{resourceName}.png");
            if (stream == null)
            {
                throw new ArgumentException("Resource doesn't exist", resourceName);
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);

            var texture = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture, ms.ToArray());
            texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Textures.Add(resourceName, texture);

            return texture;
        }

        public static Sprite GetSprite(string resourceName)
        {
            if (Sprites.ContainsKey(resourceName))
            {
                return Sprites[resourceName];
            }

            var texture = GetTexture(resourceName);

            var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.zero;
            var sprite = Sprite.CreateSprite_Injected(texture, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Sprites.Add(resourceName, sprite);

            return sprite;
        }
    }
}
