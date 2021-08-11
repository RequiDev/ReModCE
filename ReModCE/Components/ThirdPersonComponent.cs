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

            _cameraBack = CreateCamera(Vector3.zero, 75f);
            _cameraFront = CreateCamera(new Vector3(0f, 180f, 0f), 75f);

            _cameraBack.GetComponent<Camera>().enabled = false;
            _cameraFront.GetComponent<Camera>().enabled = false;
            _referenceCamera.GetComponent<Camera>().enabled = true;
        }

        private GameObject CreateCamera(Vector3 rotation, float fieldOfView)
        {
            var cameraObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(cameraObject.GetComponent<MeshRenderer>());
            cameraObject.transform.localScale = _referenceCamera.transform.localScale;
            var frontRigidbody = cameraObject.AddComponent<Rigidbody>();
            frontRigidbody.isKinematic = true;
            frontRigidbody.useGravity = false;

            if (cameraObject.GetComponent<Collider>())
            {
                cameraObject.GetComponent<Collider>().enabled = false;
            }

            cameraObject.GetComponent<Renderer>().enabled = false;
            cameraObject.AddComponent<Camera>();
            cameraObject.transform.parent = _referenceCamera.transform;
            cameraObject.transform.rotation = _referenceCamera.transform.rotation;
            cameraObject.transform.Rotate(rotation);
            cameraObject.transform.position = _referenceCamera.transform.position;
            cameraObject.transform.position += -cameraObject.transform.forward * 2f;
            _referenceCamera.GetComponent<Camera>().enabled = false;
            cameraObject.GetComponent<Camera>().fieldOfView = fieldOfView;
            cameraObject.GetComponent<Camera>().nearClipPlane /= 4f;

            return cameraObject;
        }


        private void HandleHotkeys()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            {
                if (++_cameraSetup > ThirdPersonMode.Front)
                {
                    _cameraSetup = ThirdPersonMode.Off;
                }

                switch (_cameraSetup)
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
            if (_cameraBack == null || _cameraFront == null)
            {
                return;
            }

            HandleHotkeys();
            HandleThirdperson();
        }
    }
}