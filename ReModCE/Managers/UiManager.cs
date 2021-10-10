using System;
using System.IO;
using System.Linq;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.Managers
{
    internal class UiManager
    {
        public const float ButtonSize = 420f;
        
        public ReMenuCategory MainMenu { get; }
        public ReMenuPage TargetMenuPage { get; }

        public bool IsRemodLoaded { get; }
        public bool IsRubyLoaded { get; }
        public bool IsEmmVRCLoaded { get; }

        public UiManager(string menuName)
        {
            IsRemodLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "ReMod");
            IsEmmVRCLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");
            
            var dashboard = ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Menu_Dashboard").GetComponent<UIPage>();
            dashboard.GetComponentInChildren<ScrollRect>().content.Find("Carousel_Banners").gameObject.SetActive(false);

            MainMenu = new ReMenuCategory("ReModCE", menuName);
            var menu = MainMenu.AddSubMenu("ReModCE", menuName);
            for (var i = 0; i <= 32; ++i)
            {
                menu.AddButton($"Button{i}", $"Button {i}", "", () => { });
            }

            MainMenu.AddToggle("Test", "Toggle Test", "Wheeee");

            ReWingButton.Create(menuName, menu.Open, ReWingButton.WingSide.Both, false);
            return;

            // MainMenu = new ReQuickMenu(menuName);
            // ReMenuButton.Create(intialButtonPos + buttonOffset,
            //     "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
            //     () => MainMenu.Open(),
            //     ExtendedQuickMenu.ShortcutMenu);
            // 
            // MainMenu.AddSubMenu("Movement", "Access movement related settings");
            // MainMenu.AddSubMenu("Visuals", "Access anything that will affect your game visually");
            // MainMenu.AddSubMenu("Dynamic Bones", "Access your global dynamic bone settings");
            // MainMenu.AddSubMenu("Hotkeys", "Access hotkey related settings");
            // MainMenu.AddSubMenu("Avatars", "Access avatar related settings");
            // MainMenu.AddSubMenu("Logging", "Access logging related settings");
            // 
            // TargetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            // ReMenuButton.Create(new Vector2(intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), intialButtonPos.y - (ButtonSize * 2f)),
            //     "ReMod <color=#00ff00>CE</color> Target Options", "More options for this target",
            //     () => TargetMenu.Open(QuickMenuContext.UserSelected),
            //     ExtendedQuickMenu.UserInteractMenu.transform);
        }
    }
}
