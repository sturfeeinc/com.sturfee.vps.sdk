using System;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK.Providers
{
    public class IOSGpsHelper : MonoBehaviour
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
            //TODO: Use Height from Native
            return Input.location.lastData.altitude;
        }

        public void SetCurrentLocation(string location)
        {
            if (_position == null)
            {
                _position = new GeoLocation();
            }
            _position.Latitude = Convert.ToDouble(location.Split('/')[0]);
            _position.Longitude = Convert.ToDouble(location.Split('/')[1]);
            //_position.Height = Input.location.lastData.altitude;

            if (_position.Latitude != 0 && _position.Longitude != 0)
            {
                _locationReady = true;
            }

            SturfeeDebug.Log("Location from iOS Native : Lat : " + _position.Latitude.ToString() + " Long : " + _position.Longitude.ToString(), false);
        }

    }


}
