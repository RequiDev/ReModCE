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
using Object = UnityEngine.Object;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.Managers
{
    internal class UiManager
    {
        public const float ButtonSize = 420f;
        
        public ReMenuPage MainMenu { get; }
        public ReMenuPage TargetMenu { get; }

        public bool IsRemodLoaded { get; }
        public bool IsRubyLoaded { get; }
        public bool IsEmmVRCLoaded { get; }

        public UiManager(string menuName)
        {
            IsRemodLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "ReMod");
            IsEmmVRCLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");

            var dashboard = ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Menu_Dashboard").GetComponent<UIPage>();
            var dashboardScrollrect = dashboard.GetComponentInChildren<ScrollRect>();
            var dashboardScrollbar = dashboardScrollrect.transform.Find("Scrollbar").GetComponent<Scrollbar>();

            var dashboardContent = dashboardScrollrect.content;
            dashboardContent.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            dashboardContent.Find("Carousel_Banners").gameObject.SetActive(false);

            dashboardScrollrect.enabled = true;
            dashboardScrollrect.verticalScrollbar = dashboardScrollbar;
            dashboardScrollrect.viewport.GetComponent<RectMask2D>().enabled = true;

            var category = new ReMenuCategory("ReModCE", menuName);
            MainMenu = category.AddMenuPage("ReModCE", menuName, $"Open {menuName}");

            var wingMenu = new ReWingMenu("ReModCE", menuName, false);
            var wingButton = new ReWingButton("ReModCE", menuName, "", wingMenu.Open, false);

            MainMenu.AddMenuPage("Movement", "Movement", "Access movement related settings");
            MainMenu.AddMenuPage("Visuals", "Visuals", "Access anything that will affect your game visually");
            MainMenu.AddMenuPage("DynamicBones", "Dynamic Bones", "Access your global dynamic bone settings");
            MainMenu.AddMenuPage("Hotkeys", "Hotkeys", "Access hotkey related settings");
            MainMenu.AddMenuPage("Avatars", "Avatars", "Access avatar related settings");
            MainMenu.AddMenuPage("Logging", "Logging", "Access logging related settings");

            TargetMenu = new ReMenuPage("TargetMenu", "Target Menu");
            var targetMenuButton = new ReMenuButton("TargetMenu", "ReMod <color=#00ff00>CE</color> Target Options",
                "More options for this target", TargetMenu.Open,
                ExtendedQuickMenu.Instance._selectedUserMenuLocal.GetComponentInChildren<ScrollRect>().content.Find("Buttons_UserActions"));
        }
    }
}
