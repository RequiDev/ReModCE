using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReScrollView : UIElement
    {
        private readonly Text _logText;

        public ReScrollView(string name, Vector2 pos, Transform parent) : base(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/ReportWorldButton"), parent, pos, $"ScrollView_{name}")
        {
            Object.DestroyImmediate(gameObject.GetComponentsInChildren<Image>(true).First(a => a.transform != gameObject.transform));
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(gameObject.GetComponent<ButtonReaction>());

            gameObject.AddComponent<RectMask2D>();

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
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1260F);
            rect.ForceUpdateRectTransforms();

            _logText = gameObject.GetComponentInChildren<Text>();

            var viewport = new GameObject("Viewport", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[1] { Il2CppType.Of<RectTransform>() }));
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.SetParent(rect);

            var scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.content = _logText.GetComponent<RectTransform>();
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.horizontal = false;
            scrollRect.decelerationRate = 0.03f;
            scrollRect.scrollSensitivity = 3;

            //_logText.GetComponent<RectTransform>().SetParent(viewportRect);

            _logText.fontSize = (int)(_logText.fontSize * 0.75f);
            _logText.transform.localPosition += new Vector3(0f, -140f);
            _logText.alignment = TextAnchor.LowerLeft;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;
            _logText.text = "";

        }

        public void AddText(string message)
        {
            _logText.text += message;
        }
    }
}
