using System;
using System.Collections.Generic;
using System.Linq;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using VRC.UI.Elements.Menus;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReCategoryPage : UIElement
    {
        private static GameObject _menuPrefab;

        private static GameObject MenuPrefab
        {
            get
            {
                if (_menuPrefab == null)
                {
                    _menuPrefab = ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Menu_Dashboard").gameObject;
                }
                return _menuPrefab;
            }
        }

        private static int SiblingIndex => ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Modal_AddMessage").GetSiblingIndex();

        private readonly List<ReMenuCategory> _categories = new List<ReMenuCategory>();

        public event Action OnOpen;
        private readonly string _menuName;
        private readonly bool _isRoot;

        private readonly Transform _container;

        public UIPage UiPage { get; }

        public ReCategoryPage(string name, string text, bool isRoot = false) : base(MenuPrefab, MenuPrefab.transform.parent, $"Menu_{name}", false)
        {
            Object.DestroyImmediate(GameObject.GetComponent<LaunchPadQMMenu>());

            RectTransform.SetSiblingIndex(SiblingIndex);

            _menuName = name;
            _isRoot = isRoot;
            var headerTransform = RectTransform.GetChild(0);
            Object.DestroyImmediate(headerTransform.Find("RightItemContainer/Button_QM_Expand").gameObject);

            var titleText = headerTransform.GetComponentInChildren<TextMeshProUGUI>();
            titleText.text = text;
            titleText.richText = true;


            if (!_isRoot)
            {
                var backButton = headerTransform.GetComponentInChildren<Button>(true);
                backButton.gameObject.SetActive(true);
            }

            _container = RectTransform.GetComponentInChildren<ScrollRect>().content;
            foreach (var obj in _container)
            {
                var control = obj.Cast<Transform>();
                if (control == null)
                {
                    continue;
                }
                Object.Destroy(control.gameObject);
            }

            // Set up UIPage
            UiPage = GameObject.AddComponent<UIPage>();
            UiPage.Name = $"QuickMenu{name}";
            UiPage._inited = true;
            UiPage._menuStateController = ExtendedQuickMenu.MenuStateCtrl;
            UiPage._pageStack = new Il2CppSystem.Collections.Generic.List<UIPage>();
            UiPage._pageStack.Add(UiPage);

            ExtendedQuickMenu.MenuStateCtrl._uiPages.Add(UiPage.Name, UiPage);

            if (isRoot)
            {
                var rootPages = ExtendedQuickMenu.MenuStateCtrl.menuRootPages.ToList();
                rootPages.Add(UiPage);
                ExtendedQuickMenu.MenuStateCtrl.menuRootPages = rootPages.ToArray();
            }
        }

        public void Open()
        {
            if (_isRoot)
            {
                ExtendedQuickMenu.MenuStateCtrl.SwitchToRootPage($"QuickMenu{_menuName}");
            }
            else
            {
                ExtendedQuickMenu.MenuStateCtrl.PushPage($"QuickMenu{_menuName}");
            }


            // Active = true;
            OnOpen?.Invoke();
        }

        public ReMenuCategory AddCategory(string name, string title)
        {
            var category = new ReMenuCategory(name, title, _container);
            _categories.Add(category);
            return category;
        }

        public ReMenuCategory GetCategory(string name)
        {
            return _categories.FirstOrDefault(c => c.Name == name);
        }
    }
}
