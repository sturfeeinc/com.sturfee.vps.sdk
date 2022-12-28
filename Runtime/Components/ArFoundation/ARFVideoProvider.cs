using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SturfeeVPS.SDK
{
    public class ARFVideoProvider : BaseVideoProvider
    {
        private Matrix4x4 _projectionMatrix;
        public override void OnRegister()
        {
            base.OnRegister();
            //_arfManager = FindObjectOfType<ARFManager>();

            ARFManager.CurrentInstance.ArCamera.GetComponent<ARCameraManager>().frameReceived += OnFrameReceived;

            ARSession.stateChanged += ARSession_stateChanged;
        }

        public override Texture2D GetCurrentFrame()
        {
            int width = GetWidth();
            int height = GetHeight();

            // Create a temporary render texture 
            RenderTexture tempRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            tempRT.depth = 24;

            Graphics.Blit(Texture2D.whiteTexture, tempRT, ARFManager.CurrentInstance.ArCamera.GetComponent<ARCameraBackground>().material);

            // Copy the tempRT to a regular texture by reading from the current render target (i.e. tempRT)
            var snap = new Texture2D(width, height);
            snap.ReadPixels(new Rect(0, 0, width, height), 0, 0, false); // ReadPixels(Rect source, ...) ==> Rectangular region of the view to read from. ***Pixels are read from current render target.***
            snap.Apply();

            Destroy(tempRT);

            return snap;
        }

        public override float GetFOV()
        {
            return 2.0f * Mathf.Atan(1.0f / GetProjectionMatrix()[1, 1]) * Mathf.Rad2Deg;
        }

        public override int GetHeight()
        {
            return (int)ResizedResolution().y;
        }

        public override Matrix4x4 GetProjectionMatrix()
        {
            return _projectionMatrix;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ARFManager.CurrentInstance.ProviderStatus;
        }

        public override int GetWidth()
        {
            return (int)ResizedResolution().x;
        }

        public override bool IsPortrait()
        {
            return Screen.orientation == ScreenOrientation.Portrait;
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (args.projectionMatrix.HasValue)
            {
                _projectionMatrix = args.projectionMatrix.Value;
            }
        }

        private void ARSession_stateChanged(ARSessionStateChangedEventArgs state)
        {
            if (state.state == ARSessionState.SessionTracking)
            {
                XrCamera.Camera.clearFlags = CameraClearFlags.Depth;
            }
        }

        private Vector2 ResizedResolution()
        {
            float aspectRatio = GetProjectionMatrix()[1, 1] / GetProjectionMatrix()[0, 0];
            float width, height;
            int divideBy = 1;

            if (IsPortrait())
            {
                width = Screen.width;
                height = width / aspectRatio;
                if (width > 720)
                {
                    divideBy = (int)width / 720;
                    if ((int)width % 720 != 0)
                    {
                        divideBy++;
                    }
                }
            }
            else
            {
                height = Screen.height;
                width = height * aspectRatio;
                if (height > 720)
                {
                    divideBy = (int)height / 720;

                    if ((int)height % 720 != 0)
                    {
                        divideBy++;
                    }
                }
            }

            return new Vector2(width / divideBy, height / divideBy);
        }
    }
}
