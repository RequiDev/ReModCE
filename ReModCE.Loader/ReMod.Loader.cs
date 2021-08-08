using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }

        public override void OnPreferencesLoaded()
        {
            base.OnPreferencesLoaded();
        }

        public override void OnPreferencesSaved()
        {
            base.OnPreferencesSaved();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);
        }
    }
}
