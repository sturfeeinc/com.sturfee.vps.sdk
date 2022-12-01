using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class MobileVRCameraVideoProvider : BaseVideoProvider
    {        
        [SerializeField]
        private MobileVRCameraController _vrCamera;
        [SerializeField]
        private MobileVRCameraController _vrCameraPrefab;


        public override void OnRegister()
        {
            base.OnRegister();
            if(_vrCamera == null)
            {
                _vrCamera = Instantiate(_vrCameraPrefab, transform);
            }

            _vrCamera.gameObject.SetActive(true);
        }

        public override void OnUnregister()
        {
            base.OnUnregister();
            _vrCamera?.gameObject.SetActive(false);
        }

        public override Texture2D GetCurrentFrame()
        {
            throw new System.NotImplementedException();
        }

        public override float GetFOV()
        {
            return _vrCamera.Camera.fieldOfView;
        }

        public override int GetHeight()
        {
            return Screen.height;
        }

        public override Matrix4x4 GetProjectionMatrix()
        {
            return _vrCamera.Camera.projectionMatrix;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }

        public override int GetWidth()
        {
            return Screen.width;
        }

        public override bool IsPortrait()
        {
            return Screen.orientation == ScreenOrientation.Portrait;
        }
    }
}
