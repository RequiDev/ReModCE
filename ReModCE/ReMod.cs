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
using ReModCE.UI;
using UnityEngine;
using QuickMenuContext = QuickMenuContextualDisplay.EnumNPublicSealedvaUnNoToUs7vUsNoUnique;

namespace ReModCE
{
    public static class ReModCE
    {
        private static readonly List<ModComponent> Components = new List<ModComponent>();

        public static void OnApplicationStart()
        {
            ReLogger.Msg("Initializing...");
            InitializeModComponents();
            ReLogger.Msg("Done!");
        }

        public static void OnUiManagerInit()
        {
            GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/UserIconCameraButton").transform.localPosition +=
                new Vector3(420f, -420f, 0f);

            var reportWorldButton = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/ReportWorldButton").GetComponent<RectTransform>().localPosition;

            var menu = new ReQuickMenu("ReModCE");
            menu.OnOpen += () => ReLogger.Msg($"Menu opened.");

            var button = new ReQuickButton(new Vector2(reportWorldButton.x, reportWorldButton.y + (420f * 2f)),
                "ReMod <color=#00ff00>CE</color>", "Access the ReMod Community Edition",
                () => menu.Open(),
                QuickMenu.prop_QuickMenu_0.transform.Find("ShortcutMenu"));

            var targetMenu = new ReQuickMenu("ReModCETarget", "UserInteractMenu", QuickMenuContext.UserSelected);
            targetMenu.OnOpen += () => ReLogger.Msg($"Target menu opened.");
            var targetButton = new ReQuickButton(new Vector2(reportWorldButton.x, reportWorldButton.y - (420f * 2f)),
                "Target Options", "More options for this target",
                () => targetMenu.Open(QuickMenuContext.UserSelected),
                QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu"));

            foreach (var t in Components)
            {
                t.OnUiManagerInit();
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
                if (t.BaseType == null)
                    continue;
                if (t.BaseType.Name != nameof(ModComponent))
                    continue;

                AddModComponent(t);
            }

            ReLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} internal mod components.");
        }
    }
}
