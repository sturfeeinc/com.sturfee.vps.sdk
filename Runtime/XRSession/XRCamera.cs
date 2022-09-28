using UnityEngine;
using System.Collections;
using SturfeeVPS.Core;

namespace SturfeeVPS.SDK
{
    public class XRCamera : MonoBehaviour
    {
        private static XRCamera _instance;

        [SerializeField]
        private Camera _camera;

        public static XRPose Pose
        {
            get
            {
                return new XRPose()
                {
                    GeoLocation = XRSessionManager.GetSession().GetXRCameraLocation(),
                    Position = Camera.transform.position,
                    Rotation = Camera.transform.rotation
                };
            }
        }

        public static Camera Camera
        {
            get
            {
                return _instance._camera;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                _instance = null;
            }

            _instance = this;

            _camera.cullingMask |= 1 << LayerMask.NameToLayer("sturfeeBuilding");
            _camera.cullingMask |= 1 << LayerMask.NameToLayer("sturfeeTerrain");
            _camera.cullingMask &= ~(1 << LayerMask.NameToLayer("sturfeeBackground"));
        }

        private void OnEnable()
        {
            Application.onBeforeRender += onBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= onBeforeRender;
        }

        private void onBeforeRender()
        {
            Update();
        }

        private void Update()
        {
            var xrSession = XRSessionManager.GetSession();

            if (xrSession == null || xrSession.Status < XRSessionStatus.Ready)
            {
                return;
            }

            // update local Pose of camera based on sensor readings(PoseProvider)
            var sensorPosition = XRSessionManager.GetSession().PoseProvider.GetPosition();
            var sensorRotation = XRSessionManager.GetSession().PoseProvider.GetOrientation();
            _camera.transform.localPosition = Converters.WorldToUnityPosition(sensorPosition);
            _camera.transform.localRotation = Converters.WorldToUnityRotation(sensorRotation);

            // update projection matrix based on sensor readings (VideoProvider)
            _camera.projectionMatrix =
                XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix();

            ApplyOffsets();
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void ApplyOffsets()
        {
            // position
            transform.position = Converters.WorldToUnityPosition(
                XRSessionManager.GetSession().GetLocationOffset());

            // rotation       
            // get rotation at origin in world coordinate system
            Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);

            // offset recieved from XRSession
            Quaternion worldOffset = XRSessionManager.GetSession().GetOrientationOffset();

            // Apply offset to worldOrgin and convert the result into Unity Coordinate System
            transform.rotation = Converters.WorldToUnityRotation(
            worldOffset * worldOrigin);
        }
    }
}