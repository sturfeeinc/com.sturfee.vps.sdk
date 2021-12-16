using SturfeeVPS.Core;
using System;
using UnityEngine;

namespace SturfeeVPS.Providers
{
    public static class AndroidNativeGps
    {
        const string GPS_PROVIDER = "gps";
        const string NETWORK_PROVIDER = "network";
        const string LOCATION_SERVICE = "location";

        private static bool _isMock = false;
        private static AndroidJavaObject _activity;
        private static AndroidJavaObject _locationManager;
        private static LocationListenerProxy _currentListener;

        public static void Initialize()
        {
            var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            _locationManager = _activity.Call<AndroidJavaObject>("getSystemService", LOCATION_SERVICE);

        }

        public static GeoLocation GetLastKnownLocation()
        {
            if(_activity == null || _locationManager == null)
            {
                Initialize();
            }

            AndroidJavaObject locationObj;

            SturfeeDebug.Log("[Android GPS] : Reading Last Known GPS from GPS_PROVIDER");
            locationObj = _locationManager.Call<AndroidJavaObject>("getLastKnownLocation", GPS_PROVIDER);

            if (locationObj == null)
            {
                SturfeeDebug.Log("[Android GPS] : GPS_PROVIDER is null. Reading Last Known GPS from NETWORK_PROVIDER");
                locationObj = _locationManager.Call<AndroidJavaObject>("getLastKnownLocation", NETWORK_PROVIDER);
            }

            if(locationObj == null)
            {
                return new GeoLocation();
            }

            double latitude = locationObj.Call<double>("getLatitude");
            double longitude = locationObj.Call<double>("getLongitude");
            _isMock = locationObj.Call<bool>("isFromMockProvider");

            return new GeoLocation
            {
                Latitude = latitude,
                Longitude = longitude
            };

        }

        public static void RequestLocationUpdates(long minTime, float minDistance, Action<GeoLocation> onLocationChangedCallback)
        {
            if(_activity == null || _locationManager == null)
            {
                Initialize();
            }

            if (minTime <= 0)
            {
                throw new ArgumentOutOfRangeException("minTime", "Time cannot be less then zero");
            }
            if (minDistance <= 0)
            {
                throw new ArgumentOutOfRangeException("minDistance", "minDistance cannot be less then zero");
            }
            if (onLocationChangedCallback == null)
            {
                throw new ArgumentNullException("onLocationChangedCallback", "Location changed callback cannot be null");
            }

            _currentListener = new LocationListenerProxy(onLocationChangedCallback);

            try
            {
                RunOnUiThread(() =>
                {
                    _locationManager.Call("requestLocationUpdates", GPS_PROVIDER, minTime, minDistance, _currentListener);
                    //_locationManager.Call("requestLocationUpdates", NETWORK_PROVIDER, minTime, minDistance, _currentListener);

                });
            }
            catch (Exception e)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning(
                        "Failed to register for location updates. Current device probably does not have GPS. Please check if device has GPS before invoking this method. " +
                        e.Message);
                }
            }
        }

        public static void RunOnUiThread(Action action)
        {
            _activity.Call("runOnUiThread", new AndroidJavaRunnable(action));
        }

        public static void StopLocationUpdates()
        {
            RunOnUiThread(() =>
            {
                _locationManager.Call("removeUpdates", _currentListener);
            });
        }
    }

    

    class LocationListenerProxy : AndroidJavaProxy
    {
        readonly Action<GeoLocation> _onLocationChanged;

        public LocationListenerProxy(Action<GeoLocation> onLocationChanged)
            : base("android.location.LocationListener")
        {
            _onLocationChanged = onLocationChanged;
        }

        void onLocationChanged( /*Location*/ AndroidJavaObject locationAJO)
        {
            int androidAPI = AndroidGpsHelper.getSDKInt();
            if (androidAPI >= 31)
            {
                int count = locationAJO.Call<int>("size");
                for (int i = 0; i < count; i++)
                {
                    var location = locationAJO.Call<AndroidJavaObject>("get", i);
                    _onLocationChanged(LocationFromAJO(location));
                }
            }
            else
            {
                _onLocationChanged(LocationFromAJO(locationAJO));
            }

            //SceneHelper.Queue(() => _onLocationChanged(location));
        }
        

        void onProviderDisabled(string provider)
        {
        }

        void onProviderEnabled(string provider)
        {
        }

        void onStatusChanged(string provider, int status, /*Bundle*/ AndroidJavaObject extras)
        {
        }

        static bool thatWasMe;

        // proxy for int java.lang.Object.hashCode()
        int hashCode()
        {
            thatWasMe = true;
            return GetHashCode();
        }

        // proxy for boolean java.lang.Object.equals(Object o)
        bool equals(AndroidJavaObject o)
        {
            thatWasMe = false;
            o.Call<int>("hashCode");
            return thatWasMe;
        }

        static GeoLocation LocationFromAJO( /*Location*/ AndroidJavaObject locationAJO)
        {
            using (locationAJO)
            {                
                var latitude = locationAJO.Call<double>("getLatitude");
                var longitude = locationAJO.Call<double>("getLongitude");
                var hasAccuracy = locationAJO.Call<bool>("hasAccuracy");
                var accuracy = locationAJO.Call<float>("getAccuracy");
                long time = locationAJO.Call<long>("getTime");

                //var hasSpeed = locationAJO.CallBool("hasSpeed");
                //var speed = locationAJO.Call<float>("getSpeed");
                //var hasBearing = locationAJO.CallBool("hasBearing");
                //var bearing = locationAJO.Call<float>("getBearing");


                //var result = new Location(latitude, longitude, hasAccuracy, accuracy, time);
                var result = new GeoLocation
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                //if (hasSpeed)
                //{
                //    result.HasSpeed = true;
                //    result.Speed = speed;
                //}
                //if (hasBearing)
                //{
                //    result.HasBearing = true;
                //    result.Bearing = bearing;
                //}

                //bool isFromMockProvider = false;
                //try
                //{
                //    isFromMockProvider = locationAJO.CallBool("isFromMockProvider");
                //}
                //catch (Exception)
                //{
                //    // Ignore
                //}
                //result.IsFromMockProvider = isFromMockProvider;

                return result;
            }
        }
    }
}
