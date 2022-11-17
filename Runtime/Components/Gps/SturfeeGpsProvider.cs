using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK.Providers
{
    public class SturfeeGpsProvider : BaseGpsProvider
    {
        /// <summary>
        /// Fake GPS location. 
        /// </summary>
        [Tooltip("Will be used as Approxiamte Location in Editor mode")]
        public GeoLocation FakeLocation = new GeoLocation
        {
            Latitude = 37.332093,
            Longitude = -121.890137
        };

        private bool _locationReady;
        private ProviderStatus _providerStatus;

        private void Update()
        {
            if (!_locationReady || _providerStatus != ProviderStatus.Ready)
            {
                if (SturfeeNativeGps.GetLatitude() != 0 && SturfeeNativeGps.GetLongitude() != 0)
                {
                    //Location Ready
                    _locationReady = true;
                    _providerStatus = ProviderStatus.Ready;

                    SturfeeDebug.Log($"Location ready : {SturfeeNativeGps.GetLatitude()}, {SturfeeNativeGps.GetLongitude()}");
                }
            }
            //Debug.Log($"{_providerStatus} , instance : {GetInstanceID()}");
        }

        public override void OnRegister()
        {
            //This will add "ACCESS_FINE_LOCATION" in AndroidManifest
            Input.location.Start(1, 1);

            SturfeeNativeGps.RequestLocationUpdates();

            _providerStatus = ProviderStatus.Initializing;

            base.OnRegister();
        }
        
        public override void OnUnregister()
        {
            SturfeeNativeGps.StopLocationUpdates();

            base.OnUnregister();
        }

        public override GeoLocation GetApproximateLocation(out bool includesElevation)
        {
            includesElevation = false;

#if UNITY_EDITOR
            return FakeLocation;
#else
            return new GeoLocation(Input.location.lastData);
#endif
        }

        public override GeoLocation GetFineLocation(out bool includesElevation)
        {
            includesElevation = false;

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
            return _providerStatus;
        }

    }
}
