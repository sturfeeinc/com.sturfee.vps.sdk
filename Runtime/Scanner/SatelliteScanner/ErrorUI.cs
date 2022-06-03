using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

public class ErrorUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _topError;
    [SerializeField]
    private GameObject _botError;

    private void Start()
    {
        
    }

    private void Update()
    {
        float pitch = XRCamera.Pose.Rotation.eulerAngles.x;
        // set to [-180, 180] range
        pitch = pitch > 180 ? pitch - 360 : pitch;

        _topError.SetActive(false);
        _botError.SetActive(false);


        if (pitch < ScanProperties.PitchMin)
        {
            _topError.SetActive(true);
        }
        else if (pitch > ScanProperties.PitchMax)
        {
            _botError.SetActive(true);
        }

    }
}
