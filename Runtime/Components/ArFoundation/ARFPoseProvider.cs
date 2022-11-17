using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SturfeeVPS.SDK
{
    public class ARFPoseProvider : BasePoseProvider
    {
        private float _heightFromGround;

        public override void OnRegister()
        {
            base.OnRegister();
            if(ARFManager.CurrentInstance.ARAnchorManager.trackables.count == 0)
            {
                GetComponent<StartMessage>().enabled = true;
            }
        }

        public override float GetHeightFromGround()
        {
            if(_heightFromGround != 0)
            {
                return _heightFromGround;
            }

            float height = 1.5f;
            if (ARFManager.CurrentInstance.ARAnchorManager.trackables.count > 0)
            {
                if(ARFManager.CurrentInstance.ARPlaneManager.trackables.count == 0)
                {
                    SturfeeDebug.Log($"Anchors available but no planes available");
                    return height;
                }

                foreach (ARPlane arPlane in ARFManager.CurrentInstance.ARPlaneManager.trackables)
                {
                    float planeHeight = -arPlane.transform.position.y;
                    if (planeHeight >= 1.2f && planeHeight <= 1.7f)
                    {
                        height = (height + planeHeight) / 2;
                        _heightFromGround = height;
                    }
                }
            }
            return height;
        }

        public override Vector3 GetPosition(out bool includesElevation)
        {
            includesElevation = false;
            var updatedPose = ARPoseDriver_Custom.GetPoseData();

            if (updatedPose.position.HasValue)
            {
                return Converters.UnityToWorldPosition(updatedPose.position.Value);
            }
            else
                return Converters.UnityToWorldPosition(
                    ARFManager.CurrentInstance.ArCamera.transform.position);
        }

        public override ProviderStatus GetProviderStatus()
        {
            if (ARFManager.CurrentInstance.ProviderStatus != ProviderStatus.Ready)
            {
                return ARFManager.CurrentInstance.ProviderStatus;
            }

            // Need atleast 1 trackable anchor
            if (ARFManager.CurrentInstance.ARAnchorManager.trackables.count == 0)
                return ProviderStatus.Initializing;
            
            return ProviderStatus.Ready;
        }

        public override Quaternion GetRotation()
        {
            var updatedPose = ARPoseDriver_Custom.GetPoseData();

            if (updatedPose.rotation.HasValue)
                return Converters.UnityToWorldRotation(updatedPose.rotation.Value);
            else
                return Converters.UnityToWorldRotation(ARFManager.CurrentInstance.ArCamera.transform.rotation);
        }
    }
}
