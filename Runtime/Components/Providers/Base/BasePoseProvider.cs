using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BasePoseProvider : BaseProvider, IPoseProvider
    {
        public abstract float GetHeightFromGround();

        public abstract Vector3 GetPosition(out bool includesElevation);

        public abstract Quaternion GetRotation();

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
