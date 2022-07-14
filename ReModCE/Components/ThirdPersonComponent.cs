using System;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.Managers;
using UnityEngine;
using CameraTakePhotoEnumerator = VRC.UserCamera.CameraUtil._TakeScreenShot_d__5;

namespace ReModCE.Components
{
    internal enum ThirdPersonMode
    {
        Off = 0,
        Back,
        Front
    }

    internal class ThirdPersonComponent : ModComponent
    {

        private ConfigValue<bool> EnableThirdpersonHotkey;
        private ReMenuToggle _hotkeyToggle;
        private ConfigValue<KeyCode> ThirdpersonHotkey;

        private static ThirdPersonMode _cameraSetup;
        
        private static Camera _thirdPersonCamera;
        private Camera _referenceCamera;
        private Transform _cameraParentTransform;
        
        private const int DefaultCullingMask = 7858963;
        private readonly int UiMenuLayer;
        
        private ConfigValue<bool> ThirdPersonNameplates = new("ThirdPersonNameplates", false, "Third Person Nameplates");

        public ThirdPersonComponent()
        {
            EnableThirdpersonHotkey = new ConfigValue<bool>(nameof(EnableThirdpersonHotkey), true);
            EnableThirdpersonHotkey.OnValueChanged += () => _hotkeyToggle.Toggle(EnableThirdpersonHotkey);

            ThirdpersonHotkey = new ConfigValue<KeyCode>(nameof(ThirdpersonHotkey), KeyCode.T);

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                if (!allowed)
                {
                    SetThirdPersonMode(ThirdPersonMode.Off);
                }
            };

            UiMenuLayer = LayerMask.NameToLayer("UiMenu");

            ReModCE.Harmony.Patch(typeof (CameraTakePhotoEnumerator).GetMethod("MoveNext"), 
                GetLocalPatch(nameof(CameraEnumeratorMoveNextPatch)));
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var hotkeyMenu = uiManager.MainMenu.GetMenuPage("Hotkeys");
            _hotkeyToggle = hotkeyMenu.AddToggle("Thirdperson Hotkey", "Enable/Disable thirdperson hotkey", EnableThirdpersonHotkey.SetValue, EnableThirdpersonHotkey);

            var cameraObject = GameObject.Find("Camera (eye)");

            if (cameraObject == null)
            {
                cameraObject = GameObject.Find("CenterEyeAnchor");

                if (cameraObject == null)
                {
                    return;
                }
            }

            _cameraParentTransform = cameraObject.transform;
            
            _referenceCamera = cameraObject.GetComponent<Camera>();
            if (_referenceCamera == null)
                return;
            
            _thirdPersonCamera = CreateCamera(Vector3.zero, 75f);

            ThirdPersonNameplates.OnValueChanged += () =>
                AddOrRemoveLayerFromCameraCullingMask(ThirdPersonNameplates.Value, _thirdPersonCamera, UiMenuLayer);
                
            AddOrRemoveLayerFromCameraCullingMask(ThirdPersonNameplates.Value, _thirdPersonCamera, UiMenuLayer);
        }

        private Camera CreateCamera(Vector3 rotation, float fieldOfView)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.transform.localScale = _referenceCamera.transform.localScale;
            cameraObject.transform.parent = _referenceCamera.transform;
            cameraObject.transform.localEulerAngles = _referenceCamera.transform.localEulerAngles + new Vector3(0, 180, 0);
            cameraObject.transform.position = _referenceCamera.transform.position + (-cameraObject.transform.forward * 2f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = fieldOfView;
            camera.nearClipPlane /= 4f;

            camera.cullingMask = DefaultCullingMask;

            return camera;
        }

        private void HandleHotkeys()
        {
            if (!EnableThirdpersonHotkey) return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(ThirdpersonHotkey))
            {
                var mode = _cameraSetup;
                if (++mode > ThirdPersonMode.Front)
                {
                    mode = ThirdPersonMode.Off;
                }

                SetThirdPersonMode(mode);
            }
        }

        private void SetThirdPersonMode(ThirdPersonMode mode)
        {
            _cameraSetup = mode;
            switch (mode)
            {
                case ThirdPersonMode.Off:
                    _thirdPersonCamera.enabled = false;
                    break;
                case ThirdPersonMode.Back:
                case ThirdPersonMode.Front:
                    _thirdPersonCamera.enabled = true;
                    _thirdPersonCamera.transform.RotateAround(_cameraParentTransform.position, _cameraParentTransform.up, 180);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleThirdperson()
        {
            if (_cameraSetup == ThirdPersonMode.Off) return;

            var scrollwheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollwheel > 0f)
            {
                _thirdPersonCamera.transform.position += _thirdPersonCamera.transform.forward * 0.1f;
            }
            else if (scrollwheel < 0f)
            {
                _thirdPersonCamera.transform.position -= _thirdPersonCamera.transform.forward * 0.1f;
            }
        }

        public override void OnUpdate()
        {
            if (!RiskyFunctionsManager.Instance.RiskyFunctionAllowed)
                return;

            if (_thirdPersonCamera == null)
                return;

            HandleHotkeys();
            HandleThirdperson();
        }

        private static void CameraEnumeratorMoveNextPatch(ref CameraTakePhotoEnumerator __instance)
        {
            if(_cameraSetup == ThirdPersonMode.Off)
                return;

            __instance.field_Public_Camera_0 = _thirdPersonCamera;
        }

        private void AddOrRemoveLayerFromCameraCullingMask(bool add, Camera camera, int layer)
        {
            camera.cullingMask = add ? camera.cullingMask |= (1 << layer) : camera.cullingMask &= ~(1 << layer);
        }
    }
}
