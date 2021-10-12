using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    [Flags]
    public enum WingSide
    {
        Left = (1 << 0),
        Right = (1 << 1),
        Both = Left | Right
    }

    internal class ReWingMenu : UIElement
    {
        private static GameObject _wingMenuPrefab;

        private static GameObject WingMenuPrefab
        {
            get
            {
                if (_wingMenuPrefab == null)
                {
                    _wingMenuPrefab = ExtendedQuickMenu.LeftWing.innerContainer.Find("WingMenu").gameObject;
                }
                return _wingMenuPrefab;
            }
        }

        private readonly Wing _wing;
        private readonly string _menuName;
        private readonly Transform _container;
        
        public ReWingMenu(string name, string text, bool left = true) : base(WingMenuPrefab, (left ? ExtendedQuickMenu.LeftWing : ExtendedQuickMenu.RightWing).innerContainer, name, false)
        {
            _menuName = name;
            _wing = left ? ExtendedQuickMenu.LeftWing : ExtendedQuickMenu.RightWing;

            var headerTransform = RectTransform.GetChild(0);
            var titleText = headerTransform.GetComponentInChildren<TextMeshProUGUI>();
            titleText.text = text;
            titleText.richText = true;

            var backButton = headerTransform.GetComponentInChildren<Button>(true);
            backButton.gameObject.SetActive(true);

            var backIcon = backButton.transform.Find("Icon");
            backIcon.gameObject.SetActive(true);
            var components = new Il2CppSystem.Collections.Generic.List<Behaviour>();
            backButton.GetComponents(components);

            foreach (var comp in components)
            {
                comp.enabled = true;
            }

            var content = RectTransform.GetComponentInChildren<ScrollRect>().content;
            foreach (var obj in content)
            {
                var control = obj.Cast<Transform>();
                if (control == null)
                {
                    continue;
                }

                Object.Destroy(control.gameObject);
            }

            _container = content;

            var uiPage = GameObject.GetComponent<UIPage>();
            uiPage.Name = name;
            uiPage._inited = true;
            uiPage._menuStateController = _wing.menuController;
            uiPage._pageStack = new Il2CppSystem.Collections.Generic.List<UIPage>();
            uiPage._pageStack.Add(uiPage);

            _wing.menuController._uiPages.Add(uiPage.Name, uiPage);
        }

        public void Open()
        {
            _wing.menuController.PushPage(_menuName);
        }

        public ReWingButton AddButton(string name, string text, string tooltip, Action onClick, bool arrow = true, bool background = true, bool seperator = false)
        {
            return new ReWingButton(name, text, tooltip, onClick, _container, arrow, background, seperator);
        }

        public ReWingMenu AddSubMenu(string name, string text, string tooltip)
        {
            var menu = new ReWingMenu(name, text, _wing.wingType == Wing.WingPanel.Left);
            AddButton(name, text, tooltip, menu.Open);
            return menu;
        }
    }

    internal class ReWingButton : UIElement
    {
        private static GameObject _wingButtonPrefab;
        private static GameObject WingButtonPrefab
        {
            get
            {
                if (_wingButtonPrefab == null)
                {
                    _wingButtonPrefab = ExtendedQuickMenu.LeftWing.transform.Find("Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Profile").gameObject;
                }
                return _wingButtonPrefab;
            }
        }

        public ReWingButton(string name, string text, string tooltip, Action onClick, Sprite sprite = null, bool left = true, bool arrow = true, bool background = true,
            bool seperator = false) : base(WingButtonPrefab, (left ? ExtendedQuickMenu.LeftWing : ExtendedQuickMenu.RightWing).innerContainer.Find("WingMenu/ScrollRect/Viewport/VerticalLayoutGroup"), $"Button_{name}")
        {
            var container = RectTransform.Find("Container").transform;
            container.Find("Background").gameObject.SetActive(background);
            container.Find("Icon_Arrow").gameObject.SetActive(arrow);
            RectTransform.Find("Separator").gameObject.SetActive(seperator);
            var iconImage = container.Find("Icon").GetComponent<Image>();
            if (sprite != null)
            {
                iconImage.sprite = sprite;
                iconImage.overrideSprite = sprite;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }

            var tmp = container.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = text;
            tmp.richText = true;

            var button = GameObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(onClick));

            var uiTooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
            uiTooltip.text = tooltip;
            uiTooltip.alternateText = tooltip;
        }

        public ReWingButton(string name, string text, string tooltip, Action onClick, Transform parent, bool arrow = true, bool background = true,
            bool seperator = false) : base(WingButtonPrefab, parent, $"Button_{name}")
        {
            var container = RectTransform.Find("Container").transform;
            container.Find("Background").gameObject.SetActive(background);
            container.Find("Icon_Arrow").gameObject.SetActive(arrow);
            RectTransform.Find("Separator").gameObject.SetActive(seperator);
            container.Find("Icon").gameObject.SetActive(false);

            var tmp = container.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = text;
            tmp.richText = true;

            var button = GameObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(onClick));

            var uiTooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
            uiTooltip.text = tooltip;
            uiTooltip.alternateText = tooltip;
        }
    }
}
