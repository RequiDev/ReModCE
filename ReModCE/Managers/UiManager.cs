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
        
        public ReMenuPage MainMenu { get; }
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


            var category = new ReMenuCategory("ReModCE", menuName);
            MainMenu = category.AddSubMenu("ReModCE", menuName, $"Open {menuName}");
            ReWingButton.Create(menuName, MainMenu.Open, ReWingButton.WingSide.Both, false);

            MainMenu.AddSubMenu("Movement", "Movement", "Access movement related settings");
            MainMenu.AddSubMenu("Visuals", "Visuals", "Access anything that will affect your game visually");
            MainMenu.AddSubMenu("DynamicBones", "Dynamic\nBones", "Access your global dynamic bone settings");
            MainMenu.AddSubMenu("Hotkeys", "Hotkeys", "Access hotkey related settings");
            MainMenu.AddSubMenu("Avatars", "Avatars", "Access avatar related settings");
            MainMenu.AddSubMenu("Logging", "Logging", "Access logging related settings");
            // 
            // TargetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            // ReMenuButton.Create(new Vector2(intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), intialButtonPos.y - (ButtonSize * 2f)),
            //     "ReMod <color=#00ff00>CE</color> Target Options", "More options for this target",
            //     () => TargetMenu.Open(QuickMenuContext.UserSelected),
            //     ExtendedQuickMenu.UserInteractMenu.transform);
        }
    }
}
