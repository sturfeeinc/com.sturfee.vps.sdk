using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

public class SatelliteScanUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _captureUI;
    [SerializeField]
    private CanvasGroup _errorUI;

    private void Update()
    {
        _captureUI.alpha = 0;
        _errorUI.alpha = 0;

        float pitch = XRCamera.Pose.Rotation.eulerAngles.x;
        // set to [-180, 180] range
        pitch = pitch > 180 ? pitch - 360 : pitch;

        if (pitch >= ScanProperties.PitchMin && pitch <= ScanProperties.PitchMax)
        {
            _captureUI.alpha = 1;
        }
        else
        {
            _errorUI.alpha = 1;
        }
    }
}
