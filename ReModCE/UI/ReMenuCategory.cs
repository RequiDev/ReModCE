using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Loader;
using ReModCE.VRChat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReMenuHeader : UIElement
    {
        private static GameObject _headerPrefab;
        private static GameObject HeaderPrefab
        {
            get
            {
                if (_headerPrefab == null)
                {
                    _headerPrefab = ExtendedQuickMenu.Instance.container
                        .Find("Window/QMParent/Menu_Dashboard/ScrollRect").GetComponent<ScrollRect>().content
                        .Find("Header_QuickActions").gameObject;
                }
                return _headerPrefab;
            }
        }

        public ReMenuHeader(string name, string title, Transform parent) : base(HeaderPrefab, (parent == null ? HeaderPrefab.transform.parent : parent), $"Header_{name}")
        {
            var tmp = GameObject.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = title;
            tmp.richText = true;
        }
    }
    internal class ReMenuButtonContainer : UIElement
    {
        private static GameObject _containerPrefab;
        private static GameObject ContainerPrefab
        {
            get
            {
                if (_containerPrefab == null)
                {
                    _containerPrefab = ExtendedQuickMenu.Instance.container
                        .Find("Window/QMParent/Menu_Dashboard/ScrollRect").GetComponent<ScrollRect>().content
                        .Find("Buttons_QuickActions").gameObject;
                }
                return _containerPrefab;
            }
        }

        public ReMenuButtonContainer(string name, Transform parent = null) : base(ContainerPrefab, (parent == null ? ContainerPrefab.transform.parent : parent), $"Buttons_{name}")
        {
            foreach (var obj in RectTransform)
            {
                var control = obj.Cast<Transform>();
                if (control == null)
                {
                    continue;
                }
                Object.Destroy(control.gameObject);
            }
        }
    }

    internal class ReMenuCategory
    {
        public ReMenuHeader Header;
        private readonly ReMenuButtonContainer _buttonContainer;

        private readonly List<ReMenuPage> _menus = new List<ReMenuPage>();
        
        public ReMenuCategory(string name, string title, Transform parent = null)
        {
            Header = new ReMenuHeader(name, title, parent);
            _buttonContainer = new ReMenuButtonContainer(name, parent);
        }

        public ReMenuButton AddButton(string name, string text, string tooltip, Action onClick)
        {
            var button = new ReMenuButton(name, text, tooltip, onClick, _buttonContainer.RectTransform);
            return button;
        }

        public ReMenuToggle AddToggle(string name, string text, string tooltip)
        {
            var toggle = new ReMenuToggle(name, text, tooltip, (b) => {ReLogger.Msg($"Toggle is {b}");},_buttonContainer.RectTransform);
            return toggle;
        }

        public ReMenuPage AddSubMenu(string name, string text, string tooltip = "")
        {
            var menu = new ReMenuPage(name, text);
            AddButton(name, text, string.IsNullOrEmpty(tooltip) ? $"Open the {text} menu" : tooltip, menu.Open);
            _menus.Add(menu);
            return menu;
        }

        public ReMenuPage GetSubMenu(string name)
        {
            return _menus.FirstOrDefault(m => m.Name == $"Menu_{name}");
        }
    }
}
