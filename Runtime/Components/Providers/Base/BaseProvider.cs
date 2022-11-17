using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BaseProvider : MonoBehaviour, IProvider
    {
        public abstract ProviderStatus GetProviderStatus();
        public abstract void OnRegister();
        public abstract void OnUnregister();
    }
}
    