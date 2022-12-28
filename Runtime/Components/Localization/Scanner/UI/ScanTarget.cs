using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public class ScanTarget : MonoBehaviour
    {
        [SerializeField]
        private Image _image;

        public void SetActive(bool active = true)
        {
            _image.enabled = active;
        }
    }
}
