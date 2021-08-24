using System;
using System.IO;
using System.Linq;
using MelonLoader;
using ReModCE.Core;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.Managers
{
    internal class UiManager
    {
        public const float ButtonSize = 420f;
        private Vector3 _intialButtonPos;

        private ReQuickButton _mainMenuButton;
        public ReQuickMenu MainMenu { get; }
        public ReQuickMenu TargetMenu { get; }

        public bool IsRemodLoaded { get; }
        public bool IsRubyLoaded { get; }

        private ConfigValue<float> ButtonOffsetX;
        private ConfigValue<float> ButtonOffsetY;

        public UiManager(string menuName)
        {
            ButtonOffsetX = new ConfigValue<float>(nameof(ButtonOffsetX), 0f, "Main Button Offset X", "Offset on the X axis for the main menu button. Relative to \"Report World\" button. 1 = 1 button size.");
            ButtonOffsetX.OnValueChanged += OnButtonOffsetChanged;
            ButtonOffsetY = new ConfigValue<float>(nameof(ButtonOffsetY), 2f, "Main Button Offset Y", "Offset on the Y axis for the main menu button. Relative to \"Report World\" button. 1 = 1 button size.");
            ButtonOffsetY.OnValueChanged += OnButtonOffsetChanged;

            var buttonOffset = new Vector3(ButtonSize * ButtonOffsetX, ButtonSize * ButtonOffsetY);

            IsRemodLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "ReMod");
            IsRubyLoaded = File.Exists("hid.dll");

            var isDefaultButtonPos = buttonOffset == new Vector3(0, ButtonSize * 2f);
            if (IsRemodLoaded && isDefaultButtonPos)
            {
                buttonOffset.x = ButtonSize;
            }

            if (!IsRemodLoaded && isDefaultButtonPos)
            {
                ExtendedQuickMenu.UserIconCameraButton.localPosition +=
                    new Vector3(ButtonSize, -ButtonSize, 0f);
            }

            _intialButtonPos = ExtendedQuickMenu.ReportWorldButton.GetComponent<RectTransform>().localPosition;
            
            MainMenu = new ReQuickMenu(menuName);
            _mainMenuButton = new ReQuickButton(_intialButtonPos + buttonOffset,
                "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
                () => MainMenu.Open(),
                ExtendedQuickMenu.ShortcutMenu);

            MainMenu.AddSubMenu("Movement", "Access movement related settings");
            MainMenu.AddSubMenu("Dynamic Bones", "Access your global dynamic bone settings");
            MainMenu.AddSubMenu("Hotkeys", "Access hotkey related settings");
            MainMenu.AddSubMenu("Avatars", "Access avatar related settings");

            TargetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            ReQuickButton.Create(new Vector2(_intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), _intialButtonPos.y - (ButtonSize * 2f)),
                "ReMod <color=#00ff00>CE</color> Target Options", "More options for this target",
                () => TargetMenu.Open(QuickMenuContext.UserSelected),
                ExtendedQuickMenu.UserInteractMenu.transform);
        }

        private void OnButtonOffsetChanged()
        {
            var buttonOffset = new Vector3(ButtonSize * ButtonOffsetX, ButtonSize * ButtonOffsetY);
            var isDefaultButtonPos = buttonOffset == new Vector3(0, ButtonSize * 2f);
            if (IsRemodLoaded && isDefaultButtonPos)
            {
                buttonOffset.x = ButtonSize;
            }
            _mainMenuButton.Position = _intialButtonPos + buttonOffset;

        }
    }
}
