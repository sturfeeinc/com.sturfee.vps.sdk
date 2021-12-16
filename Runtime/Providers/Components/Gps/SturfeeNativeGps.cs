using System;
using SturfeeVPS.Core;
using UnityEngine;
using UnityEngine.Android;

namespace SturfeeVPS.Providers
{
    internal static class SturfeeNativeGps
    {
        public static GeoLocation GetLastKnownGpsLocation()
        {
            if (!HasLocationPermission())
            {
                Debug.LogError("Location permission not granted");
            }
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return AndroidNativeGps.GetLastKnownLocation();
                }
                else if (GetLatitude() != 0 && GetLongitude() != 0)
                {
                    GeoLocation gpsPosition = new GeoLocation
                    {
                        Latitude = GetLatitude(),
                        Longitude = GetLongitude(),
                        Altitude = GetAltitude()
                    };

                    return gpsPosition;
                }
            }

            return null;
        }

        public static void RequestLocationUpdates()
        {
            if (!HasLocationPermission())
            {
                Debug.LogError("Location permission not granted");
            }
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    AndroidNativeGps.Initialize();
                    AndroidNativeGps.RequestLocationUpdates(200, 1, AndroidHelper.SetCurrentLocation);
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
#if UNITY_IOS
                    LocationManagerBridge.setMessageReceivingObjectName(IOSHelper.name, "SetCurrentLocation");
                    LocationManagerBridge.startLocationMonitoring();
#endif
                }
            }
        }

        public static void StopLocationUpdates()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Input.location.Stop();
                AndroidNativeGps.StopLocationUpdates();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                LocationManagerBridge.stopLocationMonitoring();
#endif
            }
        }

        public static double GetLatitude()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return AndroidHelper.GetLatitude();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return IOSHelper.GetComponent<IOSGpsHelper>().GetLatitude();
            }
            return 0;
        }

        public static double GetLongitude()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return AndroidHelper.GetLongitude();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return IOSHelper.GetComponent<IOSGpsHelper>().GetLongitude();
            }
            return 0;
        }

        public static double GetAltitude()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return AndroidHelper.GetAltitude();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return IOSHelper.GetComponent<IOSGpsHelper>().GetAltitude();
            }
            return 0;
        }

        private static bool HasLocationPermission()
        {
            //NOTE : Unity takes care of permissions but if ArCore is used it overrides 
            // Unity's permission management and hence we need something for Android.
            // ArKit however does not override Unity's permission management so it is ok to not 
            // have any code for IOS asking for permission.

            //TODO: Add IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return true;
            }


            if (Application.platform == RuntimePlatform.Android)
            {
                //Permission.RequestUserPermission(Permission.FineLocation, )

                AndroidRuntimePermissions.Permission permission = AndroidRuntimePermissions.CheckPermission("android.permission.ACCESS_FINE_LOCATION");

                if (permission == AndroidRuntimePermissions.Permission.ShouldAsk)
                {
                    AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.ACCESS_FINE_LOCATION");
                    if (result == AndroidRuntimePermissions.Permission.Granted)
                    {
                        return true;
                    }
                }
                //If we already have permisssion
                else if (permission == AndroidRuntimePermissions.Permission.Granted)
                {
                    return true;
                }

            }

            return false;
        }

        private static AndroidGpsHelper _androidHelper;
        private static AndroidGpsHelper AndroidHelper
        {
            get
            {
                if (_androidHelper == null)
                {
                    _androidHelper = new AndroidGpsHelper();
                }

                return _androidHelper;
            }
        }

        private static GameObject _iosHelper;
        private static GameObject IOSHelper
        {
            get
            {
                if (_iosHelper == null)
                {
                    _iosHelper = new GameObject();
                    _iosHelper.name = "_iosGpshelper";
                    _iosHelper.AddComponent<IOSGpsHelper>();
                }

                return _iosHelper;
            }
        }
    }
}
