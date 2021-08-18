using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReUiButton : UIElement
    {
        private readonly Text _textComponent;

        public string Text
        {
            get => _textComponent.text;
            set => _textComponent.text = value;
        }

        private readonly Button _buttonComponent;
        public bool Interactable
        {
            get => _buttonComponent.interactable;
            set => _buttonComponent.interactable = value;
        }

        public ReUiButton(string text, Vector2 pos, Vector2 scale, Action onClick, Transform parent = null) : base(
            GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button"), parent, pos,
            $"{text}UiButton")
        {

            _buttonComponent = GameObject.GetComponentInChildren<Button>();
            _buttonComponent.onClick = new Button.ButtonClickedEvent();
            _buttonComponent.onClick.AddListener(new Action(onClick));

            _textComponent = GameObject.GetComponentInChildren<Text>();
            Text = text;

            var allTextComponents = GameObject.GetComponentsInChildren<Text>(true);
            foreach (var t in allTextComponents)
            {
                if (t.transform == _textComponent.transform)
                    continue;

                Object.DestroyImmediate(t);
            }

            RectTransform.sizeDelta *= scale;
        }
    }
}
