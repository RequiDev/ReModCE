using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class UIElement
    {
        protected string Name { get; }
        protected GameObject GameObject { get; }
        protected RectTransform RectTransform { get; }

        public Vector3 Position
        {
            get => RectTransform.localPosition;
            set => RectTransform.localPosition = value;
        }

        public UIElement(GameObject original, Transform parent, Vector3 pos, string name, bool defaultState = true) : this(original, parent, name, defaultState)
        {
            GameObject.transform.localPosition = pos;
        }

        public UIElement(GameObject original, Transform parent, string name, bool defaultState = true)
        {
            GameObject = Object.Instantiate(original, parent);
            GameObject.name = string.Concat(name.Where(char.IsLetter));
            Name = GameObject.name;

            GameObject.SetActive(defaultState);
            RectTransform = GameObject.GetComponent<RectTransform>();
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
        }
    }
}
