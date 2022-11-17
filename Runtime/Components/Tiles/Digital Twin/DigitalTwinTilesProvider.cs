using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class DigitalTwinTilesProvider : BaseTilesProvider
    {
        public float TileRadius = 300;

        [SerializeField][ReadOnly]
        private GameObject _tilesGameObject;
        [SerializeField][ReadOnly]
        private ProviderStatus _providerStatus;

        public override async void OnRegister()
        {
            base.OnRegister();
            if (_tilesGameObject != null)
            {
                _tilesGameObject.SetActive(true);
                _providerStatus = ProviderStatus.Ready;
                return;
            }

            _providerStatus = ProviderStatus.Initializing;
            _tilesGameObject = await LoadTiles(Converters.UnityToGeoLocation(Vector3.zero), TileRadius);
            _providerStatus = ProviderStatus.Ready;
        }

        public override float GetElevation(GeoLocation location)
        {
            RaycastHit hit;

            Vector3 unityPos = Converters.GeoToUnityPosition(location);
            unityPos.y += 100;

            Ray ray = new Ray(unityPos, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 10000, Color.red, 2000);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(SturfeeLayers.DigitalTwinTerrain)))
            {
                float elevation = hit.point.y;
                SturfeeDebug.Log("Elevation : " + elevation);
                return elevation;
            }

            return 0;
        }    

        public override Task<GameObject> LoadTiles(GeoLocation location, float radius, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }
    }
}
