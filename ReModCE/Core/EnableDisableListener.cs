using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ReModCE.Core
{
    public class EnableDisableListener : MonoBehaviour
    {
        [method: HideFromIl2Cpp]
        public event Action OnEnableEvent;
        [method: HideFromIl2Cpp]
        public event Action OnDisableEvent;

        public EnableDisableListener(IntPtr obj) : base(obj){ }
        public void OnEnable() => OnEnableEvent?.Invoke();
        public void OnDisable() => OnDisableEvent?.Invoke();
    }
}
