using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.SDK.Samples
{
    public class PhotosphereGpsProvider : BaseGpsProvider
    {
        private RemoteARManager _remoteManager;
        private ProviderStatus _providerStatus = ProviderStatus.Initializing;
        
        public override async void OnRegister()
        {
            _remoteManager = FindObjectOfType<PhotosphereManager>();
            await _remoteManager.GetRemoteDataAsync();
            _providerStatus = ProviderStatus.Ready;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }


        public override GeoLocation GetApproximateLocation(out bool includesElevation)
        {
            includesElevation = false;
            return _remoteManager.RemoteData.sensorExternalParameters.location;
        }

        public override GeoLocation GetFineLocation(out bool includesElevation)
        {
            includesElevation = false;
            return _remoteManager.RemoteData.sensorExternalParameters.location;
        }
    }
}       