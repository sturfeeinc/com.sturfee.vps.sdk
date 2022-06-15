using System.Collections;
using SturfeeVPS.Core;
using UnityEngine;


namespace SturfeeVPS.Providers
{
    /// <summary>
    /// Sturfee's GpsProvider
    /// </summary>
    public class SturfeeGpsProvider : GpsProviderBase
    {
        /// <summary>
        /// If you want to use Fake Gps
        /// </summary>
        public bool UseFakeGps;

        /// <summary>
        /// Fake GPS location
        /// </summary>
        public GeoLocation FakeLocation = new GeoLocation
        {
            Latitude = 37.332093,            
            Longitude = -121.890137
        };

        private bool _locationReady;    
        private ProviderStatus _providerStatus;

        private void Update()
        {
            if (!_locationReady || _providerStatus != ProviderStatus.Ready )
            {
                if (SturfeeNativeGps.GetLatitude() != 0 && SturfeeNativeGps.GetLongitude() != 0)
                {
                    //Location Ready
                    _locationReady = true;
                    _providerStatus = ProviderStatus.Ready;
                }
            }
        }

        public override void Initialize()
        {
            if (UseFakeGps)
                return;

            //This will add "ACCESS_FINE_LOCATION" in AndroidManifest
            Input.location.Start(1, 1);

            _providerStatus = ProviderStatus.Initializing;
            SturfeeNativeGps.RequestLocationUpdates();
        }        

        public override GeoLocation GetCurrentLocation()
        {
            if (UseFakeGps) return FakeLocation;
            if (_providerStatus != ProviderStatus.Ready)
            {
                SturfeeDebug.LogWarning("Location service is not ready");
            }
            else
            {
                GeoLocation gpsPosition = new GeoLocation
                {   
                    Latitude = SturfeeNativeGps.GetLatitude(),
                    Longitude = SturfeeNativeGps.GetLongitude(),
                    Altitude = SturfeeNativeGps.GetAltitude()
                };

                return gpsPosition;
            }
            return null;
        }

        public override ProviderStatus GetProviderStatus()
        {
            if (UseFakeGps) return ProviderStatus.Ready;
            return _providerStatus;
        }

        public override void Destroy()
        {
            if(UseFakeGps)
                return;

            SturfeeNativeGps.StopLocationUpdates();
        }
    }
}