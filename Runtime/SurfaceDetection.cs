using System;
using UnityEngine;
using SturfeeVPS.Core;

namespace SturfeeVPS.SDK
{
    public class SurfaceDetection : MonoBehaviour
    {
        /// <summary>
        /// Detects surface at provided screenPosition point
        /// </summary>
        /// <param name="screenPos">screen coordinates></param>
        /// <param name="OnSurfaceDetectedCallback">callback returning geoLocation, unity local position
        /// and Normal</param>
        public static void DetectSurfaceAtPoint(Vector2 screenPos, Action<GeoLocation, Vector3, Vector3> OnSurfaceDetectedCallback)
        {
            if (XRSessionManager.GetSession().Status != XRSessionStatus.Localized)
            {
                Debug.LogError(" Cannot detect surface until the session is localized");
                return;
            }

            RaycastHit hit;

            Ray ray = XRCamera.Camera.ScreenPointToRay(screenPos);
            //Debug.DrawRay(ray.origin, ray.direction * 10000, Color.green, 2000);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(SturfeeLayers.Building, SturfeeLayers.Terrain)))
            {
                GeoLocation hitLocation = new GeoLocation();
                hitLocation = Converters.UnityToGeoLocation(hit.point);

                OnSurfaceDetectedCallback(hitLocation, hit.point, hit.normal);
            }
        }
    }
}