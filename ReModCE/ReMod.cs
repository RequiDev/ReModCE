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
using Object = UnityEngine.Object;
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

        public static string ToHexCode(ConsoleColor c)
        {
            string[] cColors = { 
                "#000000", //Black = 0
                "#000080", //DarkBlue = 1
                "#008000", //DarkGreen = 2
                "#008080", //DarkCyan = 3
                "#800000", //DarkRed = 4
                "#800080", //DarkMagenta = 5
                "#808000", //DarkYellow = 6
                "#C0C0C0", //Gray = 7
                "#808080", //DarkGray = 8
                "#0000FF", //Blue = 9
                "#00FF00", //Green = 10
                "#00FFFF", //Cyan = 11
                "#FF0000", //Red = 12
                "#FF00FF", //Magenta = 13
                "#FFFF00", //Yellow = 14
                "#FFFFFF"  //White = 15
            };
            return cColors[(int)c];
        }
        public static void OnUiManagerInit()
        {
            ReLogger.Msg("Initializing UI...");

            var cameraButton = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/UserIconCameraButton");
            var cameraButtonPos = cameraButton.transform.localPosition;
            Object.DestroyImmediate(cameraButton);

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

            var scrollLog = new ReScrollView("LogTest", cameraButtonPos + new Vector3(1050F, -630f), QuickMenu.prop_QuickMenu_0.transform.Find("ShortcutMenu"));

            MelonLogger.MsgCallbackHandler += (color, consoleColor, nameSection, msg) =>
            {
                scrollLog.AddText($"<color={ToHexCode(color)}>[{nameSection}]</color> <color={ToHexCode(consoleColor)}>{msg}</color>\n");
            };

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
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                AddModComponent(t);
            }

            ReLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} internal mod components.");
        }
    }
}
