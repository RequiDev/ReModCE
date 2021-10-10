using UnityEngine;

namespace ReModCE.VRChat
{
    internal static class ExtendedQuickMenu
    {
        private static VRC.UI.Elements.QuickMenu _quickMenuInstance;

        public static VRC.UI.Elements.QuickMenu Instance
        {
            get
            {
                if (_quickMenuInstance == null)
                {
                    _quickMenuInstance = Object.FindObjectOfType<VRC.UI.Elements.QuickMenu>();
                }
                return _quickMenuInstance;
            }
        }

        public static VRC.UI.Elements.MenuStateController MenuStateCtrl => Instance.MenuStateController;
    }
}
