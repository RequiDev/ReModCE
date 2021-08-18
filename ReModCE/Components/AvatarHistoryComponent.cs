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
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.Components
{
    internal class AvatarHistoryComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _avatarList;
        private readonly List<ReAvatar> _recentAvatars;

        public AvatarHistoryComponent()
        {
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
            _avatarList = new ReAvatarList("Recently Used", this, false);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            changeButton.GetComponent<Button>().onClick.AddListener(new Action(OnChangeAvatar));
        }

        public override void OnAvatarIsReady(VRCPlayer vrcPlayer)
        {
            if (vrcPlayer.gameObject == VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject)
            {
                AddAvatarToHistory(vrcPlayer.GetPlayer().GetApiAvatar());
            }
        }

        private void AddAvatarToHistory(ApiAvatar avatar)
        {
            if (_recentAvatars.FirstOrDefault(a => a.Id == avatar.id) != null)
            {
                _recentAvatars.RemoveAll(a => a.Id == avatar.id);
            }

            _recentAvatars.Insert(0, new ReAvatar(avatar));

            if (_recentAvatars.Count > 25)
            {
                _recentAvatars.Remove(_recentAvatars.Last());
            }

            SaveAvatarsToDisk();

            _avatarList.Refresh(GetAvatars());
        }

        private void OnChangeAvatar()
        {
            var apiAvatar = _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
            AddAvatarToHistory(apiAvatar);
        }

        private void SaveAvatarsToDisk()
        {
            Directory.CreateDirectory("UserData/ReModCE");
            BinaryGZipSerializer.Serialize(_recentAvatars, "UserData/ReModCE/recent_avatars.bin");
        }

        public AvatarList GetAvatars()
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
