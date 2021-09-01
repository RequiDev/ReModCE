using System;
using Newtonsoft.Json;
using VRC.Core;

namespace ReModCE.Core
{
    [Serializable]
    internal class ReAvatar
    {
        public string Id { get; set; }
        public string AvatarName { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public string AssetUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public ApiModel.SupportedPlatforms SupportedPlatforms = ApiModel.SupportedPlatforms.StandaloneWindows;

        public ReAvatar()
        {
        }

        public ReAvatar(ApiAvatar apiAvatar)
        {
            Id = apiAvatar.id;
            AvatarName = apiAvatar.name;
            AuthorId = apiAvatar.authorId;
            AuthorName = apiAvatar.authorName;
            Description = apiAvatar.description;
            AssetUrl = apiAvatar.assetUrl;
            ThumbnailUrl = apiAvatar.thumbnailImageUrl;
            SupportedPlatforms = apiAvatar.supportedPlatforms;
        }

        public ApiAvatar AsApiAvatar()
        {
            return new ApiAvatar
            {
                id = Id,
                name = AvatarName,
                authorId = AuthorId,
                authorName = AuthorName,
                description = Description,
                assetUrl = AssetUrl,
                thumbnailImageUrl = string.IsNullOrEmpty(ThumbnailUrl) ? (string.IsNullOrEmpty(ImageUrl) ? "https://assets.vrchat.com/system/defaultAvatar.png" : ImageUrl) : ThumbnailUrl,
                releaseStatus = "public",
                unityVersion = "2019.4.29f1",
                version = 1,
                apiVersion = 1,
                Endpoint = "avatars",
                Populated = false,
                assetVersion = new AssetVersion("2019.4.29f1", 0),
                tags = new Il2CppSystem.Collections.Generic.List<string>(0),
                supportedPlatforms = SupportedPlatforms,
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
