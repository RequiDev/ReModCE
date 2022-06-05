using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Components
{

    internal class SocialMenuCompactor : ModComponent
    {
        private ReUiButton _vrchatMenuButton;

        private readonly Dictionary<string, bool> _buttonStates = new Dictionary<string, bool>();
        private readonly List<GameObject> _buttonsToControl = new List<GameObject>();

        public override void OnUiManagerInitEarly()
        {
            var buttonContainer = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/UserInfo/Buttons/RightSideButtons/RightUpperButtonColumn/");

            _buttonsToControl.Add(buttonContainer.Find("Supporter").gameObject);
            _buttonsToControl.Add(buttonContainer.Find("PlaylistsButton").gameObject);
            _buttonsToControl.Add(buttonContainer.Find("FavoriteButton").gameObject);
            _buttonsToControl.Add(buttonContainer.Find("GiftVRChatPlusButton").gameObject);

            _vrchatMenuButton = new ReUiButton("VRChat", Vector2.zero, new Vector2(0.68f, 1.2f), () =>
            {
                foreach (var button in _buttonsToControl)
                {
                    if (button.activeSelf)
                    {
                        button.SetActive(false);
                    }
                    else
                    {
                        if (_buttonStates[button.name])
                        {
                            button.SetActive(true);
                        }
                    }
                }
            }, buttonContainer);
            _vrchatMenuButton.RectTransform.SetAsFirstSibling();
        }

        public override void OnSetupUserInfo(APIUser apiUser)
        {
            MelonCoroutines.Start(CollectButtonStates());
        }

        private IEnumerator CollectButtonStates()
        {
            yield return null;

            foreach (var button in _buttonsToControl)
            {
                _buttonStates[button.name] = button.activeSelf;

                button.SetActive(false);
            }
        }
    }
}
