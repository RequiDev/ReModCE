using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReModCE.Loader;

namespace ReModCE
{
    public static class ReModCE
    {
        public static void OnApplicationStart()
        {
            ReLogger.Msg(nameof(OnApplicationStart));
        }
        public static void OnUiManagerInit()
        {
            ReLogger.Msg(nameof(OnUiManagerInit));
        }

        public static void OnFixedUpdate()
        {
            ReLogger.Msg(nameof(OnFixedUpdate));
        }

        public static void OnUpdate()
        {
            ReLogger.Msg(nameof(OnUpdate));
        }

        public static void OnLateUpdate()
        {
            ReLogger.Msg(nameof(OnLateUpdate));
        }

        public static void OnGUI()
        {
            ReLogger.Msg(nameof(OnGUI));
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ReLogger.Msg(nameof(OnSceneWasLoaded));
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ReLogger.Msg(nameof(OnSceneWasInitialized));
        }

        public static void OnApplicationQuit()
        {
            ReLogger.Msg(nameof(OnApplicationQuit));
        }

        public static void OnPreferencesLoaded()
        {
            ReLogger.Msg(nameof(OnPreferencesLoaded));
        }

        public static void OnPreferencesSaved()
        {
            ReLogger.Msg(nameof(OnPreferencesSaved));
        }
    }
}
