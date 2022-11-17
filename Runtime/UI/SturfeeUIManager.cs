using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SturfeeUIManager : MonoBehaviour
    {
        private void OnEnable()
        {
            SturfeeEventManager.OnLocalizationFail += OnLocalizationFail;
        }

        private void OnDisable()
        {
            SturfeeEventManager.OnLocalizationFail += OnLocalizationFail;
        }

        private void OnLocalizationFail(string error)
        {
            MobileToastManager.Instance.ShowToast(error, -1, true);
        }
    }
}
