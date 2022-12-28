using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SimpleGpsProvider : BaseGpsProvider
    {
        public GeoLocation Location;

        public override GeoLocation GetApproximateLocation(out bool includesElevation)
        {
            includesElevation = false;
            return Location;
        }

        public override GeoLocation GetFineLocation(out bool includesElevation)
        {
            includesElevation = false;
            return Location;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }
    }
}
