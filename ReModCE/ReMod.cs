using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using UnhollowerRuntimeLib;

namespace ReModCE
{
    public static class ReModCE
    {
        private static readonly List<ModComponent> Components = new List<ModComponent>();
        private static UiManager _uiManager;

        public static HarmonyLib.Harmony Harmony { get; private set; }

        public static void OnApplicationStart()
        {
            Harmony = MelonHandler.Mods.First(m => m.Info.Name == "ReModCE").HarmonyInstance;
            Directory.CreateDirectory("UserData/ReModCE");
            ReLogger.Msg("Initializing...");
            
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();

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
            Harmony.Patch(typeof(VRCPlayer).GetMethod(nameof(VRCPlayer.Awake)), GetLocalPatch(nameof(VRCPlayerAwakePatch)));
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

        private class LoadableModComponent
        {
            public int Priority;
            public Type Component;
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

            var loadableModComponents = new List<LoadableModComponent>();
            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;
                if (t.BaseType != typeof(ModComponent))
                    continue;
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                var priority = 0;
                if (t.IsDefined(typeof(ComponentPriority)))
                {
                    priority = ((ComponentPriority)Attribute.GetCustomAttribute(t, typeof(ComponentPriority)))
                        .Priority;
                }

                loadableModComponents.Add(new LoadableModComponent
                {
                    Component = t,
                    Priority = priority
                });
            }

            var sortedComponents = loadableModComponents.OrderBy(component => component.Priority);
            foreach (var modComp in sortedComponents)
            {
                AddModComponent(modComp.Component);
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
