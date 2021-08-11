using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using ReModCE.Loader;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Managers
{
    // modcomponent + singleton?
    internal static class RiskyFunctionsManager
    {
        public static event Action<bool> OnRiskyFunctionsChanged;

        public static bool RiskyFunctionAllowed { get; private set; }

        public static void AppStart()
        {
            // possibly move this to main mod class?
            var harmony = new HarmonyLib.Harmony("ReModCE");
            harmony.Patch(
                typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)),
                new HarmonyMethod(typeof(RiskyFunctionsManager).GetMethod(nameof(EnterWorldPatch),
                    BindingFlags.Static | BindingFlags.NonPublic)));
        }

        private static void EnterWorldPatch(ApiWorld __0, ApiWorldInstance __1)
        {
            var worldName = __0.name.ToLower();
            RiskyFunctionAllowed = !worldName.Contains("club") && !worldName.Contains("game") && !__0.tags.Contains("author_tag_game") && !__0.tags.Contains("author_tag_club");
            OnRiskyFunctionsChanged?.Invoke(RiskyFunctionAllowed);
        }
    }
}
