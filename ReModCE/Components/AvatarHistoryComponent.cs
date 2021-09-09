using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.Components
{
    internal class AvatarHistoryComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _avatarList;
        private readonly List<ReAvatar> _recentAvatars;

        private Button.ButtonClickedEvent _changeButtonEvent;

        private ConfigValue<bool> AvatarHistoryEnabled;
        private ReQuickToggle _enabledToggle;
        public AvatarHistoryComponent()
        {
            AvatarHistoryEnabled = new ConfigValue<bool>(nameof(AvatarHistoryEnabled), true);
            AvatarHistoryEnabled.OnValueChanged += () =>
            {
                _enabledToggle.Toggle(AvatarHistoryEnabled);
                _avatarList.GameObject.SetActive(AvatarHistoryEnabled);
            };

            if (File.Exists("UserData/ReModCE/recent_avatars.bin"))
            {
                _recentAvatars =
                    BinaryGZipSerializer.Deserialize("UserData/ReModCE/recent_avatars.bin") as List<ReAvatar>;
            }
            else
            {
                _recentAvatars = new List<ReAvatar>();
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetSubMenu("Avatars");
            _enabledToggle = menu.AddToggle("Avatar History", "Enable/Disable avatar history",
                AvatarHistoryEnabled.SetValue, AvatarHistoryEnabled);

            _avatarList = new ReAvatarList("Recently Used", this, false);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            
            if (changeButton != null)
            {
                var button = changeButton.GetComponent<Button>();
                _changeButtonEvent = button.onClick;

                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(new Action(() =>
                {
                    var currentAvatar = _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
                    if (!IsAvatarInHistory(currentAvatar.id)) // this isn't in our list. we don't care about it
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
                                _recentAvatars.RemoveAll(a => a.Id == currentAvatar.id);
                                _avatarList.RefreshAvatars();
                                break;
                            default:
                                _changeButtonEvent.Invoke();
                                break;
                        }
                    }), new Action<ApiContainer>(ac =>
                    {
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar has been deleted. You can't switch into it.");
                        _recentAvatars.RemoveAll(a => a.Id == currentAvatar.id);
                    }));
                }));
            }
        }

        public override void OnAvatarIsReady(VRCPlayer vrcPlayer)
        {
            if (vrcPlayer.gameObject == VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject)
            {
                AddAvatarToHistory(vrcPlayer.GetPlayer().GetApiAvatar());
            }
        }

        private bool IsAvatarInHistory(string id)
        {
            return _recentAvatars.FirstOrDefault(a => a.Id == id) != null;
        }

        private void AddAvatarToHistory(ApiAvatar avatar)
        {
            if (avatar == null)
                return;
            if (avatar.IsLocal)
                return;

            if (IsAvatarInHistory(avatar.id))
            {
                _recentAvatars.RemoveAll(a => a.Id == avatar.id);
            }

            _recentAvatars.Insert(0, new ReAvatar(avatar));

            if (_recentAvatars.Count > 25)
            {
                _recentAvatars.Remove(_recentAvatars.Last());
            }

            SaveAvatarsToDisk();

            _avatarList.RefreshAvatars();
        }

        private void OnChangeAvatar()
        {
            var apiAvatar = _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
            AddAvatarToHistory(apiAvatar);
        }

        private void SaveAvatarsToDisk()
        {
            BinaryGZipSerializer.Serialize(_recentAvatars, "UserData/ReModCE/recent_avatars.bin");
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            var list = new AvatarList();
            foreach (var avi in _recentAvatars.Distinct().Select(x => x.AsApiAvatar()).ToList())
            {
                list.Add(avi);
            }
            return list;
        }
    }
}
