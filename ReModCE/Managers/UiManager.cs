using System.IO;
using System.Linq;
using MelonLoader;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

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

            FixLaunchpadScrolling();


            MainMenu = new ReMenuPage("ReModCE", menuName, true);
            var mainTab = new ReTabButton("ReModCE", $"Open {menuName}", "ReModCE", ResourceManager.GetSprite("remod"));

            var categoryPage = MainMenu.AddCategoryPage("MediaMenu", "Media", "Media Menu");
            var videoCat = categoryPage.AddCategory("VideoOptions", "Video Options");

            videoCat.AddButton("PlayVideo", "Play\nVideo", "Play Video on Videoplayer", () => { });

            var wingMenu = new ReWingMenu("ReModCE", menuName, false);
            var wingButton = new ReWingButton("ReModCE", menuName, "", wingMenu.Open, ResourceManager.GetSprite("remod"), false);

            MainMenu.AddMenuPage("Movement", "Movement", "Access movement related settings", ResourceManager.GetSprite("running"));
            MainMenu.AddMenuPage("Visuals", "Visuals", "Access anything that will affect your game visually");
            MainMenu.AddMenuPage("DynamicBones", "Dynamic Bones", "Access your global dynamic bone settings", ResourceManager.GetSprite("bone"));
            MainMenu.AddMenuPage("Hotkeys", "Hotkeys", "Access hotkey related settings");
            MainMenu.AddMenuPage("Avatars", "Avatars", "Access avatar related settings");
            MainMenu.AddMenuPage("Logging", "Logging", "Access logging related settings");

            TargetMenu = new ReMenuPage("TargetMenu", "Target Menu");
            var targetMenuButton = new ReMenuButton("TargetMenu", "Target Options",
                "More options for this target", TargetMenu.Open,
                ExtendedQuickMenu.Instance._selectedUserMenuLocal.GetComponentInChildren<ScrollRect>().content.Find("Buttons_UserActions"),
                ResourceManager.GetSprite("remod"));
        }

        private void FixLaunchpadScrolling()
        {
            var dashboard = ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Menu_Dashboard").GetComponent<UIPage>();
            var scrollRect = dashboard.GetComponentInChildren<ScrollRect>();
            var dashboardScrollbar = scrollRect.transform.Find("Scrollbar").GetComponent<Scrollbar>();

            var dashboardContent = scrollRect.content;
            dashboardContent.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            dashboardContent.Find("Carousel_Banners").gameObject.SetActive(false);

            scrollRect.enabled = true;
            scrollRect.verticalScrollbar = dashboardScrollbar;
            scrollRect.viewport.GetComponent<RectMask2D>().enabled = true;
        }
    }
}
