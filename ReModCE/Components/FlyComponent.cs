using System.Collections.Generic;
using System.Linq;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using ReModCE.Core;
using ReModCE.Managers;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VRC.Animation;
using VRC.SDK3.Components;
using VRC.UI.Elements;
using VRCSDK2;
using VRC_AvatarPedestal = VRC.SDKBase.VRC_AvatarPedestal;
using VRC_Pickup = VRC.SDKBase.VRC_Pickup;
using VRC_UiShape = VRC.SDKBase.VRC_UiShape;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
namespace ReModCE.Components
{
    internal class FlyComponent : ModComponent
    {
        private bool _noclipEnabled;
        private readonly List<int> _disabledColliders = new List<int>();
        private bool _flyEnabled;
        private ConfigValue<float> FlySpeed;
        private Vector3 _originalGravity;
        private ConfigValue<bool> EnableFlyHotkey;
        
        private ReMenuButton _flySpeedButton;

        private ReMenuToggle _flyToggle;
        private ReMenuToggle _noclipToggle;
        private ReMenuToggle _hotkeyToggle;

        public FlyComponent()
        {
            FlySpeed = new ConfigValue<float>(nameof(FlySpeed), 4);
            FlySpeed.OnValueChanged += () => _flySpeedButton.Text = $"Fly Speed: {FlySpeed}";
            EnableFlyHotkey = new ConfigValue<bool>(nameof(EnableFlyHotkey), true);
            EnableFlyHotkey.OnValueChanged += () => _hotkeyToggle.Toggle(EnableFlyHotkey);

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                if (_flyToggle != null)
                {
                    _flyToggle.Interactable = allowed;
                }
                if (_noclipToggle != null)
                {
                    _noclipToggle.Interactable = allowed;
                }

                if (!allowed)
                {
                    ToggleNoclip(false);
                }
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var movementMenu = uiManager.MainMenu.GetMenuPage("Movement");
            var hotkeyMenu = uiManager.MainMenu.GetMenuPage("Hotkeys");

            _flyToggle = movementMenu.AddToggle("Fly", "Enable/Disable Fly", ToggleFly, _flyEnabled);
            _noclipToggle = movementMenu.AddToggle("Noclip", "Enable/Disable Noclip", ToggleNoclip, _noclipEnabled);
            _hotkeyToggle = hotkeyMenu.AddToggle("Fly Hotkey", "Enable/Disable fly hotkey",
                EnableFlyHotkey.SetValue, EnableFlyHotkey);

            _flySpeedButton = movementMenu.AddButton($"Fly Speed: {FlySpeed}", "Adjust your speed when flying", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set fly speed", FlySpeed.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!float.TryParse(s, out var flySpeed))
                            return;

                        FlySpeed.SetValue(flySpeed);
                    }, null);
            }, ResourceManager.Instance.GetSprite("speed"));
        }

        private readonly List<Il2CppSystem.Type> _blacklistedComponents = new List<Il2CppSystem.Type>
        {
            Il2CppType.Of<PlayerSelector>(),
            Il2CppType.Of<VRC_Pickup>(),
            Il2CppType.Of<QuickMenu>(),
            Il2CppType.Of<VRC_Station>(),
            Il2CppType.Of<VRC_AvatarPedestal>(),
            Il2CppType.Of<VRC_UiShape>(),
            Il2CppType.Of<VRCUiShape>()
        };

        private void ToggleNoclipObjects()
        {
            var player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            if (player == null)
                return;

            var colliders = Object.FindObjectsOfType<Collider>();
            var ownCollider = player.GetComponents<Collider>().FirstOrDefault();

            foreach (var collider in colliders)
            {
                if (_blacklistedComponents.Any(comp => collider.GetComponent(comp) != null))
                {
                    continue;
                }

                if (collider == ownCollider)
                    continue;
                
                if (!(_noclipEnabled && collider.enabled || !_noclipEnabled && _disabledColliders.Contains(collider.GetInstanceID())))
                    continue;

                collider.enabled = !_noclipEnabled;
                if (_noclipEnabled)
                {
                    _disabledColliders.Add(collider.GetInstanceID());
                }
            }
        }

        private void ToggleFly(bool value)
        {
            _flyEnabled = value;
            _flyToggle?.Toggle(value);

            if (_flyEnabled)
            {
                if (Physics.gravity == Vector3.zero) return;
                
                _originalGravity = Physics.gravity;
                Physics.gravity = Vector3.zero;
            }
            else
            {
                if (_noclipEnabled) ToggleNoclip(false);

                if (_originalGravity == Vector3.zero) return;

                Physics.gravity = _originalGravity;
                _originalGravity = Vector3.zero;
            }
        }

        private void ToggleNoclip(bool value)
        {
            _noclipEnabled = value;
            _noclipToggle?.Toggle(value);
            if (_noclipEnabled && !_flyEnabled)
            {
                ToggleFly(true);
            }

            ToggleNoclipObjects();
        }

        private void HandleHotkeys()
        {
            if (!EnableFlyHotkey) return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
            {
                if (!_flyEnabled)
                {
                    ToggleNoclip(true);
                }
                else
                {
                    ToggleFly(false);
                }
            }
        }

        public override void OnUpdate()
        {
            if (!RiskyFunctionsManager.Instance.RiskyFunctionAllowed)
                return;

            HandleHotkeys();
            HandleFly();
        }

        private VRCMotionState _motionState;
        private void HandleFly()
        {
            var player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            if (player == null)
                return;

            if (_motionState == null)
            {
                _motionState = player.GetComponent<VRCMotionState>();
            }

            if (!_flyEnabled) return;

            var playerTransform = player.transform;
            if (XRDevice.isPresent)
            {
                playerTransform.position += playerTransform.forward * Time.deltaTime * Input.GetAxis("Vertical") * FlySpeed;
                playerTransform.position += playerTransform.right * Time.deltaTime * Input.GetAxis("Horizontal") * FlySpeed;
                playerTransform.position += new Vector3(0f, Time.deltaTime * Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical") * FlySpeed);
            }
            else
            {
                var speed = Input.GetKey(KeyCode.LeftShift) ? FlySpeed * 2 : FlySpeed;
                var camera = Camera.main.transform;
                playerTransform.position += camera.forward * Time.deltaTime * Input.GetAxis("Vertical") * speed;
                playerTransform.position += camera.right * Time.deltaTime * Input.GetAxis("Horizontal") * speed;

                if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Q))
                {
                    playerTransform.position -= new Vector3(0f, Time.deltaTime * speed, 0f);
                }

                if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.E))
                {
                    playerTransform.position += new Vector3(0f, Time.deltaTime * speed, 0f);
                }
            }
            
            _motionState?.Reset();
        }
    }
}
