using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Loader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReQuickButton : UIElement
    {
        private static GameObject _reportWorldButton;
        private static GameObject ReportWorldButton
        {
            get
            {
                if (_reportWorldButton == null)
                {
                    _reportWorldButton = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/ReportWorldButton");
                }

                return _reportWorldButton;
            }
        }

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

        public ReQuickButton(Vector2 pos, string text, string tooltip, Action onClick, Transform parent = null) : base(ReportWorldButton, parent, pos, $"{text}Button")
        {
            Object.DestroyImmediate(GameObject.GetComponentsInChildren<Image>(true).First(a => a.transform != GameObject.transform));

            _textComponent = GameObject.GetComponentInChildren<Text>();
            _textComponent.text = text;
            _textComponent.resizeTextForBestFit = true;

            var tooltipComponent = GameObject.GetComponent<UiTooltip>();
            tooltipComponent.field_Public_String_0 = tooltip;
            tooltipComponent.field_Public_String_1 = tooltip;

            var buttonComponent = GameObject.GetComponent<Button>();
            buttonComponent.onClick = new Button.ButtonClickedEvent();
            buttonComponent.onClick.AddListener(new Action(onClick));
        }

        public static void Create(Vector2 pos, string text, string tooltip, Action onClick, Transform parent = null)
        {
            var _ = new ReQuickButton(pos, text, tooltip, onClick, parent);
        }
    }
}
