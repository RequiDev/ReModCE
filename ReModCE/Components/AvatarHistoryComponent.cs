using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader.ICSharpCode.SharpZipLib.GZip;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
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
        private ConfigValue<bool> AvatarHistoryExcludeOwn;
        private ReMenuToggle _enabledToggle;
        private ReMenuToggle _excludeOwnToggle;

        public AvatarHistoryComponent()
        {
            AvatarHistoryEnabled = new ConfigValue<bool>(nameof(AvatarHistoryEnabled), true);
            AvatarHistoryEnabled.OnValueChanged += () =>
            {
                _enabledToggle?.Toggle(AvatarHistoryEnabled);
                _avatarList.GameObject.SetActive(AvatarHistoryEnabled);
            };

            AvatarHistoryExcludeOwn = new ConfigValue<bool>(nameof(AvatarHistoryExcludeOwn), false);
            AvatarHistoryExcludeOwn.OnValueChanged += () =>
            {
                _excludeOwnToggle.Toggle(AvatarHistoryExcludeOwn);
            };

            if (File.Exists("UserData/ReModCE/recent_avatars.bin"))
            {
                try
                {
                    _recentAvatars =
                        BinaryGZipSerializer.Deserialize("UserData/ReModCE/recent_avatars.bin") as List<ReAvatar>;
                }
                catch (GZipException e)
                {
                    ReLogger.Error($"Your recent avatars file seems to be corrupted. I renamed it for you, so this error doesn't happen again.");
                    File.Delete("UserData/ReModCE/recent_avatars.bin.corrupted");
                    File.Move("UserData/ReModCE/recent_avatars.bin", "UserData/ReModCE/recent_avatars.bin.corrupted");
                }
            }
            else
            {
                _recentAvatars = new List<ReAvatar>();
            }
        }

        public override void OnUiManagerInitEarly()
        {
            _avatarList = new ReAvatarList("Recently Used", this, true, false);

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

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetMenuPage("Avatars");
            _enabledToggle = menu.AddToggle("Avatar History", "Enable/Disable avatar history",
                AvatarHistoryEnabled.SetValue, AvatarHistoryEnabled);
            _excludeOwnToggle = menu.AddToggle("Exclude own avatars", "Exclude own avatars for avatar history",
                AvatarHistoryExcludeOwn.SetValue, AvatarHistoryExcludeOwn);
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

            if (AvatarHistoryExcludeOwn && avatar.authorId == APIUser.CurrentUser.id)
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

        public void Clear(ReAvatarList avatarList)
        {
            _recentAvatars.Clear();
            SaveAvatarsToDisk();
            avatarList.RefreshAvatars();
        }
    }
}
