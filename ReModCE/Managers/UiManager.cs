using ReModCE.UI;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.Managers
{
    internal class UiManager
    {
        private const float ButtonSize = 420f;
        private readonly ReQuickMenu _mainMenu;
        private readonly ReQuickMenu _targetMenu;

        public ReQuickMenu MainMenu => _mainMenu;
        public ReQuickMenu TargetMenu => _targetMenu;

        public UiManager(string menuName)
        {
            GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/UserIconCameraButton").transform.localPosition += new Vector3(ButtonSize, -ButtonSize, 0f);
            var intialButtonPos = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/ReportWorldButton").GetComponent<RectTransform>().localPosition;

            var shortcutMenu = QuickMenu.prop_QuickMenu_0.transform.Find("ShortcutMenu");
            var userInteractMenu = QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu");

            _mainMenu = new ReQuickMenu(menuName);
            ReQuickButton.Create(new Vector2(intialButtonPos.x, intialButtonPos.y + (ButtonSize * 2f)),
                "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
                () => _mainMenu.Open(),
                shortcutMenu);

            _mainMenu.AddSubMenu("Movement", "Access movement related options");

            _targetMenu = new ReQuickMenu("TargetReModCE", "UserInteractMenu", QuickMenuContext.UserSelected);
            ReQuickButton.Create(new Vector2(intialButtonPos.x, intialButtonPos.y - (ButtonSize * 2f)),
                "Target Options", "More options for this target",
                () => _targetMenu.Open(QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique.UserSelected),
                userInteractMenu);
        }
    }
}
