using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;

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

        private Vector3 _localCameraOffset = Vector3.zero;

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

        private void OnDestroy()
        {
            _instance = null;
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
    }
}