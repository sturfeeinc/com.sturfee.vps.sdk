using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ElevationProvider : SimpleSingleton<ElevationProvider>
    {
        public float GetElevation(GeoLocation location, float stepOffset = 0)
        {
            // Get elevation from Sturg/DT/HDSite tiles
            var terrainElevation = GetTerrainElevation(location);
            // Get a small elevation on top of terrain elevation if it exists.  For example => footpath
            var stepElevation = GetStepElevation(location, terrainElevation, stepOffset);

            return stepElevation;
        }

        public float GetTerrainElevation(GeoLocation location)
        {
            float elevation = 0;

            //check if Scanned using HD
            bool hdsiteElevationNotFound = true;
            var localizationProvider = XrSessionManager.GetSession().GetProvider<BaseLocalizationProvider>();
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                // if localized using HD scan
                if (localizationProvider.Scanner != null && localizationProvider.Scanner.ScanType == ScanType.HD)
                {
                    float hditeElevation = GetHDSiteElevation(location);
                    if (hditeElevation != 0)
                    {
                        hdsiteElevationNotFound = false;
                    }
                }
            }

            if (hdsiteElevationNotFound)
            {
                var tilesProvider = XrSessionManager.GetSession().GetProvider<ITilesProvider>();
                if (tilesProvider != null && tilesProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    elevation = tilesProvider.GetElevation(location);
                }
            }

            return elevation;
        }

        private float GetStepElevation(GeoLocation location, float terrainElevation, float stepOffset)
        {
            var stepElevation = terrainElevation;

            Vector3 localPos = Converters.GeoToUnityPosition(location);
            localPos.y = terrainElevation + stepOffset;

            Ray ray = new Ray(localPos, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                stepElevation = hit.point.y;
                Debug.Log($" step elevation : {stepElevation}");
            }
            return stepElevation;
        }

        private float GetHDSiteElevation(GeoLocation location)
        {
            RaycastHit hit;

            Vector3 unityPos = Converters.GeoToUnityPosition(location);
            unityPos.y += 100;

            Ray ray = new Ray(unityPos, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 10000, Color.blue, 2000);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(SturfeeLayers.HDSiteTerrain)))
            {
                float elevation = hit.point.y;
                //SturfeeDebug.Log("Elevation : " + elevation);
                return elevation;
            }

            return 0;
        }
    }
}
