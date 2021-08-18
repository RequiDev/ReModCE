using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using ReModCE.Managers;

namespace ReModCE.Core
{
    internal class ComponentDisabled : Attribute
    {
    }

    internal class ComponentPriority : Attribute
    {
        public int Priority;

        public ComponentPriority(int priority = 0) => Priority = priority;
    }

    internal class ModComponent
    {
        public virtual void OnUiManagerInit(UiManager uiManager) { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnGUI() { }
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }
        public virtual void OnApplicationQuit() { }
        public virtual void OnPreferencesLoaded() { }
        public virtual void OnPreferencesSaved() { }
        public virtual void OnAvatarIsReady(VRCPlayer vrcPlayer) { }

        protected HarmonyMethod GetLocalPatch(string methodName)
        {
            return GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod();
        }
    }
}
