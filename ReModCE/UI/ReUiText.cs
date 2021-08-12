using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReUiText : UIElement
    {
        private readonly Text _textComponent;

        public string Text
        {
            get => _textComponent.text;
            set => _textComponent.text = value;
        }
        public ReUiText(string text, Vector2 pos, Vector2 scale, Transform parent = null) : base(
            GameObject.Find("/UserInterface/MenuContent/Screens/Avatar/Favorite Button"), parent, pos,
            $"{text}UiButton")
        {

            Object.DestroyImmediate(GameObject.GetComponentInChildren<Button>());
            Object.DestroyImmediate(GameObject.GetComponentInChildren<Image>());

            _textComponent = GameObject.GetComponentInChildren<Text>();
            _textComponent.text = text;

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
