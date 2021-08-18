using System;
using System.Linq;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.VRChat
{
    internal static class PopupManagerExtensions
    {
        public delegate void ShowAlertDelegate(VRCUiPopupManager popupManager, string title, string body, float timeout);

        private static ShowAlertDelegate _showAlertDelegate;

        private static ShowAlertDelegate ShowAlertFn
        {
            get
            {
                if (_showAlertDelegate != null)
                    return _showAlertDelegate;

                var showAlertFn = typeof(VRCUiPopupManager).GetMethods().Single(m =>
                {
                    if (m.ReturnType != typeof(void))
                        return false;

                    if (m.GetParameters().Length != 3)
                        return false;

                    return XrefScanner.XrefScan(m).Any(x => x.Type == XrefType.Global && x.ReadAsObject()?.ToString() ==
                        "UserInterface/MenuContent/Popups/AlertPopup");
                });

                _showAlertDelegate = (ShowAlertDelegate)Delegate.CreateDelegate(typeof(ShowAlertDelegate), showAlertFn);

                return _showAlertDelegate;
            }
        }

        public static void ShowAlert(this VRCUiPopupManager popupManager, string title, string body, float timeout = 0f)
        {
            ShowAlertFn(popupManager, title, body, timeout);
        }

        public static void ShowInputPopupWithCancel(this VRCUiPopupManager popupManager, string title, string preFilledText,
            InputField.InputType inputType, bool useNumericKeypad, string submitButtonText,
            Il2CppSystem.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text> submitButtonAction,
            Il2CppSystem.Action cancelButtonAction, string placeholderText = "Enter text....", bool hidePopupOnSubmit = true,
            Action<VRCUiPopup> additionalSetup = null)
        {
            popupManager.Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0(
                    title,
                    preFilledText,
                    inputType, useNumericKeypad, submitButtonText, submitButtonAction, cancelButtonAction, placeholderText, hidePopupOnSubmit, additionalSetup);
        }
    }
}
