using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements.Controls;

namespace ReModCE.UI
{
    internal class ReTabButton : UIElement
    {
        private static GameObject _tabButtonPrefab;
        private static GameObject TabButtonPrefab
        {
            get
            {
                if (_tabButtonPrefab == null)
                {
                    _tabButtonPrefab = ExtendedQuickMenu.Instance.devToolsButton;
                }
                return _tabButtonPrefab;
            }
        }

        public ReTabButton(string name, string tooltip, string pageName, Sprite sprite) : base(TabButtonPrefab, TabButtonPrefab.transform.parent, $"Page_{name}")
        {
            var menuTab = RectTransform.GetComponent<MenuTab>();
            menuTab.pageName = $"QuickMenu{pageName}";
            menuTab._menuStateController = ExtendedQuickMenu.MenuStateCtrl;

            var button = GameObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(menuTab.ShowTabContent));

            var uiTooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
            uiTooltip.text = tooltip;
            uiTooltip.alternateText = tooltip;

            var iconImage = RectTransform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = sprite;
            iconImage.overrideSprite = sprite;
        }
    }
}
