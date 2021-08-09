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
        public event Action OnOpen;

        public ReQuickMenu(string name, string parent = "ShortcutMenu", QuickMenuContext backButtonContext = QuickMenuContext.NoSelection) : base(GameObject.Find("UserInterface/QuickMenu/CameraMenu"), QuickMenu.prop_QuickMenu_0.transform, name, false)
        {
            var origControl = GameObject.Find("UserInterface/QuickMenu/CameraMenu");
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

                    continue;
                }
                
                Object.Destroy(control.gameObject);
            }
        }

        public void Open(QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            QuickMenu.prop_QuickMenu_0.SetCurrentPage(gameObject.name);
            OnOpen?.Invoke();
        }
    }
}
