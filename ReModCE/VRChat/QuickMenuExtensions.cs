using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Reflection;
using ReModCE.Loader;
using UnhollowerRuntimeLib;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;
using QuickMenuPage = QuickMenu.EnumNPublicSealedvaUnShEmUsEmNoCaMo_nUnique;

namespace ReModCE.VRChat
{
    internal static class QuickMenuExtensions
    {
        private static FieldInfo _fiCurrentPage;

        private static GameObject GetCurrentPage(this QuickMenu quickMenu)
        {
            if (_fiCurrentPage == null)
            {
                var shortcutMenu = quickMenu.transform.Find("ShortcutMenu").gameObject;

                var objectToFind = shortcutMenu;
                if (objectToFind == null || !objectToFind.activeInHierarchy)
                {
                    objectToFind = quickMenu.transform.Find("UserInteractMenu").gameObject;
                }

                _fiCurrentPage = Il2CppType.Of<QuickMenu>().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(a => a.FieldType == Il2CppType.Of<GameObject>()).LastOrDefault(a =>
                    {
                        var obj = a.GetValue(quickMenu);
                        if (obj == null)
                            return false;
                        var gameObject = obj.Cast<GameObject>();

                        return gameObject == objectToFind;
                    });

                if (_fiCurrentPage == null)
                    return null;
            }

            return _fiCurrentPage.GetValue(quickMenu) as GameObject;
        }

        private static void SetCurrentPage(this QuickMenu quickMenu, GameObject value)
        {
            if (_fiCurrentPage == null)
            {
                var shortcutMenu = quickMenu.transform.Find("ShortcutMenu").gameObject;

                var objectToFind = shortcutMenu;
                if (objectToFind == null || !objectToFind.activeInHierarchy)
                {
                    objectToFind = quickMenu.transform.Find("UserInteractMenu").gameObject;
                }

                _fiCurrentPage = Il2CppType.Of<QuickMenu>().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(a => a.FieldType == Il2CppType.Of<GameObject>()).LastOrDefault(a =>
                    {
                        var obj = a.GetValue(quickMenu);
                        if (obj == null)
                            return false;
                        var gameObject = obj.Cast<GameObject>();

                        return gameObject == objectToFind;
                    });

                if (_fiCurrentPage == null)
                    return;
            }

            _fiCurrentPage.SetValue(quickMenu, value);
        }

        public static void SetCurrentPage(this QuickMenu quickMenu, string pageName,
            QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            var newPage = quickMenu.transform.Find(pageName);
            if (newPage == null)
                return;

            var currentPage = quickMenu.GetCurrentPage();
            currentPage?.Cast<GameObject>().SetActive(false);

            quickMenu.transform.Find("QuickMenu_NewElements/_InfoBar").gameObject.SetActive(pageName == "ShortcutMenu");
            quickMenu.field_Private_QuickMenuContextualDisplay_0.Method_Public_Void_EnumNPublicSealedvaUnNoToUs7vUsNoUnique_0(context);
            newPage.gameObject.SetActive(true);

            quickMenu.SetCurrentPage(newPage.gameObject);

            switch (pageName)
            {
                case "ShortMenu":
                    quickMenu.SetMenuIndex(QuickMenuPage.ShortcutMenu);
                    break;
                case "UserInteractMenu":
                    quickMenu.SetMenuIndex(QuickMenuPage.UserInteractMenu);
                    break;
                default:
                    quickMenu.SetMenuIndex(QuickMenuPage.Unknown);
                    quickMenu.transform.Find("ShortcutMenu").gameObject.SetActive(false);
                    quickMenu.transform.Find("UserInteractMenu").gameObject.SetActive(false);
                    break;
            }
        }
        
        public static void SetMenuIndex(this QuickMenu quickMenu, QuickMenuPage index)
        {
            quickMenu.field_Private_EnumNPublicSealedvaUnShEmUsEmNoCaMo_nUnique_0 = index;
        }
    }
}
