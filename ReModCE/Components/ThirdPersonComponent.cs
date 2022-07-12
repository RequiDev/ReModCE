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
        private static Camera _cameraBack;
        private static Camera _cameraFront;
        private Camera _referenceCamera;
        private Camera _photoCamera;

        private static ThirdPersonMode _cameraSetup;

        private ConfigValue<bool> EnableThirdpersonHotkey;
        private ReMenuToggle _hotkeyToggle;

        private ConfigValue<KeyCode> ThirdpersonHotkey;

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
            
            ReModCE.Harmony.Patch(typeof (CameraTakePhotoEnumerator).GetMethod("MoveNext"), 
                GetLocalPatch(nameof(CameraEnumeratorMoveNextPatch)));
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var hotkeyMenu = uiManager.MainMenu.GetMenuPage("Hotkeys");
            _hotkeyToggle = hotkeyMenu.AddToggle("Thirdperson Hotkey", "Enable/Disable thirdperson hotkey", EnableThirdpersonHotkey.SetValue, EnableThirdpersonHotkey);

            var cameraObject = GameObject.Find("Camera (eye)");
            _photoCamera = GameObject.Find("UserCamera")?.transform.Find("PhotoCamera")?.GetComponent<Camera>();

            if (cameraObject == null)
            {
                cameraObject = GameObject.Find("CenterEyeAnchor");

                if (cameraObject == null)
                {
                    return;
                }
            }
            
            _referenceCamera = cameraObject.GetComponent<Camera>();
            if (_referenceCamera == null)
                return;

            _cameraBack = CreateCamera(ThirdPersonMode.Back, Vector3.zero, 75f);
            _cameraFront = CreateCamera(ThirdPersonMode.Front, new Vector3(0f, 180f, 0f), 75f);
        }

        private Camera CreateCamera(ThirdPersonMode cameraType, Vector3 rotation, float fieldOfView)
        {
            var cameraObject = new GameObject($"{cameraType}Camera");
            cameraObject.transform.localScale = _referenceCamera.transform.localScale;
            cameraObject.transform.parent = _referenceCamera.transform;
            cameraObject.transform.rotation = _referenceCamera.transform.rotation;
            cameraObject.transform.Rotate(rotation);
            cameraObject.transform.position = _referenceCamera.transform.position + (-cameraObject.transform.forward * 2f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = fieldOfView;
            camera.nearClipPlane /= 4f;

            if(_photoCamera != null)
                camera.cullingMask = _photoCamera.cullingMask;

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
                    _cameraBack.enabled = false;
                    _cameraFront.enabled = false;
                    break;
                case ThirdPersonMode.Back:
                    _cameraBack.enabled = true;
                    _cameraFront.enabled = false;
                    break;
                case ThirdPersonMode.Front:
                    _cameraBack.enabled = false;
                    _cameraFront.enabled = true;
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
                _cameraBack.transform.position += _cameraBack.transform.forward * 0.1f;
                _cameraFront.transform.position -= _cameraBack.transform.forward * 0.1f;
            }
            else if (scrollwheel < 0f)
            {
                _cameraBack.transform.position -= _cameraBack.transform.forward * 0.1f;
                _cameraFront.transform.position += _cameraBack.transform.forward * 0.1f;
            }
        }

        public override void OnUpdate()
        {
            if (!RiskyFunctionsManager.Instance.RiskyFunctionAllowed)
                return;

            if (_cameraBack == null || _cameraFront == null)
            {
                return;
            }

            HandleHotkeys();
            HandleThirdperson();
        }

        private static void CameraEnumeratorMoveNextPatch(ref CameraTakePhotoEnumerator __instance)
        {
            if(_cameraSetup == ThirdPersonMode.Off)
                return;

            __instance.field_Public_Camera_0 = _cameraSetup == ThirdPersonMode.Back ? _cameraBack : _cameraFront;
        }
    }
}