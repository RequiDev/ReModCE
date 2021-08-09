using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReModCE.UI
{
    internal class UIElement
    {
        protected GameObject gameObject { get; }
        protected Transform transform { get; }

        public UIElement(GameObject original, Transform parent, Vector3 pos, string name, bool defaultState = true) : this(original, parent, name, defaultState)
        {
            gameObject.transform.localPosition = pos;
        }

        public UIElement(GameObject original, Transform parent, string name, bool defaultState = true)
        {
            gameObject = Object.Instantiate(original, parent);
            gameObject.name = name;

            gameObject.SetActive(defaultState);
            transform = gameObject.transform;
        }

        public void Destroy()
        {
            Object.Destroy(gameObject);
        }
    }
}
