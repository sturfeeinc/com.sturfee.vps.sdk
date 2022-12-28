using System;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class XRPose
    {
        public GeoLocation GeoLocation;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}