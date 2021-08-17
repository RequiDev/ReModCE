using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using Category = UiAvatarList.EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.UI
{
    internal interface IAvatarListOwner
    {
        AvatarList GetAvatars();
    }

    internal class ReAvatarList : UIElement
    {
        private static GameObject _legacyAvatarList;
        private static GameObject LegacyAvatarList
        {
            get
            {
                if (_legacyAvatarList == null)
                {
                    _legacyAvatarList = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Legacy Avatar List");
                }

                return _legacyAvatarList;
            }
        }

        private readonly UiAvatarList _avatarList;

        private readonly bool _hasPagination;
        private readonly ReUiButton _refreshButton;
        private readonly ReUiButton _nextPageButton;
        private readonly ReUiButton _prevPageButton;
        private readonly ReUiText _pageCount;

        private int _currentPage;

        private const int MaxAvatarsPerPage = 100;

        public SimpleAvatarPedestal AvatarPedestal => _avatarList.field_Public_SimpleAvatarPedestal_0;

        private readonly Text _textComponent;

        public string Title
        {
            get => _textComponent.text;
            set => _textComponent.text = value;
        }

        private readonly string _title;

        private readonly IAvatarListOwner _owner;
        public ReAvatarList(string title, IAvatarListOwner owner, bool addPagination = true) : base(
            LegacyAvatarList,
            LegacyAvatarList.transform.parent,
            $"{title}AvatarList")
        {
            _hasPagination = addPagination;
            _owner = owner;
            _title = title;

            _avatarList = GameObject.GetComponent<UiAvatarList>();
            _avatarList.clearUnseenListOnCollapse = false;
            _avatarList.field_Public_EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique_0 =
                Category.SpecificList;
            GameObject.transform.SetSiblingIndex(0);

            var enableDisableListener = GameObject.AddComponent<EnableDisableListener>();
            enableDisableListener.OnEnableEvent += RefreshAvatars;

            var expandButton = GameObject.GetComponentInChildren<Button>(true);
            _textComponent = expandButton.GetComponentInChildren<Text>();
            Title = title;

            _refreshButton = new ReUiButton("↻", new Vector3(980f, 0f), new Vector2(0.25f, 1), RefreshAvatars, expandButton.transform);
            if (_hasPagination)
            {
                _nextPageButton = new ReUiButton("→", new Vector2(900f, 0f), new Vector2(0.25f, 1f), () =>
                {
                    _currentPage += 1;
                    Refresh(_owner.GetAvatars());
                }, expandButton.transform);

                _prevPageButton = new ReUiButton("←", new Vector2(750f, 0f), new Vector2(0.25f, 1f), () =>
                {
                    _currentPage -= 1;
                    Refresh(_owner.GetAvatars());
                }, expandButton.transform);

                _pageCount = new ReUiText("0 / 0", new Vector2(825f, 0f), new Vector2(0.25f, 1f), expandButton.transform);
            }
        }

        private void RefreshAvatars()
        {
            Refresh(_owner.GetAvatars());
        }

        public void Refresh(AvatarList avatars)
        {
            if (_hasPagination)
            {
                var pagesCount = avatars.Count / MaxAvatarsPerPage;
                _currentPage = Mathf.Clamp(_currentPage, 0, pagesCount);

                _pageCount.Text = $"{_currentPage + 1} / {pagesCount + 1}";
                var cutDown = avatars.GetRange(_currentPage * MaxAvatarsPerPage,
                    Math.Abs(_currentPage * MaxAvatarsPerPage - avatars.Count));
                if (cutDown.Count > MaxAvatarsPerPage)
                {
                    cutDown.RemoveRange(MaxAvatarsPerPage, cutDown.Count - MaxAvatarsPerPage);
                }

                _prevPageButton.Interactable = _currentPage > 0;
                _nextPageButton.Interactable = _currentPage < avatars.Count / MaxAvatarsPerPage;

                Title = $"{_title} ({cutDown.Count}/{avatars.Count})";

                _avatarList.StartRenderElementsCoroutine(cutDown);
            }
            else
            {
                _avatarList.StartRenderElementsCoroutine(avatars);
            }
        }
    }
}
