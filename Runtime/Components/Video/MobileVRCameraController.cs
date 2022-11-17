using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class MobileVRCameraController : MonoBehaviour
    {
        private void Update()
        {
            transform.position = XrCamera.Pose.Position;
            transform.rotation = XrCamera.Pose.Rotation;
        }
    }
}
