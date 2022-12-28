using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ErrorUI : MonoBehaviour
    {
        public float PitchMin = -30f;
        public float PitchMax = -5f;

        [SerializeField]
        private GameObject _topError;
        [SerializeField]
        private GameObject _botError;

        private void Start()
        {

        }

        private void Update()
        {
            float pitch = XrCamera.Pose.Rotation.eulerAngles.x;
            // set to [-180, 180] range
            pitch = pitch > 180 ? pitch - 360 : pitch;

            _topError.SetActive(false);
            _botError.SetActive(false);


            if (pitch < PitchMin)
            {
                _topError.SetActive(true);
            }
            else if (pitch > PitchMax)
            {
                _botError.SetActive(true);
            }

        }
    }
}