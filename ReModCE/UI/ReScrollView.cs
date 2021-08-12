using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.VRChat;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReScrollView : UIElement
    {
        private readonly Text _logText;

        public ReScrollView(string name, Vector2 pos, Transform parent) : base(ExtendedQuickMenu.ReportWorldButton.gameObject, parent, pos, $"ScrollView{name}")
        {
            Object.DestroyImmediate(GameObject.GetComponentsInChildren<Image>(true).First(a => a.transform != GameObject.transform));
            Object.DestroyImmediate(GameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(GameObject.GetComponent<ButtonReaction>());

            GameObject.AddComponent<RectMask2D>();

            var button = GameObject.GetComponent<Button>();
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

             // RectTransform = Scroll View
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1260F);
            RectTransform.ForceUpdateRectTransforms();

            var content = new GameObject("Content", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[1] { Il2CppType.Of<RectTransform>() }));
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.SetParent(RectTransform);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5040F); // make it based on newlines in text?
            contentRect.ForceUpdateRectTransforms();
            contentRect.localRotation = new Quaternion(0f, 0f, 0f, 0f);
            contentRect.localScale = Vector3.one;

            var scrollRect = GameObject.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.horizontal = false;
            scrollRect.decelerationRate = 0.03f;
            scrollRect.scrollSensitivity = 3;

            _logText = GameObject.GetComponentInChildren<Text>();
            var textRect = _logText.GetComponent<RectTransform>();
            textRect.SetParent(contentRect);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5040F); // make it based on newlines in text?
            textRect.ForceUpdateRectTransforms();
            textRect.localPosition = Vector3.zero;
            textRect.localRotation = new Quaternion(0f, 0f, 0f, 0f);
            textRect.localScale = Vector3.one;

            contentRect.localPosition = new Vector3(50f, 1260F + 1260F / 2, 0f);

            _logText.fontSize = (int)(_logText.fontSize * 0.75f);
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
