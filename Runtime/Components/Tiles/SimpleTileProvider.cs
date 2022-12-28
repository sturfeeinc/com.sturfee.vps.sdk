using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SimpleTileProvider : BaseTilesProvider
    {
        public override async void OnRegister()
        {
            base.OnRegister();
            await LoadTiles();
        }

        public override float GetElevation(GeoLocation location)
        {
            return 15;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }


        public override async Task<GameObject> GetTiles(GeoLocation location, float radius = 0, CancellationToken cancellationToken = default)
        {
            return await LoadTiles();
        }

        public override async Task<GameObject> GetTiles(CancellationToken cancellationToken = default)
        {
            return await LoadTiles();
        }

        public async Task<GameObject> LoadTiles()
        {
            await Task.Delay(1000);

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.localScale = Vector3.one * 100;
            quad.transform.Translate(0, 15, 0);

            return quad;
        }
    }
}
