using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Loader;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE.Managers
{

    [ComponentPriority(int.MinValue)]
    internal class RiskyFunctionsManager : ModComponent
    {
        public static RiskyFunctionsManager Instance;

        public event Action<bool> OnRiskyFunctionsChanged;

        private static readonly List<string> BlacklistedTags = new List<string>
        {
            "author_tag_game",
            "author_tag_games",
            "author_tag_club",
            "admin_game"
        };

        public bool RiskyFunctionAllowed { get; private set; }

        public RiskyFunctionsManager()
        {
            Instance = this;

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

            var tags = new List<string>();
            foreach (var tag in __0.tags)
            {
                tags.Add(tag.ToLower());
            }

            var hasBlacklistedTag = BlacklistedTags.Any(tag => tags.Contains(tag));

            Instance.RiskyFunctionAllowed = !worldName.Contains("club") && !worldName.Contains("game") && !hasBlacklistedTag;
            Instance.OnRiskyFunctionsChanged?.Invoke(Instance.RiskyFunctionAllowed);
        }
    }
}
