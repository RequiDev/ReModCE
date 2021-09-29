using System;
using System.Collections;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ReModCE.Components
{
    public class WireframeEnabler : MonoBehaviour
    {
        public WireframeEnabler(IntPtr obj) : base(obj) { }

        public void OnPreRender()
        {
            GL.wireframe = true;
        }

        public void OnPostRender()
        {
            GL.wireframe = false;
        }
    }

    internal class WireframeComponent : ModComponent
    {
        private Camera _wireframeCamera;
        private Camera _originalCamera;

        private ConfigValue<bool> WireframeEnabled;
        private ReQuickToggle _wireframeToggle;

        private ConfigValue<bool> WireframeIgnoreZ;
        private ReQuickToggle _wireframeIgnoreZToggle;

        private ConfigValue<bool> WireframeIncludePlayers;
        private ReQuickToggle _includePlayersToggle;

        private ConfigValue<bool> WireframeIncludeSelf;
        private ReQuickToggle _includeSelfToggle;

        private ConfigValue<bool> WireframeIncludeDefault;
        private ReQuickToggle _includeWorldToggle;

        private ConfigValue<bool> WireframeIncludePickups;
        private ReQuickToggle _includePickupsToggle;

        private ConfigValue<float> WireframeRange;
        private ReQuickButton _rangeButton;

        private ConfigValue<bool> WireframeHideOriginalObjects;
        private ReQuickToggle _hideOriginalObjectsToggle;

        public WireframeComponent()
        {
            WireframeEnabled = new ConfigValue<bool>(nameof(WireframeEnabled), false);
            WireframeEnabled.OnValueChanged += () =>
            {
                _wireframeToggle.Toggle(WireframeEnabled);
                _wireframeCamera.enabled = WireframeEnabled;

                if (!WireframeEnabled)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                }
                else
                {
                    if (!WireframeIncludePlayers)
                    {
                        _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    }
                    if (!WireframeIncludeSelf)
                    {
                        _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    }
                    if (!WireframeIncludeDefault)
                    {
                        _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    }
                    if (!WireframeIncludePickups)
                    {
                        _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    }
                }
            };

            WireframeIgnoreZ = new ConfigValue<bool>(nameof(WireframeIgnoreZ), true);
            WireframeIgnoreZ.OnValueChanged += () =>
            {
                _wireframeIgnoreZToggle.Toggle(WireframeIgnoreZ);
                _wireframeCamera.clearFlags = WireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            };

            WireframeHideOriginalObjects = new ConfigValue<bool>(nameof(WireframeHideOriginalObjects), false);
            WireframeHideOriginalObjects.OnValueChanged += () =>
            {
                _hideOriginalObjectsToggle.Toggle(WireframeHideOriginalObjects);
                if (!WireframeHideOriginalObjects)
                {
                    if (WireframeIncludePlayers)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                    }

                    if (WireframeIncludeSelf)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                    }

                    if (WireframeIncludeDefault)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                    }

                    if (WireframeIncludePickups)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                    }
                }
                else
                {
                    if (WireframeIncludePlayers)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    }

                    if (WireframeIncludeSelf)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    }

                    if (WireframeIncludeDefault)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    }

                    if (WireframeIncludePickups)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    }
                }
            };

            WireframeIncludePlayers = new ConfigValue<bool>(nameof(WireframeIncludePlayers), true);
            WireframeIncludePlayers.OnValueChanged += () =>
            {
                _includePlayersToggle.Toggle(WireframeIncludePlayers);
                if (WireframeIncludePlayers)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    }
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                    }
                }
            };

            WireframeIncludeSelf = new ConfigValue<bool>(nameof(WireframeIncludeSelf), true);
            WireframeIncludeSelf.OnValueChanged += () =>
            {
                _includeSelfToggle.Toggle(WireframeIncludeSelf);
                if (WireframeIncludeSelf)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    }
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                    }
                }
            };

            WireframeIncludeDefault = new ConfigValue<bool>(nameof(WireframeIncludeDefault), false);
            WireframeIncludeDefault.OnValueChanged += () =>
            {
                _includeWorldToggle.Toggle(WireframeIncludeDefault);
                if (WireframeIncludeDefault)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    }
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                    }
                }
            };

            WireframeIncludePickups = new ConfigValue<bool>(nameof(WireframeIncludePickups), false);
            WireframeIncludePickups.OnValueChanged += () =>
            {
                _includePickupsToggle.Toggle(WireframeIncludePickups);
                if (WireframeIncludePickups)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    }
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    if (WireframeHideOriginalObjects)
                    {
                        _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                    }
                }
            };

            WireframeRange = new ConfigValue<float>(nameof(WireframeRange), 100f);
            WireframeRange.OnValueChanged += () =>
            {
                _rangeButton.Text = $"Range: {WireframeRange}";
                _wireframeCamera.farClipPlane = WireframeRange;
            };

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                _wireframeToggle.Interactable = allowed;
                if (!allowed)
                    WireframeEnabled.SetValue(false);
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _wireframeCamera = CreateCamera();
            if (_wireframeCamera == null)
                return;

            _wireframeCamera.enabled = WireframeEnabled;
            _wireframeCamera.gameObject.AddComponent<WireframeEnabler>();
            _wireframeCamera.cullingMask = 0;

            if (WireframeIncludePlayers)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
            }

            if (WireframeIncludeSelf)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
            }

            if (WireframeIncludeDefault)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
            }

            if (WireframeIncludePickups)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
            }

            _wireframeCamera.clearFlags = WireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            _wireframeCamera.farClipPlane = WireframeRange;

            var menu = uiManager.MainMenu.GetSubMenu("Visuals").AddSubMenu("Wireframe", "Access wireframe settings");
            _wireframeToggle = menu.AddToggle("Enable", "Highlight objects using wireframe.",
                WireframeEnabled.SetValue, WireframeEnabled);

            _rangeButton = menu.AddButton($"Range: {WireframeRange}",
                "Set the range on when wireframe starts rendering",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set wireframe range",
                        $"{WireframeRange}", InputField.InputType.Standard, false, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!float.TryParse(s, out var range))
                                return;

                            WireframeRange.SetValue(range);
                        }, null);
                });

            _hideOriginalObjectsToggle = menu.AddToggle("Hide Original", "Hide original meshes so only the wireframe shows",
                    WireframeHideOriginalObjects.SetValue, WireframeHideOriginalObjects);

                _wireframeIgnoreZToggle = menu.AddToggle("Ignore Z", "Enable/Disable Ignore Z (Visible through walls)",
                WireframeIgnoreZ.SetValue, WireframeIgnoreZ);
            _includePlayersToggle = menu.AddToggle("Include Players", "Include players in wireframe ESP",
                WireframeIncludePlayers.SetValue, WireframeIncludePlayers);
            _includeSelfToggle = menu.AddToggle("Include Self", "Include yourself in wireframe ESP",
                WireframeIncludeSelf.SetValue, WireframeIncludeSelf);
            _includeWorldToggle = menu.AddToggle("Include Default/World", "Include default layer stuff like the world in wireframe ESP",
                WireframeIncludeDefault.SetValue, WireframeIncludeDefault);
            _includePickupsToggle = menu.AddToggle("Include Pickups", "Include pickups in wireframe ESP",
                WireframeIncludePickups.SetValue, WireframeIncludePickups);

        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (_wireframeCamera == null)
                return;

            switch (buildIndex)
            {
                case 0:
                case 1:
                    break;
                default:
                    MelonCoroutines.Start(FixCameraDelayed());
                    break;
            }
        }

        private IEnumerator FixCameraDelayed()
        {
            yield return new WaitForSecondsRealtime(5f);

            _wireframeCamera.clearFlags = WireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            _wireframeCamera.farClipPlane = WireframeRange;
            Object.DestroyImmediate(_wireframeCamera.GetComponent<PostProcessLayer>()); // make sure we don't double or PostProcessing

            if (WireframeHideOriginalObjects)
            {
                if (WireframeIncludePlayers)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                }

                if (WireframeIncludeSelf)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                }

                if (WireframeIncludeDefault)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                }

                if (WireframeIncludePickups)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                }
            }
        }

        private Camera CreateCamera()
        {
            var refCam = GameObject.Find("Camera (eye)");
            if (refCam == null)
            {
                refCam = GameObject.Find("CenterEyeAnchor");

                if (refCam == null)
                {
                    return null;
                }
            }

            _originalCamera = refCam.GetComponent<Camera>();
            if (_originalCamera == null)
                return null;

            var cameraObject = new GameObject($"WireframeCamera");
            cameraObject.transform.localScale = _originalCamera.transform.localScale;
            cameraObject.transform.parent = _originalCamera.transform;
            cameraObject.transform.rotation = _originalCamera.transform.rotation;
            cameraObject.transform.position = _originalCamera.transform.position;

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = _originalCamera.fieldOfView;
            camera.nearClipPlane /= 4f;

            return camera;
        }
    }
}
