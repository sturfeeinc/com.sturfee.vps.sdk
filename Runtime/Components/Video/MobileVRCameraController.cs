using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Controls VR scene camera's transform
    /// </summary>
    public class MobileVRCameraController : MonoBehaviour
    {
        public Camera Camera;

        private void Update()
        {
            transform.position = XrCamera.Pose.Position;
            transform.rotation = XrCamera.Pose.Rotation;
        }
    }
}
