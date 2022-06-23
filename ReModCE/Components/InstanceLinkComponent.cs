using ReMod.Core;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace ReModCE.Components
{
    internal class InstanceLinkComponent : ModComponent
    {
        public override void OnUiManagerInitEarly()
        {
            var userProfileSection = VRCUiManagerEx.Instance.MenuContent().transform
                .Find("Screens/Social/UserProfileAndStatusSection");
            var currentStatus = userProfileSection.Find("Status");
            var viewProfileButton = userProfileSection.Find("ViewProfileButton");

            var joinInstanceButton = new ReUiButton("Join Instance",
                viewProfileButton.localPosition - new Vector3(20f, 0f), Vector2.one,
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Join Instance", string.Empty, InputField.InputType.Standard, false, "Join", (s, l, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        var joinId = s;
                        if (s.Contains("worldId=") && s.Contains("&instanceId="))
                        {
                            var worldIdIndex = s.IndexOf("worldId=");
                            var instanceIdIndex = s.IndexOf("&instanceId=");
                            var worldId = s.Substring(worldIdIndex + "worldId=".Length, instanceIdIndex - (worldIdIndex + "worldId=".Length));
                            var instanceId = s.Substring(instanceIdIndex + "&instanceId=".Length);

                            joinId = $"{worldId}:{instanceId}".Trim().TrimEnd('\r', '\n');
                            ReLogger.Msg($"Parsed {joinId} from join link!");
                        }

                        Networking.GoToRoom(joinId);
                    }, null);
                }, userProfileSection);

            var sizeDeltaX = viewProfileButton.GetComponent<RectTransform>().sizeDelta.x;
            joinInstanceButton.RectTransform.sizeDelta = new Vector2(sizeDeltaX, joinInstanceButton.RectTransform.sizeDelta.y);

            viewProfileButton.transform.localPosition += new Vector3(210f, 0f);
            currentStatus.transform.localPosition += new Vector3(210f, 0f);

            var copyInstanceButton = new ReUiButton("Copy Joinlink",
                viewProfileButton.localPosition - new Vector3(20f, 0f), Vector2.one,
                () =>
                {
                    var apiWorld = RoomManager.field_Internal_Static_ApiWorld_0;
                    var apiWorldInstance = RoomManager.field_Internal_Static_ApiWorldInstance_0;
                    GUIUtility.systemCopyBuffer = $"{apiWorld.id}:{apiWorldInstance.instanceId}";
                }, userProfileSection);
            copyInstanceButton.RectTransform.sizeDelta = new Vector2(sizeDeltaX, copyInstanceButton.RectTransform.sizeDelta.y);

            viewProfileButton.transform.localPosition += new Vector3(210f, 0f);
            currentStatus.transform.localPosition += new Vector3(210f, 0f);
        }
    }
}
