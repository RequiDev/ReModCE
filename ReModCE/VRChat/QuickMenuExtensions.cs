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
    internal static class QuickMenuExtensions
    {
        private static FieldInfo _fiCurrentPage;

        private static FieldInfo GetCurrentPageField(QuickMenu quickMenu)
        {
            if (_fiCurrentPage != null) return _fiCurrentPage;

            var shortcutMenu = quickMenu.transform.Find("ShortcutMenu").gameObject;

            var objectToFind = shortcutMenu;
            if (objectToFind == null || !objectToFind.activeInHierarchy)
            {
                objectToFind = quickMenu.transform.Find("UserInteractMenu").gameObject;
            }

            var array = (from fi in Il2CppType.Of<QuickMenu>().GetFields((BindingFlags)36)
                where fi.FieldType == Il2CppType.Of<GameObject>()
                select fi).ToArray();
            var num = 0;
            foreach (var fieldInfo in array)
            {
                var value = fieldInfo.GetValue(quickMenu);
                if (value?.TryCast<GameObject>() != objectToFind || ++num != 2) continue;
                _fiCurrentPage = fieldInfo;
                break;
            }

            return _fiCurrentPage;
        }

        public static void ShowPage(this QuickMenu quickMenu, string pageName,
            QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            var newPage = quickMenu.transform.Find(pageName);
            if (newPage == null)
                return;

            var currentPageField = GetCurrentPageField(quickMenu);
            var currentPage = currentPageField.GetValue(quickMenu);
            currentPage?.Cast<GameObject>().SetActive(false);

            // This is very ghetto, but it works and didn't break in a few months (after the reflection broke)
            quickMenu.field_Private_GameObject_26 = newPage.gameObject;

            quickMenu.transform.Find("QuickMenu_NewElements/_InfoBar").gameObject.SetActive(pageName == "ShortcutMenu");
            quickMenu.field_Private_QuickMenuContextualDisplay_0.Method_Public_Void_EnumNPublicSealedvaUnNoToUs7vUsNoUnique_0(context);
            newPage.gameObject.SetActive(true);
            currentPageField.SetValue(quickMenu, newPage.gameObject);
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
