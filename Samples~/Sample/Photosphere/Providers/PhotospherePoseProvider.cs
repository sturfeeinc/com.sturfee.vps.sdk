using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK.Examples
{
    public class PhotospherePoseProvider : BasePoseProvider
    {
        public float MouseSensitivity = 100.0f;

        private float rotY = 0.0f; // rotation around the up/y axis
        private float rotX = 0.0f; // rotation around the right/x axis

        private float _clampAngle = 80.0f;
        private Quaternion _rotation = Quaternion.identity;

        public override void OnRegister()
        {
            Vector3 rot = _rotation.eulerAngles;
            rotY = rot.y;
            rotX = rot.x;

            Input.gyro.enabled = true;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = -Input.GetAxis("Mouse Y");

                rotY += mouseX * MouseSensitivity * Time.deltaTime;
                rotX += mouseY * MouseSensitivity * Time.deltaTime;

                rotX = Mathf.Clamp(rotX, -_clampAngle, _clampAngle);

                _rotation = Quaternion.Euler(rotX, rotY, 0.0f);
            }
#else
            _rotation = Converters.WorldToUnityRotation(Input.gyro.attitude);
#endif
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }

        public override float GetHeightFromGround()
        {
            return 1.5f;
        }

        public override Vector3 GetPosition(out bool includesElevation)
        {
            includesElevation = false;
            return Converters.UnityToWorldPosition(Vector3.zero);
        }

        public override Quaternion GetRotation()
        {
            return Converters.UnityToWorldRotation(_rotation);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            string guiText = " Hold Right mouse button to move ";

            GUIStyle style = new GUIStyle
            {
                fontSize = 40,
                fontStyle = FontStyle.Bold
            };

            style.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width/2 - 300, Screen.height - 100, 400, 400), guiText, style);
        }
#endif
    }
}