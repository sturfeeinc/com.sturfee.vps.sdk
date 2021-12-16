using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Utility class that provides conversion methods between different
    /// coordinate systems
    /// </summary>
    public class Converters
    {
        private static GameObject _helperObj;
        private static Vector3 _fixedUp = new Vector3();
        private static Vector3 _fixedForward = new Vector3();

        /// <summary>
        /// Gets a World Coordinate System rotation for a rotation provided
        /// in Unity Coordinate System
        /// </summary>
        /// <param name="unityRotation"> Unity rotation </param>
        /// <returns></returns>
        public static Quaternion UnityToWorldRotation(Quaternion unityRotation)
        {
            return ConvertOrientation(unityRotation);
        }

        /// <summary>
        /// Gets a Unity Coordinate System rotation for a rotation provided
        /// in World Coordinate System
        /// </summary>
        /// <param name="worldRotation"> World rotation</param>
        /// <returns></returns>
        public static Quaternion WorldToUnityRotation(Quaternion worldRotation)
        {
            return ConvertOrientation(worldRotation);
        }

        /// <summary>
        /// Gets a World Coordinate System position for a position provided
        /// in Unity Coordinate System
        /// </summary>
        /// <param name="unityPos"> Unity position</param>
        /// <returns></returns>
        public static Vector3 UnityToWorldPosition(Vector3 unityPos)
        {
            return new Vector3()
            {
                x = unityPos.x,
                y = unityPos.z,
                z = unityPos.y
            };
        }

        /// <summary>
        /// Gets a Unity Coordinate System position for a position provided
        /// in World Coordinate System
        /// </summary>
        /// <param name="worldPos"> World position</param>
        /// <returns></returns>
        public static Vector3 WorldToUnityPosition(Vector3 worldPos)
        {
            return new Vector3()
            {
                x = worldPos.x,
                y = worldPos.z,
                z = worldPos.y
            };
        }

        /// <summary>
        /// Gets a Unity Coordinate System position for a location provided in
        /// Geo-spatial Coordinate System        
        /// </summary>
        /// <param name="location"> Geo-spatial location </param>
        /// <returns></returns>
        public static Vector3 GeoToUnityPosition(GeoLocation location)
        {
            return WorldToUnityPosition(
                PositioningUtils.GeoToWorldPosition(location));
        }

        /// <summary>
        /// Gets a Geo-spatial location for a position provided in Unity
        /// Coordinate System
        /// </summary>
        /// <param name="unityPos"> Local position in Unity Coordinate System</param>
        /// <returns></returns>
        public static GeoLocation UnityToGeoLocation(Vector3 unityPos)
        {
            return PositioningUtils.WorldToGeoLocation(
                UnityToWorldPosition(unityPos));
        }

        private static Quaternion ConvertOrientation(Quaternion orientation)
        {
            if (_helperObj == null)
            {
                _helperObj = new GameObject
                {
                    name = "_orienatationHelper"
                };
            }

            _helperObj.transform.rotation = orientation;

            _fixedForward.x = _helperObj.transform.forward.x;
            _fixedForward.y = _helperObj.transform.forward.z;
            _fixedForward.z = _helperObj.transform.forward.y;

            _fixedUp.x = _helperObj.transform.up.x;
            _fixedUp.y = _helperObj.transform.up.z;
            _fixedUp.z = _helperObj.transform.up.y;

            var mappedOrientation = Quaternion.LookRotation(-_fixedForward, _fixedUp);

            return mappedOrientation;

        }

        internal static GameObject GetHelperObject()
        {
            return _helperObj;
        }

    }
}