using System;
using System.Linq;
using ReModCE.Loader;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReMenuButton : UIElement
    {
        private static GameObject _buttonPrefab;

        private static GameObject ButtonPrefab
        {
            get
            {
                if (_buttonPrefab == null)
                {
                    _buttonPrefab = ExtendedQuickMenu.Instance.container
                        .Find("Window/QMParent/Menu_Dashboard/ScrollRect").GetComponent<ScrollRect>().content
                        .Find("Buttons_QuickActions/Button_Respawn").gameObject;
                }
                return _buttonPrefab;
            }
        }

        private readonly TextMeshProUGUI _text;

        public string Text
        {
            get => _text.text;
            set => _text.SetText(value);
        }

        private readonly Button _button;
        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public ReMenuButton(string name, string text, string tooltip, Action onClick, Transform parent) : base(ButtonPrefab, parent,
            $"Button_{name}")
        {
            _text = GameObject.GetComponentInChildren<TextMeshProUGUI>();
            _text.text = text;
            _text.richText = true;
            _text.fontSize = 38;
            //_text.autoSizeTextContainer = true;
            _text.color = new Color(0.4157f, 0.8902f, 0.9765f, 1f);
            _text.m_fontColor = new Color(0.4157f, 0.8902f, 0.9765f, 1f);
            _text.m_htmlColor = new Color(0.4157f, 0.8902f, 0.9765f, 1f);
            _text.transform.localPosition = new Vector3(_text.transform.localPosition.x, -30f);

            var styleElement = _text.GetComponent<StyleElement>();
            styleElement.classes = "H1";

            Object.DestroyImmediate(RectTransform.Find("Icon").gameObject);
            Object.DestroyImmediate(RectTransform.Find("Icon_Secondary").gameObject);
            Object.DestroyImmediate(RectTransform.Find("Badge_Close").gameObject);
            Object.DestroyImmediate(RectTransform.Find("Badge_MMJump").gameObject);

            var uiTooltip = GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
            uiTooltip.text = tooltip;
            uiTooltip.alternateText = tooltip;

            _button = GameObject.GetComponent<Button>();
            _button.onClick = new Button.ButtonClickedEvent();
            _button.onClick.AddListener(new Action(onClick));
        }
    }
}
