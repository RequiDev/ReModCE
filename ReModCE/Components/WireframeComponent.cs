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

        public WireframeComponent()
        {
            WireframeEnabled = new ConfigValue<bool>(nameof(WireframeEnabled), false);
            WireframeEnabled.OnValueChanged += () =>
            {
                _wireframeToggle.Toggle(WireframeEnabled);
                _wireframeCamera.enabled = WireframeEnabled;

                RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
                {
                    _wireframeToggle.Interactable = allowed;
                    if (!allowed)
                        WireframeEnabled.SetValue(false);
                };
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
        }


        private static Camera CreateCamera()
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
            camera.nearClipPlane /= 4f;

            return camera;
        }
    }
}
