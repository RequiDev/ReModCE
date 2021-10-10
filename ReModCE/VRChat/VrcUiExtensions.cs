using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI.Elements;

namespace ReModCE.VRChat
{
    internal static class VrcUiExtensions
    {
        public static void StartRenderElementsCoroutine(this UiVRCList instance, List<ApiAvatar> avaterList, int offset = 0, bool endOfPickers = true, VRCUiContentButton contentHeaderElement = null)
        {
            if (!instance.gameObject.activeInHierarchy || !instance.isActiveAndEnabled || instance.isOffScreen ||
                !instance.enabled)
                return;

            if (instance.scrollRect != null)
            {
                instance.scrollRect.normalizedPosition = new Vector2(0f, 0f);
            }
            instance.Method_Protected_Void_List_1_T_Int32_Boolean_VRCUiContentButton_0(avaterList, offset, endOfPickers, contentHeaderElement);
        }

        public static Transform GetContent(this UIPage page)
        {
            var scrollrectTransform = page.transform.Find("ScrollRect");
            if (scrollrectTransform == null)
                scrollrectTransform = page.transform.Find("Scrollrect");
            return scrollrectTransform.GetComponent<ScrollRect>().content;
        }
    }
}
