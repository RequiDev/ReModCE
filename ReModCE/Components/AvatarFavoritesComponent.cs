using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IFavoriteListener
    {
        private ReAvatarList _avatarList;

        private readonly List<ReAvatar> _savedAvatars;

        public AvatarFavoritesComponent()
        {
            if (File.Exists("UserData/ReModCE/avatars.json"))
            {
                _savedAvatars = JsonConvert.DeserializeObject<List<ReAvatar>>(File.ReadAllText("UserData/ReModCE/avatars.json"));
            }
            else
            {
                _savedAvatars = new List<ReAvatar>();
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _avatarList = new ReAvatarList("ReModCE Favorites", this);
            _avatarList.SetAvatars(_savedAvatars.Select(x => x.AsApiAvatar()).ToList());

            if (uiManager.IsRemodLoaded)
            {
                _avatarList.FavoriteButton.Position += new Vector3(UiManager.ButtonSize, 0f);
            }
        }

        public void OnFavoriteAvatar(ApiAvatar avatar)
        {
            if (_savedAvatars.FirstOrDefault(a => a.Id == avatar.id) == null)
            {
                _savedAvatars.Add(new ReAvatar(avatar));
            }
            SaveAvatarsToDisk();
        }

        public void OnUnfavoriteAvatar(ApiAvatar avatar)
        {
            _savedAvatars.RemoveAll(a => a.Id == avatar.id);
            SaveAvatarsToDisk();
        }

        private void SaveAvatarsToDisk()
        {
            Directory.CreateDirectory("UserData/ReModCE");
            File.WriteAllText("UserData/ReModCE/avatars.json", JsonConvert.SerializeObject(_savedAvatars));
        }
    }
}
