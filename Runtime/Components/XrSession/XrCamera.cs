using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class XrCamera : MonoBehaviour
    {
        private static XrCamera _instance;

        [SerializeField]
        private Camera _camera;

        public static XRPose Pose
        {
            get
            {
                return new XRPose()
                {
                    GeoLocation = XrSessionManager.GetSession()?.Location,
                    Position = Camera.transform.position,
                    Rotation = Camera.transform.rotation
                };
            }
        }

        public static Camera Camera
        {
            get
            {
                return _instance?._camera;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                _instance = null;
            }

            _instance = this;

            _camera.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.SturgBuilding);
            _camera.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.SturgTerrain);
            
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
            var xrSession = XrSessionManager.GetSession();

            if (xrSession == null)// || xrSession.Status < XRSessionStatus.Ready)
            {
                return;
            }

            // update local Pose of camera based on sensor readings(PoseProvider)
            var poseProvider = xrSession.GetProvider<IPoseProvider>();
            if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var sensorPosition = poseProvider.GetPosition(out _);
                var sensorRotation = poseProvider.GetRotation();
                _camera.transform.localPosition = Converters.WorldToUnityPosition(sensorPosition);
                _camera.transform.localRotation = Converters.WorldToUnityRotation(sensorRotation);
            }

            // update projection matrix based on sensor readings (VideoProvider)
            var videoProvider = xrSession.GetProvider<IVideoProvider>();
            if (videoProvider != null && videoProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                _camera.projectionMatrix = videoProvider.GetProjectionMatrix();
            }

            ApplyOffsets();
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void ApplyOffsets()
        {
            var xrSession = XrSessionManager.GetSession();

            // position
            transform.position = Converters.WorldToUnityPosition(xrSession.PositionOffset);

            // rotation       
            // get rotation at origin in world coordinate system
            Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);

            // offset recieved from XRSession
            Quaternion worldOffset = xrSession.RotationOffset;

            // Apply offset to worldOrgin and convert the result into Unity Coordinate System
            transform.rotation = Converters.WorldToUnityRotation(worldOffset * worldOrigin);
        }
    }
}
