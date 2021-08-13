using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace ReModCE.Loader
{
    public static class BuildInfo
    {
        public const string Name = "ReModCE";
        public const string Author = "Requi, FenrixTheFox";
        public const string Company = null;
        public const string Version = "1.0.0.0";
    }

    public class ReLoader : MelonMod
    {
        private Action _onApplicationStart;
        private Action _onUiManagerInit;
        private Action _onFixedUpdate;
        private Action _onUpdate;
        private Action _onGUI;
        private Action _onApplicationQuit;
        private Action _onLateUpdate;
        private Action _onPreferencesLoaded;
        private Action _onPreferencesSaved;

        private Action<int, string> _onSceneWasLoaded;
        private Action<int, string> _onSceneWasInitialized;

        private List<string> _possiblePaths = new List<string>
        {
            "ReModCE.dll",
            "Mods/ReModCE.dll"
        };

        public override void OnApplicationStart()
        {
            var bytes = (from path in _possiblePaths where File.Exists(path) select File.ReadAllBytes(path)).FirstOrDefault();

            if (bytes == null)
            {
                MelonLogger.Error($"Couldn't find ReModCE.dll. Can't load ReModCE.");
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.Load(bytes);
            }
            catch (BadImageFormatException e)
            {
                MelonLogger.Error($"Couldn't load specified image: {e}");
                return;
            }

            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }

            var remodClass = types.FirstOrDefault(type => type.Name == "ReModCE");
            if (remodClass == null)
            {
                MelonLogger.Error($"Couldn't find ReModCE class in assembly. ReModCE won't load.");
                return;
            }

            var methods = remodClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var m in methods)
            {
                var parameters = m.GetParameters();
                switch (m.Name)
                {
                    case nameof(OnApplicationStart) when parameters.Length == 0:
                        _onApplicationStart = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnApplicationQuit) when parameters.Length == 0:
                        _onApplicationQuit = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnSceneWasLoaded) when parameters.Length == 2 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(string):
                        _onSceneWasLoaded = (Action<int, string>)Delegate.CreateDelegate(typeof(Action<int, string>), m);
                        break;
                    case nameof(OnSceneWasInitialized) when parameters.Length == 2 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(string):
                        _onSceneWasInitialized = (Action<int, string>)Delegate.CreateDelegate(typeof(Action<int, string>), m);
                        break;
                    case nameof(OnUpdate) when parameters.Length == 0:
                        _onUpdate = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnUiManagerInit) when parameters.Length == 0:
                        _onUiManagerInit = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnGUI) when parameters.Length == 0:
                        _onGUI = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnLateUpdate) when parameters.Length == 0:
                        _onLateUpdate = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnFixedUpdate) when parameters.Length == 0:
                        _onFixedUpdate = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnPreferencesLoaded) when parameters.Length == 0:
                        _onPreferencesLoaded = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                    case nameof(OnPreferencesSaved) when parameters.Length == 0:
                        _onPreferencesSaved = (Action)Delegate.CreateDelegate(typeof(Action), m);
                        break;
                }
            }

            MelonCoroutines.Start(WaitForUiManager());
            _onApplicationStart();
        }

        public void OnUiManagerInit()
        {
            _onUiManagerInit();
        }

        public override void OnFixedUpdate()
        {
            _onFixedUpdate();
        }

        public override void OnUpdate()
        {
            _onUpdate();
        }

        public override void OnLateUpdate()
        {
            _onLateUpdate();
        }

        public override void OnGUI()
        {
            _onGUI();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _onSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            _onSceneWasInitialized(buildIndex, sceneName);
        }

        public override void OnApplicationQuit()
        {
            _onApplicationQuit();
        }

        public override void OnPreferencesLoaded()
        {
            _onPreferencesLoaded();
        }

        public override void OnPreferencesSaved()
        {
            _onPreferencesSaved();
        }

        private IEnumerator WaitForUiManager()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;
            while (QuickMenu.prop_QuickMenu_0 == null) yield return null;

            OnUiManagerInit();
        }
    }
}
