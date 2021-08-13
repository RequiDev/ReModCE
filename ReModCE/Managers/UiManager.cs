using System;
using System.Linq;
using MelonLoader;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.Managers
{
    internal class UiManager
    {
        public const float ButtonSize = 420f;
        private readonly ReQuickMenu _mainMenu;
        private readonly ReQuickMenu _targetMenu;

        public ReQuickMenu MainMenu => _mainMenu;
        public ReQuickMenu TargetMenu => _targetMenu;

        public bool IsRemodLoaded { get; }

        public UiManager(string menuName)
        {
            IsRemodLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "ReMod");

            if (!IsRemodLoaded)
            {
                ExtendedQuickMenu.ShortcutMenu.Find("UserIconCameraButton").localPosition +=
                    new Vector3(ButtonSize, -ButtonSize, 0f);
            }

            var intialButtonPos = ExtendedQuickMenu.ReportWorldButton.GetComponent<RectTransform>().localPosition;
            
            _mainMenu = new ReQuickMenu(menuName);
            ReQuickButton.Create(new Vector2(intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), intialButtonPos.y + (ButtonSize * 2f)),
                "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
                () => _mainMenu.Open(),
                ExtendedQuickMenu.ShortcutMenu);

            _mainMenu.AddSubMenu("Movement", "Access movement related options");

            _targetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            ReQuickButton.Create(new Vector2(intialButtonPos.x + (Convert.ToInt32(IsRemodLoaded) * ButtonSize), intialButtonPos.y - (ButtonSize * 2f)),
                "ReMod <color=#00ff00>CE</color> Target Options", "More options for this target",
                () => _targetMenu.Open(QuickMenuContext.UserSelected),
                ExtendedQuickMenu.UserInteractMenu.transform);
        }
    }
}
