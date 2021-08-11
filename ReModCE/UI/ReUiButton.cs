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
    internal class ReUiButton : UIElement
    {
        public ReUiButton(string text, Vector2 pos, Vector2 scale, Action onClick, Transform parent = null) : base(
            GameObject.Find("/UserInterface/MenuContent/Screens/Avatar/Favorite Button"), parent, pos,
            $"{text}UiButton")
        {

            var buttonComponent = GameObject.GetComponentInChildren<Button>();
            buttonComponent.onClick = new Button.ButtonClickedEvent();
            buttonComponent.onClick.AddListener(new Action(onClick));

            var textComponent = GameObject.GetComponentInChildren<Text>();
            textComponent.text = text;

            var allTextComponents = GameObject.GetComponentsInChildren<Text>(true);
            foreach (var t in allTextComponents)
            {
                if (t.transform == textComponent.transform)
                    continue;

                Object.DestroyImmediate(t);
            }

            RectTransform.sizeDelta *= scale;
        }
    }
}
