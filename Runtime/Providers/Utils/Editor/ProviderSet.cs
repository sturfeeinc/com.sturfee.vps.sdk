using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ProviderSet : MonoBehaviour
    {
        public GpsProviderBase GpsProvider;
        public PoseProviderBase PoseProvider;
        public VideoProviderBase VideoProvider;
    }
}