using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using ReMod.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;

namespace ReModCE.Managers
{
    [ComponentPriority(int.MinValue)]
    internal class RiskyFunctionsManager : ModComponent
    {
        public static RiskyFunctionsManager Instance;

        public event Action<bool> OnRiskyFunctionsChanged;

        private readonly List<string> _blacklistedTags = new List<string>
        {
            "author_tag_game",
            "author_tag_games",
            "author_tag_club",
            "admin_game"
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
            var apiWorldInstance = RoomManager.field_Internal_Static_ApiWorldInstance_0;

            bool riskyFunctionAllowed;
            if (apiWorldInstance.type <= InstanceAccessType.FriendsOfGuests)
            {
                var worldName = apiWorld.name.ToLower();
                var tags = new List<string>();
                foreach (var tag in apiWorld.tags)
                {
                    tags.Add(tag.ToLower());
                }

                var hasBlacklistedTag = _blacklistedTags.Any(tag => tags.Contains(tag));
                riskyFunctionAllowed =
                    !worldName.Contains("club") && !worldName.Contains("game") && !hasBlacklistedTag;

                var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                if (rootGameObjects.Any(go => go.name is "eVRCRiskFuncDisable" or "UniversalRiskyFuncDisable"))
                {
                    riskyFunctionAllowed = false;
                }
                else if (rootGameObjects.Any(go => go.name is "eVRCRiskFuncEnable" or "UniversalRiskyFuncEnable"))
                {
                    riskyFunctionAllowed = true;
                }
            }
            else
            {
                riskyFunctionAllowed = true;
            }

            RiskyFunctionAllowed = riskyFunctionAllowed;
            OnRiskyFunctionsChanged?.Invoke(RiskyFunctionAllowed);
        }
    }
}
