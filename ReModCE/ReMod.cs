using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using ReModCE.Components;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReModCE
{
    public static class ReModCE
    {
        private static readonly List<ModComponent> Components = new List<ModComponent>();
        private static UiManager _uiManager;

        public static void OnApplicationStart()
        {
            ReLogger.Msg("Initializing...");
            
            RiskyFunctionsManager.AppStart();

            InitializePatches();

            InitializeModComponents();
            ReLogger.Msg("Done!");
        }

        private static HarmonyMethod GetLocalPatch(string name)
        {
            return new HarmonyMethod(typeof(ReModCE).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static void InitializePatches()
        {
            var harmony = new HarmonyLib.Harmony("ReModCE");
            harmony.Patch(typeof(VRCPlayer).GetMethod(nameof(VRCPlayer.Awake)), GetLocalPatch(nameof(VRCPlayerAwakePatch)));
        }

        public static void OnUiManagerInit()
        {
            ReLogger.Msg("Initializing UI...");

            _uiManager = new UiManager("ReModCE");

            foreach (var m in Components)
            {
                m.OnUiManagerInit(_uiManager);
            }
        }

        public static void OnFixedUpdate()
        {
            foreach (var m in Components)
            {
                m.OnFixedUpdate();
            }
        }

        public static void OnUpdate()
        {
            foreach (var m in Components)
            {
                m.OnUpdate();
            }
        }

        public static void OnLateUpdate()
        {
            foreach (var m in Components)
            {
                m.OnLateUpdate();
            }
        }

        public static void OnGUI()
        {
            foreach (var m in Components)
            {
                m.OnGUI();
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasLoaded(buildIndex, sceneName);
            }
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasInitialized(buildIndex, sceneName);
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (var m in Components)
            {
                m.OnApplicationQuit();
            }

            Process.GetCurrentProcess().Kill();
        }

        public static void OnPreferencesLoaded()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesLoaded();
            }
        }

        public static void OnPreferencesSaved()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesSaved();
            }
        }

        private static void AddModComponent(Type type)
        {
            try
            {
                var newModComponent = Activator.CreateInstance(type) as ModComponent;
                Components.Add(newModComponent);
            }
            catch (Exception e)
            {
                ReLogger.Error($"Failed adding ModComponent.\n{e}");
            }
        }

        private static void InitializeModComponents()
        {
            var assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException reflectionTypeLoadException)
            {
                types = reflectionTypeLoadException.Types.Where(t => t != null);
            }

            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;
                if (t.BaseType != typeof(ModComponent))
                    continue;
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                AddModComponent(t);
            }

            ReLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} mod components.");
        }


        private static void VRCPlayerAwakePatch(VRCPlayer __instance)
        {
            if (__instance == null) return;

            __instance.Method_Public_add_Void_MulticastDelegateNPublicSealedVoUnique_0(new Action(() =>
            {
                foreach (var m in Components)
                {
                    m.OnAvatarIsReady(__instance);
                }
            }));
        }
    }
}
