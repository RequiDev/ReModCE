using System;
using ReMod.Core;
using ReModCE.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.Components
{
    internal class PopupExComponent : ModComponent
    {
        public override void OnUiManagerInitEarly()
        {
            var inputPopup = GameObject.Find("UserInterface/MenuContent/Popups/InputPopup").transform;
            var pasteButton = Object.Instantiate(inputPopup.Find("ButtonRight").gameObject, inputPopup);
            pasteButton.name = "ButtonPaste";
            pasteButton.transform.localPosition += new Vector3(320f, 0f);
            pasteButton.GetComponentInChildren<Text>().text = "Paste";

            var inputField = inputPopup.GetComponentInChildren<InputField>();

            var button = pasteButton.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(() =>
            {
                inputField.text = GUIUtility.systemCopyBuffer;
            }));
        }
    }
}
