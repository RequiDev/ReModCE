using System.Linq;
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
        private readonly RectTransform _textTransform;
        private readonly RectTransform _contentTransform;

        public ReScrollView(string name, Vector2 pos, Transform parent) : base(ExtendedQuickMenu.QuickButtonPrefab.gameObject, parent, pos, $"ScrollView{name}")
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
            _contentTransform = content.GetComponent<RectTransform>();
            _contentTransform.SetParent(RectTransform);
            _contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            _contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5040F);
            _contentTransform.ForceUpdateRectTransforms();
            _contentTransform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
            _contentTransform.localScale = Vector3.one;

            var scrollRect = GameObject.AddComponent<ScrollRect>();
            scrollRect.content = _contentTransform;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.horizontal = false;
            scrollRect.decelerationRate = 0.03f;
            scrollRect.scrollSensitivity = 3;

            _logText = GameObject.GetComponentInChildren<Text>();
            _textTransform = _logText.GetComponent<RectTransform>();
            _textTransform.SetParent(_contentTransform);
            _textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1680f);
            _textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5040F);
            _textTransform.ForceUpdateRectTransforms();
            _textTransform.localPosition = Vector3.zero;
            _textTransform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
            _textTransform.localScale = Vector3.one;

            _contentTransform.localPosition = new Vector3(50f, 1260F + (1260F / 2), 0f);

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
