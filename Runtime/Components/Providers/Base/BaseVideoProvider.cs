using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BaseVideoProvider : BaseProvider, IVideoProvider
    {
        public abstract Texture2D GetCurrentFrame();
        public abstract float GetFOV();
        public abstract int GetHeight();
        public abstract Matrix4x4 GetProjectionMatrix();
        
        public abstract int GetWidth();
        public abstract bool IsPortrait();

        //public abstract ProviderStatus GetProviderStatus();
        public override void OnRegister()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public override void OnUnregister()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
