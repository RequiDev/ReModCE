using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private GameObject _cameraBack;
        private GameObject _cameraFront;
        private GameObject _referenceCamera;

        private ThirdPersonMode _cameraSetup;

        public ThirdPersonComponent()
        {
            RiskyFunctionsManager.OnRiskyFunctionsChanged += allowed =>
            {
                if (!allowed)
                {
                    SetThirdPersonMode(ThirdPersonMode.Off);
                }
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var backCameraObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(backCameraObject.GetComponent<MeshRenderer>());
            _referenceCamera = GameObject.Find("Camera (eye)");
            if (_referenceCamera == null)
            {
                _referenceCamera = GameObject.Find("CenterEyeAnchor");
            }

            if (_referenceCamera == null)
            {
                return;
            }

            _cameraBack = CreateCamera(ThirdPersonMode.Back, Vector3.zero, 75f);
            _cameraFront = CreateCamera(ThirdPersonMode.Front, new Vector3(0f, 180f, 0f), 75f);
        }

        private GameObject CreateCamera(ThirdPersonMode cameraType, Vector3 rotation, float fieldOfView)
        {
            var cameraObject = new GameObject($"{cameraType}Camera");
            cameraObject.transform.localScale = _referenceCamera.transform.localScale;
            
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            cameraObject.transform.parent = _referenceCamera.transform;
            cameraObject.transform.rotation = _referenceCamera.transform.rotation;
            cameraObject.transform.Rotate(rotation);
            cameraObject.transform.position = _referenceCamera.transform.position + (-cameraObject.transform.forward * 2f);
            camera.fieldOfView = fieldOfView;
            camera.nearClipPlane /= 4f;

            return cameraObject;
        }


        private void HandleHotkeys()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
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
                    _cameraBack.GetComponent<Camera>().enabled = false;
                    _cameraFront.GetComponent<Camera>().enabled = false;
                    break;
                case ThirdPersonMode.Back:
                    _cameraBack.GetComponent<Camera>().enabled = true;
                    _cameraFront.GetComponent<Camera>().enabled = false;
                    break;
                case ThirdPersonMode.Front:
                    _cameraBack.GetComponent<Camera>().enabled = false;
                    _cameraFront.GetComponent<Camera>().enabled = true;
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
            if (!RiskyFunctionsManager.RiskyFunctionAllowed)
                return;

            if (_cameraBack == null || _cameraFront == null)
            {
                return;
            }

            HandleHotkeys();
            HandleThirdperson();
        }
    }
}