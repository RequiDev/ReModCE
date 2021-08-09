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
    internal class ReScrollView : UIElement
    {
        public Text _logText;

        public ReScrollView(string name, Vector2 pos, Transform parent) : base(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/ReportWorldButton"), parent, pos, $"ScrollView_{name}")
        {
            Object.DestroyImmediate(gameObject.GetComponentsInChildren<Image>(true).First(a => a.transform != gameObject.transform));
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());

            var image = gameObject.GetComponent<Image>();

            var button = gameObject.GetComponent<Button>();
            var originalColors = button.colors;
            button.colors = new ColorBlock
            {
                colorMultiplier = originalColors.colorMultiplier,
                disabledColor = originalColors.normalColor,
                fadeDuration = originalColors.fadeDuration,
                highlightedColor = originalColors.normalColor,
                pressedColor = originalColors.normalColor,
                selectedColor = originalColors.normalColor
            };
            button.interactable = false;

            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1680);
            rect.ForceUpdateRectTransforms();

            _logText = gameObject.GetComponentInChildren<Text>();
            _logText.fontSize = (int)(_logText.fontSize * 0.75f);
            _logText.transform.localPosition += new Vector3(10f, -100f);
            _logText.alignment = TextAnchor.LowerLeft;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;
            _logText.SetClipRect(new Rect(new Vector2(0, -220), new Vector2(3000, 1500)), true);
            _logText.text = "";
        }

        public void AddText(string message)
        {
            _logText.text += message;
        }
    }
}
