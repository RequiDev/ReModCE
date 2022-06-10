using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using VRC.UI.Core;

namespace ReModCE.Loader
{
    public static class BuildInfo
    {
        public const string Name = "ReModCE";
        public const string Author = "Requi, FenrixTheFox, Xaiver, Potato, Psychloor";
        public const string Company = null;
        public const string Version = "1.0.0.6";
        public const string DownloadLink = "https://github.com/RequiDev/ReModCE/releases/latest/";
    }

    internal static class GitHubInfo
    {
        public const string Author = "RequiDev";
        public const string Repository = "ReModCE";
        public const string Version = "latest";
    }

    public class ReLoader : MelonMod
    {
        private ReLogger _logger;
        
        private Action _onApplicationStart;
        private Action _onUiManagerInit;
        private Action _onUiManagerInitEarly;
        private Action _onFixedUpdate;
        private Action _onUpdate;
        private Action _onGUI;
        private Action _onApplicationQuit;
        private Action _onLateUpdate;
        private Action _onPreferencesLoaded;
        private Action _onPreferencesSaved;

        private Action<int, string> _onSceneWasLoaded;
        private Action<int, string> _onSceneWasInitialized;

        private readonly MelonPreferences_Entry<bool> _paranoidMode;

        public ReLoader()
        {
            _logger = new ReLogger(new MelonLogger.Instance("ReModCE"));

            var category = MelonPreferences.CreateCategory("ReModCE");
            _paranoidMode = category.CreateEntry("ParanoidMode", false, "Paranoid Mode",
                "If enabled ReModCE will not automatically download the latest version from GitHub. Manual update will be required.",
                true);

            // check for ReMod.Core.Updater plugin before attempting to load ReMod.Core
            if (MelonHandler.Plugins.Any(p => p.Info.Name == "ReMod.Core.Updater"))
                return;
            
            ReLogger.Msg($"Loading ReMod.Core early so other mods don't break me...");
            DownloadFromGitHub("ReMod.Core", out _);
        }

        public override void OnApplicationStart()
        {
            DownloadFromGitHub("ReModCE", out var assembly);

            if (assembly == null)
                return;

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
                    case nameof(OnUiManagerInitEarly) when parameters.Length == 0:
                        _onUiManagerInitEarly = (Action)Delegate.CreateDelegate(typeof(Action), m);
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

        public void OnUiManagerInitEarly()
        {
            _onUiManagerInitEarly();
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
            OnUiManagerInitEarly();

            while (UIManager.field_Private_Static_UIManager_0 == null) yield return null;
            while (GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true) == null) yield return null;

            OnUiManagerInit();
        }

        private void DownloadFromGitHub(string fileName, out Assembly loadedAssembly)
        {
            using var sha256 = SHA256.Create();

            // delete files saved in old path
            if (File.Exists($"{fileName}.dll"))
            {
                File.Delete($"{fileName}.dll");
            }

            byte[] bytes = null;
            var path = Path.Combine("UserLibs", $"{fileName}.dll");
            if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }

            using var wc = new WebClient
            {
                Headers =
                {
                    ["User-Agent"] =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0"
                }
            };

            byte[] latestBytes = null;
            try
            {
                latestBytes = wc.DownloadData($"https://github.com/{GitHubInfo.Author}/{GitHubInfo.Repository}/releases/{GitHubInfo.Version}/download/{fileName}.dll");
            }
            catch (WebException e)
            {
                MelonLogger.Error($"Unable to download latest version of ReModCE: {e}");
            }

            if (bytes == null)
            {
                if (latestBytes == null)
                {
                    MelonLogger.Error($"No local file exists and unable to download latest version from GitHub. {fileName} will not load!");
                    loadedAssembly = null;
                    return;
                }
                MelonLogger.Warning($"Couldn't find {fileName}.dll on disk. Saving latest version from GitHub.");
                bytes = latestBytes;
                try
                {
                    File.WriteAllBytes(path, bytes);
                }
                catch (IOException e)
                {
                    ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                }
            }

#if !DEBUG
            if (latestBytes != null)
            {
                var latestHash = ComputeHash(sha256, latestBytes);
                var currentHash = ComputeHash(sha256, bytes);

                if (latestHash != currentHash)
                {
                    if (_paranoidMode.Value)
                    {
                        MelonLogger.Msg(ConsoleColor.Cyan,
                            $"There is a new version of ReModCE available. You can either delete the \"{fileName}.dll\" from your VRChat directory or go to https://github.com/{GitHubInfo.Author}/{GitHubInfo.Repository}/releases/latest/ and download the latest version.");
                    }
                    else
                    {
                        bytes = latestBytes;
                        try
                        {
                            File.WriteAllBytes(path, bytes);
                        }
                        catch (IOException e)
                        {
                            ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                        }
                        MelonLogger.Msg(ConsoleColor.Green, $"Updated {fileName} to latest version.");
                    }
                }
            }
#endif

            try
            {
                loadedAssembly = Assembly.Load(bytes);
            }
            catch (BadImageFormatException e)
            {
                MelonLogger.Error($"Couldn't load specified image: {e}");
                loadedAssembly = null;
            }
        }

        private static string ComputeHash(HashAlgorithm sha256, byte[] data)
        {
            var bytes = sha256.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
