using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public enum XrCameraControlType
    {
        VpsSatellite,
        VpsHd
    }

    public class XrCameraController : SceneSingleton<XrCameraController>
    {
        [Header("DEPS")]
        [SerializeField]
        private MultiframeScanner Scanner;

        [Header("INTERNAL")]
        public XrCameraControlType ControlType;
        public LayerMask _initialMask;

        [SerializeField]
        private bool _vpsActive = false;
        [SerializeField]
        private LocalizationResponse _currentVpsReponse;

        [SerializeField]
        private int _currentOffsetIndex = 0;
        [SerializeField]
        private ScanFrameInfo _currentFrameInfo = null;

        private Vector3 _localCameraOffset = Vector3.zero;

        private void Start()
        {
            SturfeeEventManager.OnLocalizationSuccessful += HandleLocalizationSuccess;
            SturfeeEventManager.OnLocalizationDisabled += HandleLocalizationReset;
            SturfeeEventManager.OnLocalizationFail += HandleLocalizationFail;

            XrCamera.InternalControl = true;
            _initialMask = XrCamera.Camera.cullingMask;
            XrCamera.Camera.cullingMask = 0;
        }

        private void OnDestroy()
        {
            SturfeeEventManager.OnLocalizationSuccessful -= HandleLocalizationSuccess;
            Application.onBeforeRender -= onBeforeRender;
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
            if (Scanner == null)
            {
                Scanner = FindObjectOfType<MultiframeScanner>();
            }

            if (Scanner == null && ControlType == XrCameraControlType.VpsHd)
            {
                Scanner = FindObjectOfType<HDScanner>();
            }

            if (Scanner == null && ControlType == XrCameraControlType.VpsSatellite)
            {
                Scanner = FindObjectOfType<SatelliteScanner>();
            }

            var xrSession = XrSessionManager.GetSession();
            if (xrSession == null) { return; }

            // update projection matrix based on sensor readings (VideoProvider)
            var videoProvider = xrSession.GetProvider<IVideoProvider>();
            if (videoProvider != null && videoProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                XrCamera.Camera.projectionMatrix = videoProvider.GetProjectionMatrix();
            }

            if (_vpsActive && _currentFrameInfo != null)
            {
                var localizationProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>();

                //_localCameraOffset = Vector3.zero;
                //if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                //{
                //    if (localizationProvider.FrameNumber < MultiframeScanner.ScanFrames.Count)
                //    {
                //        _localCameraOffset = MultiframeScanner.ScanFrames[localizationProvider.FrameNumber].LocalPosition;
                //    }
                //}

                // update local Pose of camera based on sensor readings(PoseProvider)
                XrCamera.Camera.transform.localPosition = Converters.WorldToUnityPosition(Position) - _currentFrameInfo.LocalPosition; // _localCameraOffset; // _currentFrameInfo.LocalPosition;
                XrCamera.Camera.transform.localRotation = Converters.WorldToUnityRotation(Rotation);

                if (ControlType == XrCameraControlType.VpsHd)
                {
                    ApplyHdOffets(localizationProvider);
                }
                else
                {
                    ApplySatelliteOffets(localizationProvider);
                }
            }
            else
            {
                XrCamera.Camera.transform.localPosition = Converters.WorldToUnityPosition(Position);
                XrCamera.Camera.transform.localRotation = Converters.WorldToUnityRotation(Rotation);
            }
        }

        private async void HandleLocalizationSuccess()
        {
            if (Scanner != null)
            {
                await Task.Delay(500);

                Debug.Log($"Scanner.VpsReponse.response.rotationOffset = {MultiframeScanner.VpsReponse.response.rotationOffset}");

                XrCamera.InternalControl = false;
                XrCamera.Camera.cullingMask = _initialMask;
                if (MultiframeScanner.VpsReponse.error == null)
                {
                    _vpsActive = true;
                    SetupOffsets(); // _currentOffsetIndex);

                    //var arCamera = ARFManager.CurrentInstance.ArCamera;
                    //arCamera.cullingMask = 0;
                }
            }
        }

        private void HandleLocalizationFail(string error)
        {
            HandleLocalizationReset();
        }

        private void HandleLocalizationReset()
        {
            _vpsActive = false;
            _currentFrameInfo = null;
            _currentOffsetIndex = 0;
            XrCamera.InternalControl = true;
            XrCamera.Camera.cullingMask = 0;
        }

        private void SetupOffsets() // int index)
        {
            if (MultiframeScanner.ScanFrames.Count == 0)
            {
                Debug.Log($"NO SCAN FRAMES RECORDED");
            }

            if (MultiframeScanner.ScanFrames.Count > 0)
            {
                if (MultiframeScanner.VpsReponse != null && MultiframeScanner.VpsReponse.response != null)
                {
                    _currentOffsetIndex = MultiframeScanner.VpsReponse.response.FrameNumber;
                    _currentFrameInfo = MultiframeScanner.ScanFrames[_currentOffsetIndex];
                    _currentFrameInfo = new ScanFrameInfo
                    {
                        Position = MultiframeScanner.ScanFrames[_currentOffsetIndex].Position,
                        Rotation = MultiframeScanner.ScanFrames[_currentOffsetIndex].Rotation,
                        LocalPosition = MultiframeScanner.ScanFrames[_currentOffsetIndex].LocalPosition,
                        LocalRotation = MultiframeScanner.ScanFrames[_currentOffsetIndex].LocalRotation
                    };

                    _currentVpsReponse = new LocalizationResponse
                    {
                        location = MultiframeScanner.VpsReponse.response.location,
                        yawOrientationCorrection = MultiframeScanner.VpsReponse.response.yawOrientationCorrection,
                        pitchOrientationCorrection = MultiframeScanner.VpsReponse.response.pitchOrientationCorrection,
                        rollOrientationCorrection = MultiframeScanner.VpsReponse.response.rollOrientationCorrection,
                        rotationOffset = MultiframeScanner.VpsReponse.response.rotationOffset,
                        eulerOffset = MultiframeScanner.VpsReponse.response.eulerOffset,
                        FrameNumber = MultiframeScanner.VpsReponse.response.FrameNumber,
                    };
                }
            }
        }

        private void ApplyHdOffets(ILocalizationProvider localizationProvider)
        {
            // get the data from VPS
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var vpsLocation = localizationProvider.Location; // MultiframeScanner.VpsReponse.response.location;
                var vpsPosition = Converters.GeoToUnityPosition(vpsLocation); // PositioningUtils.GeoToWorldPosition(vpsLocation);
                                                                              //var vpsOffsetRotation = Converters.WorldToUnityRotation(MultiframeScanner.VpsReponse.response.rotationOffset); // convert server response to Unity coords

                // get rotation at origin in world coordinate system (same as server coords)
                Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);
                // offset recieved from XRSession
                Quaternion worldOffset = localizationProvider.RotationOffset; // MultiframeScanner.VpsReponse.response.rotationOffset;
                var vpsOffsetRotation = Converters.WorldToUnityRotation(worldOffset * worldOrigin); // convert server response to Unity coords

                // apply VPS offsets to the XR Camera ORIGIN
                XrCamera.Camera.transform.parent.position = vpsPosition;
                XrCamera.Camera.transform.parent.rotation = vpsOffsetRotation;
            }
        }

        private void ApplySatelliteOffets(ILocalizationProvider localizationProvider)
        {
            // get the data from VPS
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var vpsLocation = localizationProvider.Location; // MultiframeScanner.VpsReponse.response.location;
                var vpsPosition = Converters.GeoToUnityPosition(vpsLocation); // PositioningUtils.GeoToWorldPosition(vpsLocation);
                                                                              //var vpsOffsetRotation = Converters.WorldToUnityRotation(MultiframeScanner.VpsReponse.response.rotationOffset); // convert server response to Unity coords

                // get rotation at origin in world coordinate system (same as server coords)
                Quaternion worldOrigin = Converters.UnityToWorldRotation(Quaternion.identity);
                // offset recieved from XRSession
                Quaternion worldOffset = localizationProvider.YawOffset * Quaternion.identity * localizationProvider.PitchOffset; // localizationProvider.RotationOffset; // MultiframeScanner.VpsReponse.response.rotationOffset;
                var vpsOffsetRotation = Converters.WorldToUnityRotation(worldOffset * worldOrigin); // convert server response to Unity coords

                // apply VPS offsets to the XR Camera ORIGIN
                XrCamera.Camera.transform.parent.position = vpsPosition;
                XrCamera.Camera.transform.parent.rotation = vpsOffsetRotation;
            }
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
    }

}
