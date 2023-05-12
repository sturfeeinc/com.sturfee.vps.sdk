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
        public static bool InternalControl = true;
        public static XrCamera Instance => _instance;

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
            if (xrSession == null) { return; }

            var localCameraOffset = Vector3.zero;
            var localizationProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>();
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                if (localizationProvider.FrameNumber < MultiframeScanner.ScanFrames.Count)
                {
                    localCameraOffset = MultiframeScanner.ScanFrames[localizationProvider.FrameNumber].LocalPosition;
                }
            }
            //if (MultiframeScanner.SelectedFrame != null)
            //{
            //    localCameraOffset = MultiframeScanner.SelectedFrame.LocalPosition;
            //}

            // update local Pose of camera based on sensor readings(PoseProvider)
            _camera.transform.localPosition = Converters.WorldToUnityPosition(Position) - localCameraOffset;
            _camera.transform.localRotation = Converters.WorldToUnityRotation(Rotation);

            // update projection matrix based on sensor readings (VideoProvider)
            var videoProvider = xrSession.GetProvider<IVideoProvider>();
            if (videoProvider != null && videoProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                _camera.projectionMatrix = videoProvider.GetProjectionMatrix();
            }

            ApplyOffsets();

            // FOR DEBUG
            if (PrintDebug)
                SturfeeDebug.Log($"[XrCamera.cs] [DEBUG BUTTON PRESS] Position: {Position}, Rotation: {Rotation}");

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
            // var xrSession = XrSessionManager.GetSession();
            // // FOR DEBUG
            // if (PrintDebug)
            // {
            //     SturfeeDebug.Log($"[XrCamera.cs] [DEBUG BUTTON PRESS] PositionOffset: {xrSession.PositionOffset}, RotatioOffset: {xrSession.RotationOffset}");
            // }
            // // FOR DEBUG
            // if (PrintDebug)
            //     xrSession.SetPrintDebug(true);


            // get the data from VPS
            var localizationProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>();
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var vpsLocation = localizationProvider.Location; // MultiframeScanner.VpsReponse.response.location;
                var vpsPosition = Converters.GeoToUnityPosition(vpsLocation); // PositioningUtils.GeoToWorldPosition(vpsLocation);

                // rotation       
                // get rotation at origin in world coordinate system (same as server coords)
                Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);
                // offset recieved from XRSession
                Quaternion worldOffset = localizationProvider.RotationOffset; // MultiframeScanner.VpsReponse.response.rotationOffset;
                var vpsOffsetRotation = Converters.WorldToUnityRotation(worldOffset * worldOrigin); // convert server response to Unity coords

                // apply VPS offsets to the XR Camera ORIGIN
                transform.position = vpsPosition;
                transform.rotation = vpsOffsetRotation;
            }
            else
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }

            // if (PrintDebug)
            // {
            //     PrintDebug = false;
            //     xrSession.SetPrintDebug(false);
            // }

        }
    }
}