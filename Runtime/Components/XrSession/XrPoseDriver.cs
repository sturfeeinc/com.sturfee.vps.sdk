using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class XrPoseDriver : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }
        private void Update()
        {
            transform.position = XrCamera.Pose.Position;
            transform.rotation = XrCamera.Pose.Rotation;
            if(_camera != null)
            {
                _camera.projectionMatrix = XrCamera.Camera.projectionMatrix;
            }            
        }
    }
}
