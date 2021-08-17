using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using HarmonyLib;

namespace ReModCE.Components
{
    class ComfyQM: ModComponent
    {
        public ComfyQM()
        {
            var Harmony = new HarmonyLib.Harmony("ReModCE");
            Harmony.Patch(typeof(QuickMenu).GetMethod(nameof(QuickMenu.Method_Private_Void_Boolean_0)), new HarmonyMethod(typeof(ComfyQM).GetMethod("SetupForDesktopOrHMDPatch",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)));
        }
        private static void SetupForDesktopOrHMDPatch(ref bool __0)
        {
            __0 = true;
        }
    }
}
