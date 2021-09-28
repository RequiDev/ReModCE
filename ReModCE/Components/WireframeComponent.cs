using System;
using System.Collections;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
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

        public WireframeComponent()
        {
            WireframeEnabled = new ConfigValue<bool>(nameof(WireframeEnabled), false);
            WireframeEnabled.OnValueChanged += () =>
            {
                _wireframeToggle.Toggle(WireframeEnabled);
                _wireframeCamera.enabled = WireframeEnabled;
            };

            WireframeIgnoreZ = new ConfigValue<bool>(nameof(WireframeIgnoreZ), true);
            WireframeIgnoreZ.OnValueChanged += () =>
            {
                _wireframeIgnoreZToggle.Toggle(WireframeIgnoreZ);
                _wireframeCamera.clearFlags = WireframeIgnoreZ ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
            };

            WireframeIncludePlayers = new ConfigValue<bool>(nameof(WireframeIncludePlayers), true);
            WireframeIncludePlayers.OnValueChanged += () =>
            {
                _includePlayersToggle.Toggle(WireframeIncludePlayers);
                if (WireframeIncludePlayers)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                }
            };

            WireframeIncludeSelf = new ConfigValue<bool>(nameof(WireframeIncludeSelf), true);
            WireframeIncludeSelf.OnValueChanged += () =>
            {
                _includeSelfToggle.Toggle(WireframeIncludeSelf);
                if (WireframeIncludeSelf)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
                }
            };

            WireframeIncludeDefault = new ConfigValue<bool>(nameof(WireframeIncludeDefault), false);
            WireframeIncludeDefault.OnValueChanged += () =>
            {
                _includeWorldToggle.Toggle(WireframeIncludeDefault);
                if (WireframeIncludeDefault)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
                }
            };

            WireframeIncludePickups = new ConfigValue<bool>(nameof(WireframeIncludePickups), false);
            WireframeIncludePickups.OnValueChanged += () =>
            {
                _includePickupsToggle.Toggle(WireframeIncludePickups);
                if (WireframeIncludePickups)
                {
                    _wireframeCamera.cullingMask |= 1 << LayerMask.NameToLayer("Pickup");
                }
                else
                {
                    _wireframeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Pickup"));
                }
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
            
            var subMenu = uiManager.MainMenu.GetSubMenu("Visuals").AddSubMenu("Wireframe", "Access wireframe settings");
            _wireframeToggle = subMenu.AddToggle("Enable", "Highlight objects using wireframe.",
                WireframeEnabled.SetValue, WireframeEnabled);
            _wireframeIgnoreZToggle = subMenu.AddToggle("Ignore Z", "Enable/Disable Ignore Z (Visible through walls)",
                WireframeIgnoreZ.SetValue, WireframeIgnoreZ);
            _includePlayersToggle = subMenu.AddToggle("Include Players", "Include players in wireframe ESP",
                WireframeIncludePlayers.SetValue, WireframeIncludePlayers);
            _includeSelfToggle = subMenu.AddToggle("Include Self", "Include yourself in wireframe ESP",
                WireframeIncludeSelf.SetValue, WireframeIncludeSelf);
            _includeWorldToggle = subMenu.AddToggle("Include Default/World", "Include default layer stuff like the world in wireframe ESP",
                WireframeIncludeDefault.SetValue, WireframeIncludeDefault);
            _includePickupsToggle = subMenu.AddToggle("Include Pickups", "Include pickups in wireframe ESP",
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
            Object.DestroyImmediate(_wireframeCamera.GetComponent<PostProcessLayer>()); // make sure we don't double or PostProcessing
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

            var referenceCamera = refCam.GetComponent<Camera>();
            if (referenceCamera == null)
                return null;

            var cameraObject = new GameObject($"WireframeCamera");
            cameraObject.transform.localScale = referenceCamera.transform.localScale;
            cameraObject.transform.parent = referenceCamera.transform;
            cameraObject.transform.rotation = referenceCamera.transform.rotation;
            cameraObject.transform.position = referenceCamera.transform.position;

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = referenceCamera.fieldOfView;
            camera.nearClipPlane /= 4f;

            return camera;
        }
    }
}
