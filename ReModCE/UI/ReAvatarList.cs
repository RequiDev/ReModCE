using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Loader;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using Category = UiAvatarList.EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique;

namespace ReModCE.UI
{
    internal class ReAvatarList : UIElement
    {
        private static Transform _legacyAvatarList;

        private UiAvatarList _avatarList;

        private Il2CppSystem.Collections.Generic.List<ApiAvatar> _savedAvatars =
            new Il2CppSystem.Collections.Generic.List<ApiAvatar>();

        public ReAvatarList(string title) : base(
            LegacyAvatarList.gameObject,
            LegacyAvatarList.parent,
            $"{title}AvatarList")
        {
            _avatarList = gameObject.GetComponent<UiAvatarList>();
            _avatarList.clearUnseenListOnCollapse = false;
            _avatarList.field_Public_EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique_0 =
                Category.SpecificList;
            gameObject.transform.SetSiblingIndex(0);

            var expandButton = gameObject.GetComponentInChildren<Button>(true);
            var textComponent = expandButton.GetComponentInChildren<Text>();
            textComponent.text = title;

            var refreshButton = new ReUiButton("↻", new Vector3(980f, 0f),
                new Vector2(0.25f, 1), Refresh, expandButton.transform);
        }

        public void Refresh()
        {
            _avatarList.StartRenderElementsCoroutine(_savedAvatars);
        }

        private static Transform LegacyAvatarList
        {
            get
            {
                if (_legacyAvatarList == null)
                {
                    _legacyAvatarList = VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform
                        .Find("Screens/Avatar/Vertical Scroll View/Viewport/Content/Legacy Avatar List");
                }

                return _legacyAvatarList;
            }
        }
    }
}
