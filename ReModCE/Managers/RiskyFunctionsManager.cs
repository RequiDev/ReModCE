﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using ReMod.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReModCE.Managers
{
    [ComponentPriority(int.MinValue)]
    internal class RiskyFunctionsManager : ModComponent
    {
        public static RiskyFunctionsManager Instance;

        public event Action<bool> OnRiskyFunctionsChanged;

        private readonly List<string> _blacklistedTags = new List<string>
        {
        };

        public bool RiskyFunctionAllowed { get; private set; }

        public RiskyFunctionsManager()
        {
            Instance = this;
        }
        
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == -1) // custom scene
            {
                MelonCoroutines.Start(CheckWorld());
            }
        }

        private IEnumerator CheckWorld()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 == null) yield return new WaitForEndOfFrame();
            var apiWorld = RoomManager.field_Internal_Static_ApiWorld_0;

            var worldName = apiWorld.name.ToLower();
            var tags = new List<string>();
            foreach (var tag in apiWorld.tags)
            {
                tags.Add(tag.ToLower());
            }

            var hasBlacklistedTag = _blacklistedTags.Any(tag => tags.Contains(tag));
            var riskyFunctionAllowed = true;

            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            if (rootGameObjects.Any(go => go.name is "eVRCRiskFuncDisable" or "UniversalRiskyFuncDisable"))
            {
                riskyFunctionAllowed = true;
            }
            else if (rootGameObjects.Any(go => go.name is "eVRCRiskFuncEnable" or "UniversalRiskyFuncEnable"))
            {
                riskyFunctionAllowed = true;
            }

            RiskyFunctionAllowed = riskyFunctionAllowed;
            OnRiskyFunctionsChanged?.Invoke(RiskyFunctionAllowed);
        }
    }
}
