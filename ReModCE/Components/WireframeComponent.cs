using System;
using System.Collections;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Managers;
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

        private bool _wireframeEnabled;
        private ReMenuToggle _wireframeToggle;

        private bool _wireframeIgnoreZ;
        private bool _wireframeIncludePlayers;
        private bool _wireframeIncludeSelf;
        private bool _wireframeIncludeDefault;
        private bool _wireframeIncludePickups;
        private bool _wireframeHideOriginalObjects;

        private ConfigValue<float> WireframeRange;
        private ReMenuButton _rangeButton;

        public WireframeComponent()
        {
            _wireframeIgnoreZ = true;
            _wireframeIncludePlayers = true;
            _wireframeIncludeSelf = true;

            WireframeRange = new ConfigValue<float>(nameof(WireframeRange), 100f);
            WireframeRange.OnValueChanged += () =>
            {
                _rangeButton.Text = $"Range: {WireframeRange}";
                _wireframeCamera.farClipPlane = WireframeRange;
            };

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                if (_wireframeToggle != null)
                {
                    _wireframeToggle.Interactable = allowed;
                }
                if (!allowed)
                    ToggleWireframe(false);
            };
        }

        public override void OnUiManagerInitEarly()
        {
            _wireframeCamera = CreateCamera();
            if (_wireframeCamera == null)
                return;

            _wireframeCamera.enabled = _wireframeEnabled;
            _wireframeCamera.gameObject.AddComponent<WireframeEnabler>();
            _wireframeCamera.cullingMask = 0;

            if (_wireframeIncludePlayers)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
            }

            if (_wireframeIncludeSelf)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
            }

            if (_wireframeIncludeDefault)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
            }

            if (_wireframeIncludePickups)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
            }

            _wireframeCamera.clearFlags = _wireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            _wireframeCamera.farClipPlane = WireframeRange;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Wireframe").AddMenuPage("Wireframe", "Access wireframe settings", ResourceManager.GetSprite("remodce.wireframe"));
            _wireframeToggle = menu.AddToggle("Enable", "Highlight objects using wireframe.", ToggleWireframe, _wireframeEnabled);

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
                }, ResourceManager.GetSprite("remodce.binoculars"));

            menu.AddToggle("Hide Original", "Hide original meshes so only the wireframe shows",
                ToggleHideOriginalObject, _wireframeHideOriginalObjects);

            menu.AddToggle("Ignore Z", "Enable/Disable Ignore Z (Visible through walls)",
                b =>
                {
                    _wireframeIgnoreZ = b;
                    _wireframeCamera.clearFlags = _wireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
                }, _wireframeIgnoreZ);
            menu.AddToggle("Include Players", "Include players in wireframe ESP",
                ToggleIncludePlayers, _wireframeIncludePlayers);
            menu.AddToggle("Include Self", "Include yourself in wireframe ESP",
                ToggleIncludeSelf, _wireframeIncludeSelf);
            menu.AddToggle("Include Default/World", "Include default layer stuff like the world in wireframe ESP",
                ToggleIncludeDefault, _wireframeIncludeDefault);
            menu.AddToggle("Include Pickups", "Include pickups in wireframe ESP",
                ToggleIncludePickups, _wireframeIncludePickups);

        }

        private void ToggleIncludePickups(bool b)
        {
            _wireframeIncludePickups = b;
            if (_wireframeIncludePickups)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                if (_wireframeHideOriginalObjects && _wireframeEnabled)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                }
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                if (_wireframeHideOriginalObjects)
                {
                    _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                }
            }
        }

        private void ToggleIncludeDefault(bool b)
        {
            _wireframeIncludeDefault = b;
            if (_wireframeIncludeDefault)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                if (_wireframeHideOriginalObjects && _wireframeEnabled)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                }
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                if (_wireframeHideOriginalObjects)
                {
                    _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                }
            }
        }

        private void ToggleIncludeSelf(bool b)
        {
            _wireframeIncludeSelf = b;
            if (_wireframeIncludeSelf)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                if (_wireframeHideOriginalObjects && _wireframeEnabled)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                }
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                if (_wireframeHideOriginalObjects)
                {
                    _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                }
            }
        }

        private void ToggleIncludePlayers(bool b)
        {
            _wireframeIncludePlayers = b;
            if (_wireframeIncludePlayers)
            {
                _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                if (_wireframeHideOriginalObjects && _wireframeEnabled)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                }
            }
            else
            {
                _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                if (_wireframeHideOriginalObjects)
                {
                    _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                }
            }
        }

        private void ToggleHideOriginalObject(bool b)
        {
            _wireframeHideOriginalObjects = b;
            if (!_wireframeHideOriginalObjects)
            {
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
            }
            else
            {
                if (_wireframeEnabled)
                {
                    if (_wireframeIncludePlayers)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    }

                    if (_wireframeIncludeSelf)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    }

                    if (_wireframeIncludeDefault)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    }

                    if (_wireframeIncludePickups)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    }
                }
            }
        }

        private void ToggleWireframe(bool enabled)
        {
            _wireframeToggle.Toggle(enabled, false, true);

            _wireframeEnabled = enabled;
            _wireframeCamera.enabled = _wireframeEnabled;

            if (!_wireframeEnabled)
            {
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                _originalCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
            }
            else
            {
                if (_wireframeHideOriginalObjects)
                {
                    if (_wireframeIncludePlayers)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                    }

                    if (_wireframeIncludeSelf)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                    }

                    if (_wireframeIncludeDefault)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                    }

                    if (_wireframeIncludePickups)
                    {
                        _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                    }
                }
            }
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

            _wireframeCamera.clearFlags = _wireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            _wireframeCamera.farClipPlane = WireframeRange;
            Object.DestroyImmediate(_wireframeCamera.GetComponent<PostProcessLayer>()); // make sure we don't double or PostProcessing

            if (_wireframeEnabled && _wireframeHideOriginalObjects)
            {
                if (_wireframeIncludePlayers)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                }

                if (_wireframeIncludeSelf)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                }

                if (_wireframeIncludeDefault)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                }

                if (_wireframeIncludePickups)
                {
                    _originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                }
            }
        }

        private Camera CreateCamera()
        {
            var vrCamera = VRCVrCamera.field_Private_Static_VRCVrCamera_0;
            if (!vrCamera)
                return null;

            _originalCamera = vrCamera.field_Public_Camera_0;

            if (_originalCamera == null)
                return null;

            var cameraObject = new GameObject($"WireframeCamera");
            cameraObject.transform.localScale = _originalCamera.transform.localScale;
            cameraObject.transform.parent = _originalCamera.transform.parent;
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.transform.localPosition = Vector3.zero;

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = _originalCamera.fieldOfView;
            camera.nearClipPlane /= 4f;
            camera.cameraType = _originalCamera.cameraType;

            return camera;
        }
    }
}
