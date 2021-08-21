using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReModCE.Core;
using ReModCE.Loader;
using UnityEngine;

namespace ReModCE.Components
{
    internal class CalibrationSavingComponent : ModComponent
    {
        private class DynamicContractResolver : DefaultContractResolver
        {
            private readonly string _propertyNameToExclude;

            public DynamicContractResolver(string propertyNameToExclude)
            {
                _propertyNameToExclude = propertyNameToExclude;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);

                // only serializer properties that are not named after the specified property.
                properties =
                    properties.Where(p => string.Compare(p.PropertyName, _propertyNameToExclude, StringComparison.OrdinalIgnoreCase) != 0).ToList();

                return properties;
            }
        }

        private class FbtCalibration
        {
            public KeyValuePair<Vector3, Quaternion> Hip;
            public KeyValuePair<Vector3, Quaternion> LeftFoot;
            public KeyValuePair<Vector3, Quaternion> RightFoot;
        }

        private static Dictionary<string, FbtCalibration> _savedCalibrations;
        private static ConfigValue<bool> CalibrationSaverEnabled;

        public CalibrationSavingComponent()
        {
            CalibrationSaverEnabled = new ConfigValue<bool>(nameof(CalibrationSaverEnabled), true);
            if (MelonHandler.Mods.Any(i => i.Info.Name == "FBT Saver"))
            {
                ReLogger.Msg(ConsoleColor.Yellow, "Found FBT Saver Mod. Not loading Calibration Saver.");
                return;
            }

            if (File.Exists("UserData/ReModCE/calibrations.json"))
            {
                _savedCalibrations =
                    JsonConvert.DeserializeObject<Dictionary<string, FbtCalibration>>(
                        File.ReadAllText("UserData/ReModCE/calibrations.json"));

                ReLogger.Msg($"Loaded {_savedCalibrations.Count} calibrations from disk.");
            }
            else
            {
                ReLogger.Msg($"No saved calibrations found. Creating new.");
                _savedCalibrations = new Dictionary<string, FbtCalibration>();
                File.WriteAllText("UserData/ReModCE/calibrations.json", JsonConvert.SerializeObject(_savedCalibrations, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }


            var methods = typeof(VRCTrackingSteam).GetMethods();
            foreach (var methodInfo in methods)
            {
                switch (methodInfo.GetParameters().Length)
                {
                    case 1 when methodInfo.GetParameters().First().ParameterType == typeof(string) && methodInfo.ReturnType == typeof(bool) && methodInfo.GetRuntimeBaseDefinition() == methodInfo:
                        ReModCE.Harmony.Patch(methodInfo, GetLocalPatch(nameof(IsCalibratedForAvatar)));
                        break;
                    case 3 when methodInfo.GetParameters().First().ParameterType == typeof(Animator) && methodInfo.ReturnType == typeof(void) && methodInfo.GetRuntimeBaseDefinition() == methodInfo:
                        ReModCE.Harmony.Patch(methodInfo, GetLocalPatch(nameof(PerformCalibration)));
                        break;
                }
            }
        }

        private static void PerformCalibration(ref VRCTrackingSteam __instance, Animator __0, bool __1, bool __2)
        {
            if (!CalibrationSaverEnabled)
                return;

            if (__0 == null || __instance == null)
            {
                return;
            }

            var avatarId = VRCPlayer.field_Internal_Static_VRCPlayer_0._player.prop_ApiAvatar_0.id;
            _savedCalibrations[avatarId] = new FbtCalibration
            {
                LeftFoot = new KeyValuePair<Vector3, Quaternion>(__instance.field_Public_Transform_10.localPosition, __instance.field_Public_Transform_10.localRotation),
                RightFoot = new KeyValuePair<Vector3, Quaternion>(__instance.field_Public_Transform_11.localPosition, __instance.field_Public_Transform_11.localRotation),
                Hip = new KeyValuePair<Vector3, Quaternion>(__instance.field_Public_Transform_12.localPosition, __instance.field_Public_Transform_12.localRotation),
            };

            try
            {
                File.WriteAllText("UserData/ReModCE/calibrations.json", JsonConvert.SerializeObject(_savedCalibrations, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new DynamicContractResolver("normalized")
                }));
            }
            catch (Exception e)
            {
                ReLogger.Error($"Could not save current calibration to file!\n {e}");
            }
        }

        private static bool IsCalibratedForAvatar(ref VRCTrackingSteam __instance, ref bool __result, string __0)
        {
            if (!CalibrationSaverEnabled)
                return true;

            if (__instance.field_Private_String_0 == null)
            {
                __result = false;
                return true;
            }

            if (_savedCalibrations.ContainsKey(__0))
            {
                var savedCalib = _savedCalibrations[__0];
                __instance.field_Public_Transform_10.localPosition = savedCalib.LeftFoot.Key;
                __instance.field_Public_Transform_10.localRotation = savedCalib.LeftFoot.Value;

                __instance.field_Public_Transform_11.localPosition = savedCalib.RightFoot.Key;
                __instance.field_Public_Transform_11.localRotation = savedCalib.RightFoot.Value;

                __instance.field_Public_Transform_12.localPosition = savedCalib.Hip.Key;
                __instance.field_Public_Transform_12.localRotation = savedCalib.Hip.Value;

                __result = true;
                return false;
            }

            __result = true;
            return false;
        }
    }
}
