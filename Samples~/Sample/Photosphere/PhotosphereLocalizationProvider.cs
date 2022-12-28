using SturfeeVPS.SDK;
using SturfeeVPS.SDK.Samples;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PhotosphereLocalizationProvider : SturfeeVPSLocalizationProvider
{
    private PhotosphereManager _photosphereManager;
    private async void Start()
    {
        _photosphereManager = FindObjectOfType<PhotosphereManager>();
        if(_photosphereManager.LocalizationMode == LocalizationMode.Local)
        {
            await Task.Delay(1000);

            var data = await _photosphereManager.GetRemoteDataAsync();

            _vpsLocation = data.localizationResponse.location;

            YawOffset = data.localizationResponse.yawOrientationCorrection;
            PitchOffset = data.localizationResponse.pitchOrientationCorrection;
            RollOffset = data.localizationResponse.rollOrientationCorrection;

            TriggerLocalizationSuccessfulEvent();
        }
    }

}
