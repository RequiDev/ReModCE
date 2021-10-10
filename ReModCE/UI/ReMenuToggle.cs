using System;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements.Controls;

namespace ReModCE.UI
{
    internal class ReMenuToggle : UIElement
    {
        private static GameObject _togglePrefab;
        private static GameObject TogglePrefab
        {
            get
            {
                if (_togglePrefab == null)
                {
                    // UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UI_Elements_Row_1/Button_ToggleQMInfo
                    _togglePrefab = ExtendedQuickMenu.Instance.container
                        .Find("Window/QMParent/Menu_Settings/Panel_QM_ScrollRect").GetComponent<ScrollRect>().content
                        .Find("Buttons_UI_Elements_Row_1/Button_ToggleQMInfo").gameObject;
                }
                return _togglePrefab;
            }
        }

        private static Sprite _onIconSprite;

        private static Sprite OnIconSprite
        {
            get
            {
                if (_onIconSprite == null)
                {
                    _onIconSprite = ExtendedQuickMenu.Instance.container
                        .Find("Window/QMParent/Menu_Notifications/Panel_NoNotifications_Message/Icon").GetComponent<Image>().sprite;
                }
                return _onIconSprite;
            }
        }

        private readonly Toggle _toggleComponent;

        public bool Interactable
        {
            get => _toggleComponent.interactable;
            set => _toggleComponent.interactable = value;
        }

        public ReMenuToggle(string name, string text, string tooltip, Action<bool> onToggle, Transform parent) : base(TogglePrefab, parent, $"Button_Toggle{name}")
        {
            var iconOn = RectTransform.Find("Icon_On").GetComponent<Image>();
            var iconOff = RectTransform.Find("Icon_Off");

            var toggleIcon = GameObject.GetComponent<ToggleIcon>();

            _toggleComponent = GameObject.GetComponent<Toggle>();
            _toggleComponent.onValueChanged = new Toggle.ToggleEvent();
            _toggleComponent.onValueChanged.AddListener(new Action<bool>(toggleIcon.OnValueChanged));
            _toggleComponent.onValueChanged.AddListener(new Action<bool>(onToggle));

            var tmp = GameObject.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = text;
            tmp.richText = true;

            var uiTooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiToggleTooltip>();
            uiTooltip.text = tooltip;
            uiTooltip.alternateText = tooltip;

            iconOn.sprite = OnIconSprite;
        }

        public void Toggle(bool value)
        {
            if (value != _toggleComponent.isOn)
            {
                _toggleComponent.InternalToggle();
            }
        }
    }
}
