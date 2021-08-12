using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.Core;

namespace ReModCE.Core
{
    internal class ReAvatar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public string AssetUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public ApiModel.SupportedPlatforms SupportedPlatforms = ApiModel.SupportedPlatforms.StandaloneWindows;

        public ReAvatar()
        {

        }

        public ReAvatar(ApiAvatar apiAvatar)
        {
            Id = apiAvatar.id;
            Name = apiAvatar.name;
            AuthorId = apiAvatar.authorId;
            AuthorName = apiAvatar.authorName;
            Description = apiAvatar.description;
            AssetUrl = apiAvatar.assetUrl;
            ThumbnailImageUrl = apiAvatar.thumbnailImageUrl;
            SupportedPlatforms = apiAvatar.supportedPlatforms;
        }

        public ApiAvatar AsApiAvatar()
        {
            return new ApiAvatar
            {
                id = Id,
                name = Name,
                authorId = AuthorId,
                authorName = AuthorName,
                description = Description,
                assetUrl = AssetUrl,
                thumbnailImageUrl = ThumbnailImageUrl,
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
    }
}
