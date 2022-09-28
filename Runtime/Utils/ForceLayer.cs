using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ForceLayer : MonoBehaviour
    {
        public string LayerName = "";

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer(LayerName);
        }
    }
}