using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.UI.Elements;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class ReWingButton : UIElement
    {
        [Flags]
        public enum WingSide
        {
            Left = (1 << 0),
            Right = (1 << 1),
            Both = Left | Right
        }

        private static Wing[] _wings;
        private static Wing _leftWing;
        private static Wing _rightWing;

        private static GameObject _wingButtonPrefab;

        private static Wing[] Wings
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

        private static Wing LeftWing
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

        private static Wing RightWing
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

        private static GameObject WingButtonPrefab
        {
            get
            {
                if (_wingButtonPrefab == null)
                {
                    _wingButtonPrefab = LeftWing.transform.Find("Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Profile").gameObject;
                }
                return _wingButtonPrefab;
            }
        }

        protected ReWingButton(string text, Action onClick, bool left = true, bool arrow = true, bool background = true,
            bool seperator = false) : base(WingButtonPrefab, (left ? LeftWing : RightWing).transform.Find("Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup"), $"Button_{text}")
        {
            var container = RectTransform.Find("Container").transform;
            container.Find("Background").gameObject.SetActive(background);
            container.Find("Icon_Arrow").gameObject.SetActive(arrow);
            RectTransform.Find("Separator").gameObject.SetActive(seperator);
            container.Find("Icon").gameObject.SetActive(false);

            var tmp = container.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = text;
            tmp.richText = true;

            var button = GameObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(onClick));
        }

        public static void Create(string text, Action onClick, WingSide wingSide = WingSide.Left, bool arrow = true, bool background = true,
            bool seperator = false)
        {
            if ((wingSide & WingSide.Left) == WingSide.Left)
            {
                var wingButton = new ReWingButton(text, onClick, true, arrow, background, seperator);
            }
            if ((wingSide & WingSide.Right) == WingSide.Right)
            {
                var wingButton = new ReWingButton(text, onClick, false, arrow, background, seperator);
            }
        }
    }
}
