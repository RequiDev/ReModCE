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
        
        public ReQuickMenu MainMenu { get; }
        public ReQuickMenu TargetMenu { get; }

        public bool IsRemodLoaded { get; }
        public bool IsRubyLoaded { get; }
        public bool IsEmmVRCLoaded { get; }

        public UiManager(string menuName)
        {
            var buttonOffset = new Vector3(ButtonSize * 0f, ButtonSize * 2f);

            IsRemodLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "ReMod");
            IsEmmVRCLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");

            if (IsRemodLoaded)
            {
                buttonOffset.x = ButtonSize;
            }
            else
            {
                ExtendedQuickMenu.UserIconCameraButton.localPosition +=
                    new Vector3(ButtonSize, -ButtonSize, 0f);
            }

            var intialButtonPos = ExtendedQuickMenu.ReportWorldButton.GetComponent<RectTransform>().localPosition;
            
            MainMenu = new ReQuickMenu(menuName);
            ReQuickButton.Create(intialButtonPos + buttonOffset,
                "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
                () => MainMenu.Open(),
                ExtendedQuickMenu.ShortcutMenu);

            MainMenu.AddSubMenu("Movement", "Access movement related settings");
            MainMenu.AddSubMenu("Visuals", "Access anything that will affect your game visually");
            MainMenu.AddSubMenu("Dynamic Bones", "Access your global dynamic bone settings");
            MainMenu.AddSubMenu("Hotkeys", "Access hotkey related settings");
            MainMenu.AddSubMenu("Avatars", "Access avatar related settings");

            TargetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            ReQuickButton.Create(new Vector2(intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), intialButtonPos.y - (ButtonSize * 2f)),
                "ReMod <color=#00ff00>CE</color> Target Options", "More options for this target",
                () => TargetMenu.Open(QuickMenuContext.UserSelected),
                ExtendedQuickMenu.UserInteractMenu.transform);
        }
    }
}
