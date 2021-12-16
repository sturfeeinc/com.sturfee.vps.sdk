using SturfeeVPS.Core;
using UnityEngine;

namespace SturfeeVPS.Providers
{
    public class SturfeePoseProvider : PoseProviderBase
    {
        public override void Initialize()
        {            
            Input.gyro.enabled = true;
        }

        public override Quaternion GetOrientation()
        {
            return Input.gyro.attitude;
        }

        public override Vector3 GetPosition()
        {
            return new Vector3();		
        }

        /// <summary>
        /// Gets provider's current status
        /// </summary>
        /// <returns>The provider status.</returns>
        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }

        public override void Destroy()
        {

        }

    }
}
