using System.Runtime.InteropServices;

namespace SturfeeVPS.SDK.Providers
{
    public class LocationManagerBridge
    {
#if UNITY_IOS

        [DllImport("__Internal")]
        private static extern int _getAuthrizationLevelForApplication();
        public static int getAuthrizationLevelForApplication()
        {
            return _getAuthrizationLevelForApplication();
        }

        [DllImport("__Internal")]
        private static extern void _requestAuthorizedAlways();
        public static void requestAuthorizedAlways()
        {
            _requestAuthorizedAlways();
        }

        [DllImport("__Internal")]
        private static extern void _requestAuthorizedWhenInUse();
        public static void requestAuthorizedWhenInUse()
        {
            _requestAuthorizedWhenInUse();
        }

        [DllImport("__Internal")]
        private static extern void _showAlertForPermissions(string alertTitle, string alertMessage, string defaultBtnTitle, string cancelBtnTitle);
        public static void showAlertForPermissions(string alertTitle, string alertMessage, string defaultBtnTitle, string cancelBtnTitle)
        {
            _showAlertForPermissions(alertTitle, alertMessage, defaultBtnTitle, cancelBtnTitle);
        }

        [DllImport("__Internal")]
        private static extern bool _startLocationMonitoring();
        public static bool startLocationMonitoring()
        {
            return _startLocationMonitoring();
        }

        [DllImport("__Internal")]
        private static extern void _stopLocationMonitoring();
        public static void stopLocationMonitoring()
        {
            _stopLocationMonitoring();
        }

        [DllImport("__Internal")]
        private static extern void _setMessageReceivingObjectName(string msgReceivingGameObjectName, string msgReceivingMethodName);
        public static void setMessageReceivingObjectName(string msgReceivingGameObjectName, string msgReceivingMethodName)
        {
            _setMessageReceivingObjectName(msgReceivingGameObjectName, msgReceivingMethodName);
        }

        [DllImport("__Internal")]
        private static extern void _getAddressForCurrentLocation();
        public static void getAddressForCurrentLocation()
        {
            _getAddressForCurrentLocation();
        }

        [DllImport("__Internal")]
        private static extern void _getAddressForLocationWithLatitudeLongitude(string locationLatitudeTemp, string locationLongitudeTemp);
        public static void getAddressForLocationWithLatitudeLongitude(double locationLatitudeTemp, double locationLongitudeTemp)
        {
            _getAddressForLocationWithLatitudeLongitude(locationLatitudeTemp.ToString(), locationLongitudeTemp.ToString());
        }
#endif
    }
}
