using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BaseTilesProvider : BaseProvider, ITilesProvider
    {
        public event TilesLoadedAction OnTileLoaded;

        public abstract float GetElevation(GeoLocation location);
        public abstract Task<GameObject> GetTiles(GeoLocation location, float radius = 0, CancellationToken cancellationToken = default);
        public abstract Task<GameObject> GetTiles(CancellationToken cancellationToken = default);

        public override void OnRegister()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
        public override void OnUnregister()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void TriggerTileLoadEvent()
        {
            OnTileLoaded?.Invoke();
        }
    }
}
