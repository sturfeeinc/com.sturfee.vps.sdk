using SturfeeVPS.Core.Proto;
using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class LocalizationRequest
    {
        public int requestId;
        public Multiframe frame;
        public string siteId;
        public ExternalParameters externalParameters;
        public InternalParameters internalParameters;

        public static LocalizationRequest ParseProtobufRequest(Request request)
        {
            LocalizationRequest localizatioRequest = new LocalizationRequest
            {
                requestId = (int)request.RequestId,
                externalParameters = new ExternalParameters
                {
                    latitude = request.ExternalParameters.Position.Lat,
                    longitude = request.ExternalParameters.Position.Lon,
                    height = request.ExternalParameters.Position.Height,
                    quaternion = new Quaternion
                    {
                        x = (float)request.ExternalParameters.Quaternion.X,
                        y = (float)request.ExternalParameters.Quaternion.Y,
                        z = (float)request.ExternalParameters.Quaternion.Z,
                        w = (float)request.ExternalParameters.Quaternion.W
                    }
                },
                internalParameters = new InternalParameters
                {
                    fov = request.InternalParameters.Fov,
                    sceneWidth = (int)request.InternalParameters.SceneWidth,
                    sceneHeight = (int)request.InternalParameters.SceneHeight,
                    projectionMatrix = new Matrix4x4
                    {
                        m00 = (float)request.InternalParameters.ProjectionMatrix[0],
                        m01 = (float)request.InternalParameters.ProjectionMatrix[1],
                        m02 = (float)request.InternalParameters.ProjectionMatrix[2],
                        m03 = (float)request.InternalParameters.ProjectionMatrix[3],
                        m10 = (float)request.InternalParameters.ProjectionMatrix[4],
                        m11 = (float)request.InternalParameters.ProjectionMatrix[5],
                        m12 = (float)request.InternalParameters.ProjectionMatrix[6],
                        m13 = (float)request.InternalParameters.ProjectionMatrix[7],
                        m20 = (float)request.InternalParameters.ProjectionMatrix[8],
                        m21 = (float)request.InternalParameters.ProjectionMatrix[9],
                        m22 = (float)request.InternalParameters.ProjectionMatrix[10],
                        m23 = (float)request.InternalParameters.ProjectionMatrix[11],
                        m30 = (float)request.InternalParameters.ProjectionMatrix[12],
                        m31 = (float)request.InternalParameters.ProjectionMatrix[13],
                        m32 = (float)request.InternalParameters.ProjectionMatrix[14],
                        m33 = (float)request.InternalParameters.ProjectionMatrix[15],

                    }
                },
                frame = new Multiframe
                {
                    count = (int)request.TotalNumOfFrames,
                    order = (int)request.FrameOrder
                },
                siteId = request.SiteId
            };

            return localizatioRequest;
        }
 
    }

    [Serializable]
    public class Multiframe
    {
        public int count;
        public int order;
    }

    [Serializable]
    public class ExternalParameters
    {
        public double latitude;
        public double longitude;
        public double height;
        public Quaternion quaternion;
    }

    [Serializable]
    public class InternalParameters
    {
        public int sceneWidth;
        public int sceneHeight;
        public float fov;
        public int isPortrait;
        public Matrix4x4 projectionMatrix;
    }
}
