using System;
using System.Linq;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReQuickButton : UIElement
    {
        private readonly Text _textComponent;
        public string Text
        {
            get => _textComponent?.text;
            set
            {
                if (_textComponent != null)
                {
                    _textComponent.text = value;
                    _textComponent.resizeTextForBestFit = true;
                }
            }
        }

        private readonly Button _buttonComponent;
        public bool Interactable
        {
            get => _buttonComponent.interactable;
            set => _buttonComponent.interactable = value;
        }

        public ReQuickButton(Vector2 pos, string text, string tooltip, Action onClick, Transform parent = null) : base(ExtendedQuickMenu.ReportWorldButton.gameObject, parent, pos, $"{text}Button")
        {
            Object.DestroyImmediate(RectTransform.Find("CLIcon"));

            _textComponent = GameObject.GetComponentInChildren<Text>();
            _textComponent.resizeTextForBestFit = true;
            Text = text;

            var tooltipComponent = GameObject.GetComponent<UiTooltip>();
            tooltipComponent.field_Public_String_0 = tooltip;
            tooltipComponent.field_Public_String_1 = tooltip;

            _buttonComponent = GameObject.GetComponent<Button>();
            _buttonComponent.onClick = new Button.ButtonClickedEvent();
            _buttonComponent.onClick.AddListener(new Action(onClick));
        }

        public static void Create(Vector2 pos, string text, string tooltip, Action onClick, Transform parent = null)
        {
            var _ = new ReQuickButton(pos, text, tooltip, onClick, parent);
        }
    }
}
