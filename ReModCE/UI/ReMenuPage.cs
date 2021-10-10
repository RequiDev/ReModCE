using System;
using System.Collections.Generic;
using System.Linq;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.DataModel.Core;
using VRC.UI.Core;
using VRC.UI.Elements;
using VRC.UI.Elements.Menus;
using Object = UnityEngine.Object;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.UI
{
    internal class ReMenuPage : UIElement
    {
        private static GameObject _menuPrefab;

        private static GameObject MenuPrefab
        {
            get
            {
                if (_menuPrefab == null)
                {
                    _menuPrefab = ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Menu_DevTools").gameObject;
                }
                return _menuPrefab;
            }
        }

        private static int SiblingIndex => ExtendedQuickMenu.Instance.container.Find("Window/QMParent/Modal_AddMessage").GetSiblingIndex();

        private readonly List<ReMenuPage> _subMenus = new List<ReMenuPage>();

        public event Action OnOpen;
        private readonly string _menuName;
        private readonly bool _isRoot;

        private readonly Transform _container;

        public ReMenuPage(string name, string text, bool isRoot = false) : base(MenuPrefab, MenuPrefab.transform.parent, $"Menu_{name}", false)
        {
            Object.DestroyImmediate(GameObject.GetComponent<DevMenu>());

            RectTransform.SetSiblingIndex(SiblingIndex);

            _menuName = name;
            _isRoot = isRoot;
            var headerTransform = RectTransform.GetChild(0);
            var titleText = headerTransform.GetComponentInChildren<TextMeshProUGUI>();
            titleText.text = text;
            titleText.richText = true;

            var backButton = headerTransform.GetComponentInChildren<Button>(true);
            backButton.gameObject.SetActive(true);

            var buttonContainer = RectTransform.Find("Scrollrect/Viewport/VerticalLayoutGroup/Buttons");
            foreach (var obj in buttonContainer)
            {
                var control = obj.Cast<Transform>();
                if (control == null)
                {
                    continue;
                }
                Object.Destroy(control.gameObject);
            }

            // Set up UIPage
            var uiPage = GameObject.AddComponent<UIPage>();
            uiPage.Name = $"QuickMenu{name}";
            uiPage._inited = true;
            uiPage._menuStateController = ExtendedQuickMenu.MenuStateCtrl;
            uiPage._pageStack = new Il2CppSystem.Collections.Generic.List<UIPage>();
            uiPage._pageStack.Add(uiPage);

            // Get scroll stuff
            var scrollRect = RectTransform.Find("Scrollrect").GetComponent<ScrollRect>();
            _container = scrollRect.content;
            
            // copy properties of old grid layout
            var gridLayoutGroup = _container.Find("Buttons").GetComponent<GridLayoutGroup>();

            Object.DestroyImmediate(_container.GetComponent<VerticalLayoutGroup>());
            var glp = _container.gameObject.AddComponent<GridLayoutGroup>();
            glp.spacing = gridLayoutGroup.spacing;
            glp.cellSize = gridLayoutGroup.cellSize;
            glp.constraint = gridLayoutGroup.constraint;
            glp.constraintCount = gridLayoutGroup.constraintCount;
            glp.startAxis = gridLayoutGroup.startAxis;
            glp.startCorner = gridLayoutGroup.startCorner;
            glp.childAlignment = gridLayoutGroup.childAlignment;
            glp.padding = gridLayoutGroup.padding;
            glp.padding.top = 8;

            // delete components we're not using
            Object.DestroyImmediate(_container.Find("Buttons").gameObject);
            Object.DestroyImmediate(_container.Find("Spacer_8pt").gameObject);

            // Fix scrolling
            var scrollbar = scrollRect.transform.Find("Scrollbar");
            scrollbar.gameObject.SetActive(true);

            scrollRect.enabled = true;
            scrollRect.verticalScrollbar = scrollbar.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.viewport.GetComponent<RectMask2D>().enabled = true;

            ExtendedQuickMenu.MenuStateCtrl._uiPages.Add(uiPage.Name, uiPage);

            if (isRoot)
            {
                var rootPages = ExtendedQuickMenu.MenuStateCtrl.menuRootPages.ToList();
                rootPages.Add(uiPage);
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

        public ReMenuButton AddButton(string name, string text, string tooltip, Action onClick)
        {
            return new ReMenuButton(name, text, tooltip, onClick, _container);
        }

        public ReMenuToggle AddToggle(string name, string text, string tooltip, Action<bool> onToggle, bool defaultValue = false)
        {
            return new ReMenuToggle(name, text, tooltip, onToggle, _container, defaultValue);
            // var toggle = new ReQuickToggle(NextButtonPos, text, tooltip, onToggle, defaultValue, RectTransform);
            // return toggle;
        }
        
        public ReMenuPage AddSubMenu(string name, string text, string tooltip)
        {
            var menu = new ReMenuPage(name, text);
            AddButton(name, text, tooltip, menu.Open);
            _subMenus.Add(menu);
            return menu;
        }

        public ReMenuPage GetSubMenu(string name)
        {
            return _subMenus.FirstOrDefault(m => m.Name == $"Menu_{name}");
        }
    }
}
