using System;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;

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

            WireframeIncludeDefault = new ConfigValue<bool>(nameof(WireframeIncludeDefault), true);
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

            WireframeIncludePickups = new ConfigValue<bool>(nameof(WireframeIncludePickups), true);
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

            var menu = uiManager.MainMenu.GetSubMenu("Visuals");
            _wireframeToggle = menu.AddToggle("Wireframe", "Highlight players using wireframe.",
                WireframeEnabled.SetValue, WireframeEnabled);
            _includeSelfToggle = menu.AddToggle("Include Self (Wireframe)", "Include yourself in wireframe ESP",
                WireframeIncludeSelf.SetValue, WireframeIncludeSelf);
            _includeWorldToggle = menu.AddToggle("Include Default/World (Wireframe)", "Include default layer stuff like the world in wireframe ESP",
                WireframeIncludeDefault.SetValue, WireframeIncludeDefault);
            _includePickupsToggle = menu.AddToggle("Include Pickups (Wireframe)", "Include pickups in wireframe ESP",
                WireframeIncludePickups.SetValue, WireframeIncludePickups);
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
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.cullingMask = 1 << LayerMask.NameToLayer("Player");

            if (WireframeIncludeSelf)
            {
                camera.cullingMask |= 1 << LayerMask.NameToLayer("PlayerLocal");
            }
            else
            {
                camera.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerLocal"));
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

            camera.nearClipPlane /= 4f;

            return camera;
        }
    }
}
