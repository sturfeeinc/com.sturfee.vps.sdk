using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ForceLayer : MonoBehaviour
    {
        public string LayerName = "";
        public bool SetOnChildren = true;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer(LayerName);
            foreach (Transform child in transform)
            {
                child.gameObject.layer= LayerMask.NameToLayer(LayerName);
            }
        }
    }
}