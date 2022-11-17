using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SturfeeVPS.SDK
{
    public class StartMessage : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject InstructionUI;

        private ARRaycastManager ARRaycastManager;
        private ARPlaneManager ARPlaneManager;
        private ARAnchorManager ARAnchorManager;
        private Camera ArCamera;

        private ARAnchor _startMessageAnchor;

        private void Start()
        {
            ARFManager arManager = ARFManager.CurrentInstance;

            ArCamera = arManager.ArCamera;
            ARRaycastManager = arManager.ARRaycastManager;
            ARPlaneManager = arManager.ARPlaneManager;
            ARAnchorManager = arManager.ARAnchorManager;

            ArCamera.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.ARPlane);

        }

        private void Update()
        {
            if (_startMessageAnchor)
            {
                return;
            }


            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began)
                    return;
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var hits = new List<ARRaycastHit>();
                // Perform the raycast        
                if (ARRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    var hit = hits[0];
                    if (hit.trackable is ARPlane plane)
                    {
                        if (ARPlaneManager)
                        {
                            ArCamera.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.ARObject);

                            var oldPrefab = ARAnchorManager.anchorPrefab;
                            ARAnchorManager.anchorPrefab = Prefab;
                            _startMessageAnchor = ARAnchorManager.AttachAnchor(plane, hit.pose);
                            ARAnchorManager.anchorPrefab = oldPrefab;

                            InstructionUI.SetActive(false);
                            ArCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(SturfeeLayers.ARPlane));

                            ARPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
                            Debug.Log("Trackable selected. Stopping Plane detection");
                        }
                    }
                }
            }
        }
    } 
}
