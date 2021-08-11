using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Loader;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.UI
{
    internal class ReQuickMenu : UIElement
    {
        private const int ButtonSize = 420;
        private const int MaxFullButtons = 12;

        private readonly string _name;
        private ReQuickMenu _nextPageMenu;
        private int _buttonsAdded;

        public event Action OnOpen;
        public List<ReQuickMenu> _subMenus = new List<ReQuickMenu>();

        public ReQuickMenu(string name, string parent = "ShortcutMenu", QuickMenuContext backButtonContext = QuickMenuContext.NoSelection) : base(GameObject.Find("UserInterface/QuickMenu/CameraMenu"), QuickMenu.prop_QuickMenu_0.transform, name, false)
        {
            _name = name;
            foreach (var obj in transform)
            {
                var control = obj.Cast<Transform>();
                if (control == null)
                {
                    continue;
                }

                if (control.name == "BackButton")
                {
                    var button = control.GetComponent<Button>();
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(new Action(() =>
                    {
                        QuickMenu.prop_QuickMenu_0.SetCurrentPage(parent, backButtonContext);
                    }));
                    button.transform.localPosition += new Vector3(420f, 0f);
                    continue;
                }
                
                Object.Destroy(control.gameObject);
            }
        }

        public void Open(QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            QuickMenu.prop_QuickMenu_0.SetCurrentPage(gameObject.name, context);
            OnOpen?.Invoke();
        }

        public ReQuickButton AddButton(string text, string tooltip, Action onClick)
        {
            if (_nextPageMenu != null)
            {
                return _nextPageMenu.AddButton(text, tooltip, onClick);
            }

            if ((_buttonsAdded + 1) == MaxFullButtons && text != "Next Page")
            {
                _nextPageMenu = AddSubMenu("Next Page", "Next Page");
                return _nextPageMenu.AddButton(text, tooltip, onClick);
            }

            var button = new ReQuickButton(NextButtonPos, text, tooltip, onClick, transform);
            ++_buttonsAdded;
            return button;
        }

        public ReQuickToggle AddToggle(string text, string tooltip, Action<bool> onToggle, bool defaultValue = false)
        {
            if (_nextPageMenu != null)
            {
                return _nextPageMenu.AddToggle(text, tooltip, onToggle, defaultValue);
            }

            if ((_buttonsAdded + 1) == MaxFullButtons && text != "Next Page")
            {
                _nextPageMenu = AddSubMenu("Next Page", "Next Page");
                return _nextPageMenu.AddToggle(text, tooltip, onToggle, defaultValue);
            }

            var toggle = new ReQuickToggle(NextButtonPos, text, tooltip, onToggle, defaultValue, transform);
            ++_buttonsAdded;
            return toggle;
        }

        public ReQuickMenu AddSubMenu(string menuName, string tooltip)
        {
            var menu = new ReQuickMenu(menuName, _name);
            AddButton(menuName, tooltip, () => menu.Open());
            _subMenus.Add(menu);
            return menu;
        }

        public ReQuickMenu GetSubMenu(string menuName)
        {
            return _subMenus.FirstOrDefault(m => m._name == menuName);
        }

        private Vector2 NextButtonPos => new Vector2(-625 + (_buttonsAdded % 4) * ButtonSize, (ButtonSize * 2.5f) - (_buttonsAdded / 4) * ButtonSize); // meth
    }
}
