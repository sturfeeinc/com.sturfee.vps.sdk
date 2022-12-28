using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BaseGpsProvider : BaseProvider, IGpsProvider
    {
        public abstract GeoLocation GetApproximateLocation(out bool includesElevation);        

        public abstract GeoLocation GetFineLocation(out bool includesElevation);        

        //public abstract ProviderStatus GetProviderStatus();

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
    }
}
