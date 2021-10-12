using System.Linq;
using UnityEngine;
using VRC.UI.Elements;

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

        public static MenuStateController MenuStateCtrl => Instance.MenuStateController;

        private static Wing[] _wings;
        private static Wing _leftWing;
        private static Wing _rightWing;

        public static Wing[] Wings
        {
            get
            {
                if (_wings == null || _wings.Length == 0)
                {
                    _wings = Object.FindObjectsOfType<Wing>();
                }

                return _wings;
            }
        }

        public static Wing LeftWing
        {
            get
            {
                if (_leftWing == null)
                {
                    _leftWing = Wings.FirstOrDefault(w => w.wingType == Wing.WingPanel.Left);
                }
                return _leftWing;
            }
        }

        public static Wing RightWing
        {
            get
            {
                if (_rightWing == null)
                {
                    _rightWing = Wings.FirstOrDefault(w => w.wingType == Wing.WingPanel.Right);
                }
                return _rightWing;
            }
        }
    }
}
