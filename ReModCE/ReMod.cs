using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            ForwardedLogger.Msg("Initializing...");

            ReLogger.AppStart();
            RiskyFunctionsManager.AppStart();

            InitializeModComponents();
            ForwardedLogger.Msg("Done!");
        }

        public static void OnUiManagerInit()
        {
            ForwardedLogger.Msg("Initializing UI...");
            ReLogger.UiInit();

            _uiManager = new UiManager("ReModCE");

            foreach (var t in Components)
            {
                t.OnUiManagerInit(_uiManager);
            }
        }

        public static void OnFixedUpdate()
        {
            foreach (var t in Components)
            {
                t.OnFixedUpdate();
            }
        }

        public static void OnUpdate()
        {
            foreach (var t in Components)
            {
                t.OnUpdate();
            }
        }

        public static void OnLateUpdate()
        {
            foreach (var t in Components)
            {
                t.OnLateUpdate();
            }
        }

        public static void OnGUI()
        {
            foreach (var t in Components)
            {
                t.OnGUI();
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var t in Components)
            {
                t.OnSceneWasLoaded(buildIndex, sceneName);
            }
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var t in Components)
            {
                t.OnSceneWasInitialized(buildIndex, sceneName);
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (var t in Components)
            {
                t.OnApplicationQuit();
            }

            Process.GetCurrentProcess().Kill();
        }

        public static void OnPreferencesLoaded()
        {
            foreach (var t in Components)
            {
                t.OnPreferencesLoaded();
            }
        }

        public static void OnPreferencesSaved()
        {
            foreach (var t in Components)
            {
                t.OnPreferencesSaved();
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
                ForwardedLogger.Error($"Failed adding ModComponent.\n{e}");
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
                if (t.BaseType == null)
                    continue;
                if (t.BaseType.Name != nameof(ModComponent))
                    continue;
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                AddModComponent(t);
            }

            ForwardedLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} internal mod components.");
        }
    }
}
