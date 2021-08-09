using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE.UI
{
    internal class ReMenu : UIElement
    {
        public event Action OnOpen;

        public ReMenu(string name) : base(GameObject.Find("UserInterface/QuickMenu/CameraMenu"), QuickMenu.prop_QuickMenu_0.transform, name, false)
        {
            foreach (var obj in transform)
            {
                var control = obj.Cast<Transform>();
                if (control == null || control.name == "BackButton")
                {
                    continue;
                }

                Object.Destroy(control.gameObject);
            }
        }

        public void Open(QuickMenuContext context = QuickMenuContext.NoSelection)
        {
            QuickMenu.prop_QuickMenu_0.ShowPage(gameObject.name);
            OnOpen?.Invoke();
        }
    }
}
