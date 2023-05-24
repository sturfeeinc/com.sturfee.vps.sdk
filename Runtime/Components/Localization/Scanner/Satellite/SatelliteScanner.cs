using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SatelliteScanner : MultiframeScanner
    {
        public override async Task Initialize(uint requestNum)
        {
            await WaitForSessionProviders();
            SturfeeDebug.Log($" [SatelliteScanner] :: Providers ready");

            XrCameraController.CurrentInstance.ControlType = XrCameraControlType.VpsSatellite;

            var location = XrSessionManager.GetSession().GetProvider<IGpsProvider>().GetFineLocation(out _);
            await ValidateToken(TokenUtils.GetVpsToken());
            bool inCoverage = await CheckCoverage(location);

            if (inCoverage)
            {
                _localizationService = CreateLocalizationService();
                await _localizationService.Connect(_serviceUrl, TokenUtils.GetVpsToken(), location.Latitude, location.Longitude);                
            }

            await base.Initialize(requestNum);

            SturfeeDebug.Log($"{ScanType}Scanner initialized");

        }
    }
}
