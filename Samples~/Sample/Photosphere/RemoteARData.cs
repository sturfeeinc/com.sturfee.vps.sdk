using System;
using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK.Samples
{
    [Serializable]
    public class RemoteARData
    {
        public string sampleId;
        public string sampleName;
        public string dataFormat;
        public string acquisitionTimestamp;
        public SensorExternalParameters sensorExternalParameters;
        public SensorInternalParameters sensorInternalParameters;
        public LocalizationResponse localizationResponse;
        public MetaData metaData;
    }

    [Serializable]
    public class SensorExternalParameters
    {
        public GeoLocation location;
        public Quaternion[] quaternion;
    }

    [Serializable]
    public class SensorInternalParameters
    {
        public float sceneWidth;
        public float sceneHeight;
        public Matrix4x4 projectionMatrix;
        public float fov;
        public bool isPortrait;
    }

    [Serializable]
    public class MetaData
    {
        public int startId = -1;
    }
}
