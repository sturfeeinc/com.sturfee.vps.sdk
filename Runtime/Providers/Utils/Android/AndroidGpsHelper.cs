using System;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.Providers
{
    public class AndroidGpsHelper
    {
        private GeoLocation _position;
        private bool _locationReady;

        public double GetLatitude()
        {
            return !_locationReady ? 0 : _position.Latitude;
        }

        public double GetLongitude()
        {
            return !_locationReady ? 0 : _position.Longitude;
        }

        public double GetAltitude()
        {
            return 0;
        }

        public void SetCurrentLocation(GeoLocation location)
        {       
            if (location == null)
            {
                return;
            }

            _position = location;

            if (_position.Latitude != 0 && _position.Longitude != 0)
            {
                _locationReady = true;
            }

            SturfeeDebug.Log("Location from Android Native : Lat : " + location.Latitude.ToString() + " Long : " + location.Longitude.ToString(), false);
        }

        public static int getSDKInt()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
    }


}