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
            if (FindObjectsOfType<SturfeeUIManager>().Length > 1)
                Destroy(this.gameObject);

            SturfeeEventManager.OnLocalizationFail += OnLocalizationFail;
            SturfeeEventManager.OnTileLoadingFail += OnTileLoadingFail;
        }        

        private void OnDisable()
        {
            SturfeeEventManager.OnLocalizationFail -= OnLocalizationFail;
            SturfeeEventManager.OnTileLoadingFail -= OnTileLoadingFail;
        }

        private void OnLocalizationFail(string error)
        {
            MobileToastManager.Instance.ShowToast(error, -1, true);
        }

        private void OnTileLoadingFail(string error)
        {
            MobileToastManager.Instance.ShowToast(error,3);
        }
    }
}
