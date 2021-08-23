using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _avatarList;
        private ReUiButton _favoriteButton;

        private readonly List<ReAvatar> _savedAvatars;

        private Button.ButtonClickedEvent _changeButtonEvent;

        public AvatarFavoritesComponent()
        {
            if (File.Exists("UserData/ReModCE/avatars.bin"))
            {
                _savedAvatars = BinaryGZipSerializer.Deserialize("UserData/ReModCE/avatars.bin") as List<ReAvatar>;
            }
            else
            {
                _savedAvatars = new List<ReAvatar>();
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _avatarList = new ReAvatarList("ReModCE Favorites", this);
            _avatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);

            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.5f, 1f), () => FavoriteAvatar(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            if (changeButton != null)
            {
                var button = changeButton.GetComponent<Button>();
                _changeButtonEvent = button.onClick;

                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(new Action(() =>
                {
                    var currentAvatar = _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
                    if (!HasAvatarFavorited(currentAvatar.id)) // this isn't in our list. we don't care about it
                    {
                        _changeButtonEvent.Invoke();
                        return;
                    }
                    
                    new ApiAvatar { id = currentAvatar.id }.Fetch(new Action<ApiContainer>(ac =>
                    {
                        var updatedAvatar = ac.Model.Cast<ApiAvatar>();
                        switch (updatedAvatar.releaseStatus)
                        {
                            case "private" when updatedAvatar.authorId != APIUser.CurrentUser.id:
                                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar is private and you don't own it. You can't switch into it.");
                                break;
                            case "unavailable":
                                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar has been deleted. You can't switch into it.");
                                break;
                            default:
                                _changeButtonEvent.Invoke();
                                break;
                        }
                    }), new Action<ApiContainer>(ac =>
                    {
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar has been deleted. You can't switch into it.");
                    }));
                }));
            }

            if (uiManager.IsRemodLoaded)
            {
                _favoriteButton.Position += new Vector3(UiManager.ButtonSize, 0f);
            }
        }

        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats)
        {
            _favoriteButton.Text = HasAvatarFavorited(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {
            var isSupporter = APIUser.CurrentUser.isSupporter;
            if (!isSupporter)
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "You need VRC+ to use this feature.\nWe're not trying to destroy VRChat's monetization.");
                return;
            }

            var hasFavorited = HasAvatarFavorited(apiAvatar.id);
            if (!hasFavorited)
            {
                _savedAvatars.Insert(0, new ReAvatar(apiAvatar));
                _favoriteButton.Text = "Unfavorite";
            }
            else
            {
                _savedAvatars.RemoveAll(a => a.Id == apiAvatar.id);
                _favoriteButton.Text = "Favorite";
            }
            SaveAvatarsToDisk();

            _avatarList.Refresh(GetAvatars());
        }

        private bool HasAvatarFavorited(string id)
        {
            return _savedAvatars.FirstOrDefault(a => a.Id == id) != null;
        }
        
        private void SaveAvatarsToDisk()
        {
            BinaryGZipSerializer.Serialize(_savedAvatars, "UserData/ReModCE/avatars.bin");
        }

        public AvatarList GetAvatars()
        {
            var list = new AvatarList();
            foreach (var avi in _savedAvatars.Distinct().Select(x => x.AsApiAvatar()).ToList())
            {
                list.Add(avi);
            }
            return list;
        }
    }
}
