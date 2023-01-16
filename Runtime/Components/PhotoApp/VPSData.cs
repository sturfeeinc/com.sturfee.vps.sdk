using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class VPSData
    {
        public string trackingId;
        public double centerX;
        public double centerY;
        public VPSTransform arfShift;
        
        public List<frame> frames;
    }

    [Serializable]
    public class frame
    {
        public int frameId;
        public string frameName;
        public string timeStamp;
        public VPSMatrix4x4 frameIntrinsic; 
        public WorldPoseContainer worldPose;
        public VPSTransform arfPose; 
    }

    [Serializable]
    public class VPSTransform
    {
        public VPSVector3 location;

        // Unity quaternion is x,y,z,w. So store orientation as w,x,y,z in Vector4
        public VPSQuaternion orientation;
    }

    [Serializable]
    public class WorldPoseContainer
    {
        public UtmPosition locationUTM;
        public VPSQuaternion mappedOrientation;
    }

    [Serializable]
    public class VPSVector3
    {
        public float X;
        public float Y;
        public float Z;
        
        public VPSVector3(){}
        
        public VPSVector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public VPSVector3(Vector3 location)
        {
            this.X = location.x;
            this.Y = location.y;
            this.Z = location.z;
        }
    }
    [Serializable]
    public class VPSQuaternion
    {
        public float W;
        public float X;
        public float Y;
        public float Z;
        
        public VPSQuaternion(){}
        
        public VPSQuaternion(float W, float X, float Y, float Z)
        {
            this.W = W;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public VPSQuaternion(Quaternion rotation)
        {
            this.W = rotation.w;
            this.X = rotation.x;
            this.Y = rotation.y;
            this.Z = rotation.z;
        }
    }
    [Serializable]
    public class VPSMatrix4x4
    {
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;
        
    }
}
