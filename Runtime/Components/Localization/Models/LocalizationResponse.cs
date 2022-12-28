using SturfeeVPS.Core;
using SturfeeVPS.Core.Proto;
using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class LocalizationResponse
    {
        public GeoLocation location;
        public Quaternion yawOrientationCorrection;
        public Quaternion pitchOrientationCorrection;
        public Quaternion rollOrientationCorrection;
        public Vector3 eulerOffset;

        public static LocalizationResponse ParseProtobufResponse(Response response)
        {
            LocalizationResponse localizationResponse = new LocalizationResponse
            {
                location = new GeoLocation
                {
                    Latitude = response.Position.Lat,
                    Longitude = response.Position.Lon,
                    Altitude = response.Position.Height
                },
                yawOrientationCorrection = new Quaternion
                {
                    x = (float)response.YawOffsetQuaternion.X,
                    y = (float)response.YawOffsetQuaternion.Y,
                    z = (float)response.YawOffsetQuaternion.Z,
                    w = (float)response.YawOffsetQuaternion.W
                },
                pitchOrientationCorrection = new Quaternion
                {
                    x = (float)response.PitchOffsetQuaternion.X,
                    y = (float)response.PitchOffsetQuaternion.Y,
                    z = (float)response.PitchOffsetQuaternion.Z,
                    w = (float)response.PitchOffsetQuaternion.W,
                },               
            };

            if(response.EulerOffset != null)
            {
                localizationResponse.eulerOffset = new Vector3
                {
                    x = (float)response.EulerOffset.PitchOffset,
                    y = (float)response.EulerOffset.RollOffset,
                    z = (float)response.EulerOffset.YawOffset
                };
            }
            
            return localizationResponse;
        }
    }

}
