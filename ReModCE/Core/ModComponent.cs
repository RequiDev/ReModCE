using System;

namespace ReModCE.Core
{
    public class ComponentDisabled : Attribute
    {
    }

    internal class ModComponent
    {
        public virtual void OnUiManagerInit() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnGUI() { }
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }
        public virtual void OnApplicationQuit() { }
        public virtual void OnPreferencesLoaded() { }
        public virtual void OnPreferencesSaved() { }
    }
}
