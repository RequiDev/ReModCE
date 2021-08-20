using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReModCE.VRChat
{
    public static class ExtendedCursor
    {
        public static VRCUiCursor CurrentCursor => VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0();

        public static bool IsUseInputPressed => CurrentCursor.field_Private_VRCInput_0.field_Private_Boolean_0 &&
                                                CurrentCursor.gameObject.active;

        public static Vector3 HitPosition =>
            CurrentCursor.gameObject.active ? CurrentCursor.field_Public_Vector3_0 : Vector3.zero;
    }
}
