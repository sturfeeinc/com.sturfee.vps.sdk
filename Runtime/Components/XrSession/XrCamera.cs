using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Controls XrCamera of the XR session using positional and rotational information from XrSessionPoseManager. 
    /// </summary>
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
            
            // FOR DEBUG
            SturfeeEventManager.OnDebugButtonPressed += OnDebugButtonPressed;
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

        // Debug
        private bool PrintDebug = false;

        private void OnDebugButtonPressed()
        {
            if (!PrintDebug)
                PrintDebug = true;
            SturfeeDebug.Log("DEBUG EVENTS INITIALIZING");
            Debug.Log("printing from XrCamera.cs");
        }

        private void Update()
        {
            var xrSession = XrSessionManager.GetSession();

            if (xrSession == null)// || xrSession.Status < XRSessionStatus.Ready)
            {
                return;
            }

            // update local Pose of camera based on sensor readings(PoseProvider)
            _camera.transform.localPosition = Converters.WorldToUnityPosition(Position);
            _camera.transform.localRotation = Converters.WorldToUnityRotation(Rotation);         

            // FOR DEBUG
            if (PrintDebug)
                SturfeeDebug.Log($"[XrCamera.cs] [DEBUG BUTTON PRESS] Position: {Position}, Rotation: {Rotation}");   
            
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

        private Vector3 Position
        {
            get
            {
                var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
                if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    return poseProvider.GetPosition(out _);                    
                }

                return Vector3.zero;
            }
        }

        private Quaternion Rotation
        {
            get
            {
                var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
                if (poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    return poseProvider.GetRotation();
                }

                return Converters.UnityToWorldRotation(Quaternion.identity);
            }
        }

        private void ApplyOffsets()
        {
            var xrSession = XrSessionManager.GetSession();

            
            // FOR DEBUG
            if (PrintDebug)
            {
                SturfeeDebug.Log($"[XrCamera.cs] [DEBUG BUTTON PRESS] PositionOffset: {xrSession.PositionOffset}, RotatioOffset: {xrSession.RotationOffset}");
            }
            // FOR DEBUG
            if (PrintDebug)
                xrSession.SetPrintDebug(true);

            // position
            transform.position = Converters.WorldToUnityPosition(xrSession.PositionOffset);

            // rotation       
            // get rotation at origin in world coordinate system
            Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);

            // offset recieved from XRSession
            Quaternion worldOffset = xrSession.RotationOffset;

            // Apply offset to worldOrgin and convert the result into Unity Coordinate System
            transform.rotation = Converters.WorldToUnityRotation(worldOffset * worldOrigin);

            if (PrintDebug)
            {
                PrintDebug = false;
                xrSession.SetPrintDebug(false);
            }
        }
    }
}
