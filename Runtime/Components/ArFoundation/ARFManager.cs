using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SturfeeVPS.SDK
{
    public class ARFManager : SceneSingleton<ARFManager>
    {
        [SerializeField]
        private ARSession _arSessionPrefab;
        [SerializeField]
        private ARSessionOrigin _aRSessionOriginPrefab;
        [SerializeField]
        private GameObject _arPlanePrefab;


        private ARSession _aRSession;
        private ARSessionOrigin _aRSessionOrigin;
        private ARPlaneManager _aRPlaneManager;
        private ARRaycastManager _aRRaycastManager;
        private ARAnchorManager _aRAnchorManager;
        private Camera _arCamera;

        public ARTrackable ARTrackable { get; private set; }
        public Camera ArCamera {
            get
            {
                if(ARSession == null)
                {
                    SturfeeDebug.LogError($"[ARFManager] :: ARSession is not created"); 
                    return null;
                }

                if (_arCamera == null)
                {
                    _arCamera = ARSessionOrigin.camera;
                    _arCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(SturfeeLayers.SturgBuilding));
                    _arCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(SturfeeLayers.SturgTerrain));
                    _arCamera.nearClipPlane = 0.02f;
                    _arCamera.farClipPlane = 2000;
                }
                return _arCamera;
            }
        }

        public ARSession ARSession
        {
            get
            {
                if (_aRSession == null)
                {
                    _aRSession = FindObjectOfType<ARSession>();
                }

                if (_aRSession == null)
                {
                    _aRSession = Instantiate(_arSessionPrefab);
                }

                return _aRSession;
            }
        }

        public ARSessionOrigin ARSessionOrigin
        {
            get
            {
                if (ARSession == null)
                {
                    SturfeeDebug.LogError($"[ARFManager] :: ARSession is not created");
                    return null;
                }

                if (_aRSessionOrigin == null)
                {
                    _aRSessionOrigin = FindObjectOfType<ARSessionOrigin>();
                }

                if (_aRSessionOrigin == null)
                {
                    _aRSessionOrigin = Instantiate(_aRSessionOriginPrefab);
                }

                return _aRSessionOrigin;
            }
        }

        public ARPlaneManager ARPlaneManager
        {
            get
            {
                if (ARSession == null)
                {
                    SturfeeDebug.LogError($"[ARFManager] :: ARSession is not created");
                    return null;
                }

                if (_aRPlaneManager == null)
                {
                    _aRPlaneManager = FindObjectOfType<ARPlaneManager>();
                }

                if (_aRPlaneManager == null)
                {
                    _aRPlaneManager = ARSessionOrigin.gameObject.AddComponent<ARPlaneManager>();
                }

                if (_aRPlaneManager.planePrefab == null)
                {
                    _aRPlaneManager.planePrefab = _arPlanePrefab ;
                }

                return _aRPlaneManager;
            }
        }

        public ARRaycastManager ARRaycastManager
        {
            get
            {
                if (ARSession == null)
                {
                    SturfeeDebug.LogError($"[ARFManager] :: ARSession is not created");
                    return null;
                }

                if (_aRRaycastManager == null)
                {
                    _aRRaycastManager = FindObjectOfType<ARRaycastManager>();
                }

                if (_aRRaycastManager == null)
                {
                    _aRRaycastManager = ARSessionOrigin.gameObject.AddComponent<ARRaycastManager>();
                }

                return _aRRaycastManager;
            }
        }

        public ARAnchorManager ARAnchorManager
        {
            get
            {
                if (ARSession == null)
                {
                    SturfeeDebug.LogError($"[ARFManager] :: ARSession is not created");
                    return null;
                }

                if (_aRAnchorManager == null)
                {
                    _aRAnchorManager = FindObjectOfType<ARAnchorManager>();
                }

                if (_aRAnchorManager == null)
                {
                    _aRAnchorManager = ARSessionOrigin.gameObject.AddComponent<ARAnchorManager>();
                }

                return _aRAnchorManager;
            }
        }

        public ProviderStatus ProviderStatus
        {
            get
            {
                switch (ARSession.state)
                {
                    case ARSessionState.Unsupported:
                        return ProviderStatus.NotSupported;

                    case ARSessionState.SessionInitializing:
                        return ProviderStatus.Initializing;

                    case ARSessionState.SessionTracking:
                        return ProviderStatus.Ready;
                }

                return ProviderStatus.Initializing;
            }
        }
    }
}
