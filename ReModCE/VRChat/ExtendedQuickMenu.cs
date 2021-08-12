using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;
using QuickMenuPage = QuickMenu.EnumNPublicSealedvaUnShEmUsEmNoCaMo_nUnique;

namespace ReModCE.VRChat
{
    internal static class ExtendedQuickMenu
    {
        public static QuickMenu Instance => QuickMenu.prop_QuickMenu_0;
        private static Transform _shortcutMenu;
        private static Transform _userInteractMenu;
        private static Transform _cameraMenu;
        private static Transform _emojiMenu;
        private static Transform _newElements;
        private static Transform _infoBar;

        private static Transform _reportWorldButton;
        private static GameObject _blockButton;

        private static FieldInfo _fiCurrentPage;

        public static Transform ShortcutMenu
        {
            get
            {
                if (_shortcutMenu == null)
                {
                    _shortcutMenu = Instance.transform.Find("ShortcutMenu");
                }
                return _shortcutMenu;
            }
        }
        public static Transform UserInteractMenu
        {
            get
            {
                if (_userInteractMenu == null)
                {
                    _userInteractMenu = Instance.transform.Find("UserInteractMenu");
                }
                return _userInteractMenu;
            }
        }

        public static Transform CameraMenu
        {
            get
            {
                if (_cameraMenu == null)
                {
                    _cameraMenu = Instance.transform.Find("CameraMenu");
                }
                return _cameraMenu;
            }
        }

        public static Transform EmojiMenu
        {
            get
            {
                if (_emojiMenu == null)
                {
                    _emojiMenu = Instance.transform.Find("EmojiMenu");
                }
                return _emojiMenu;
            }
        }

        public static Transform NewElements
        {
            get
            {
                if (_newElements == null)
                {
                    _newElements = Instance.transform.Find("QuickMenu_NewElements");
                }
                return _newElements;
            }
        }
        
        public static Transform ReportWorldButton
        {
            get
            {
                if (_reportWorldButton == null)
                {
                    _reportWorldButton = ShortcutMenu.Find("ReportWorldButton");
                }
                return _reportWorldButton;
            }
        }

        public static GameObject BlockButton
        {
            get
            {
                if (_blockButton == null)
                {
                    _blockButton = GameObject.Find("UserInterface/QuickMenu/UserInteractMenu/BlockButton");
                }

                return _blockButton;
            }
        }

        public static Transform InfoBar
        {
            get
            {
                if (_infoBar == null)
                {
                    _infoBar = NewElements.Find("_InfoBar");
                }
                return _infoBar;
            }
        }


        private static void EnsureCurrentPageFieldInfo(QuickMenu quickMenu)
        {
            if (_fiCurrentPage != null) return;

            var shortcutMenu = ShortcutMenu.gameObject;

            var menuToFind = shortcutMenu;
            if (menuToFind == null || !menuToFind.activeInHierarchy)
            {
                menuToFind = UserInteractMenu.gameObject;
            }

            _fiCurrentPage = Il2CppType.Of<QuickMenu>().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(a => a.FieldType == Il2CppType.Of<GameObject>())
                .LastOrDefault(a => a.GetValue(quickMenu)?.Cast<GameObject>() == menuToFind);
        }

        private static GameObject CurrentPage
        {
            get
            {
                EnsureCurrentPageFieldInfo(Instance);
                return _fiCurrentPage?.GetValue(Instance)?.Cast<GameObject>();
            }
            set
            {
                EnsureCurrentPageFieldInfo(Instance);
                _fiCurrentPage?.SetValue(Instance, value);
            }
        }


        public static void SetCurrentPage(this QuickMenu quickMenu, string pageName,
            QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            var newPage = quickMenu.transform.Find(pageName);
            if (newPage == null)
                return;
            
            CurrentPage?.SetActive(false);

            InfoBar.gameObject.SetActive(pageName == "ShortcutMenu");
            quickMenu.field_Private_QuickMenuContextualDisplay_0.Method_Public_Void_EnumNPublicSealedvaUnNoToUs7vUsNoUnique_0(context);
            newPage.gameObject.SetActive(true);

            CurrentPage = newPage.gameObject;

            switch (pageName)
            {
                case "ShortcutMenu":
                    quickMenu.SetMenuIndex(QuickMenuPage.ShortcutMenu);
                    break;
                case "UserInteractMenu":
                    quickMenu.SetMenuIndex(QuickMenuPage.UserInteractMenu);
                    break;
                default:
                    quickMenu.SetMenuIndex(QuickMenuPage.Unknown);
                    ShortcutMenu.gameObject.SetActive(false);
                    UserInteractMenu.gameObject.SetActive(false);
                    break;
            }
        }

        public static void SetMenuIndex(this QuickMenu quickMenu, QuickMenuPage index)
        {
            quickMenu.field_Private_EnumNPublicSealedvaUnShEmUsEmNoCaMo_nUnique_0 = index;
        }
    }
}
