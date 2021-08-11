using System;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.Core;

namespace ReModCE.VRChat
{
    internal static class VrcUiExtensions
    {
        public static void StartRenderElementsCoroutine(this UiVRCList instance, List<ApiAvatar> avaterList, int offset = 0, bool endOfPickers = true, VRCUiContentButton contentHeaderElement = null)
        {
            if (!instance.gameObject.activeInHierarchy || !instance.isActiveAndEnabled || instance.isOffScreen ||
                !instance.enabled)
                return;

            instance.Method_Protected_Void_List_1_T_Int32_Boolean_VRCUiContentButton_0(avaterList, offset, endOfPickers, contentHeaderElement);
        }
    }
}
