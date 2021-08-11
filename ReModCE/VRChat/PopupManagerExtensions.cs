using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.VRChat
{
    internal static class PopupManagerExtensions
    {
        public static void ShowInputPopupWithCancel(this VRCUiPopupManager vrcUiPopupManager, string title, string preFilledText,
            InputField.InputType inputType, bool useNumericKeypad, string submitButtonText,
            Il2CppSystem.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text> submitButtonAction,
            Il2CppSystem.Action cancelButtonAction, string placeholderText = "Enter text....", bool hidePopupOnSubmit = true,
            Action<VRCUiPopup> additionalSetup = null)
        {
            vrcUiPopupManager.Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0(
                    title,
                    preFilledText,
                    inputType, useNumericKeypad, submitButtonText, submitButtonAction, cancelButtonAction, placeholderText, hidePopupOnSubmit, additionalSetup);
        }
    }
}
