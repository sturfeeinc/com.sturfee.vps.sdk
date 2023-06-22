using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections.LowLevel.Unsafe;

namespace SturfeeVPS.SDK
{
    public class ARFVideoProvider : BaseVideoProvider
    {
        public bool ClearFrameData = true;

        private Matrix4x4 _projectionMatrix;
        private Matrix4x4 _localizationProjectionMatrix;
        private Matrix4x4 _xrCameraProjectionMatrix;

        private ARCameraManager _arCameraManager;

        private Texture2D _currentFrame = null;
        private TextureFormat _format = TextureFormat.RGBA32;

        private int _imageCounter = 0;

        private int _maxWidth = 720;

        private bool _isLocalized = false;

        private void Update()
        {
            _projectionMatrix = _isLocalized ? _xrCameraProjectionMatrix : _localizationProjectionMatrix;
        }

        public override void OnRegister()
        {
            base.OnRegister();
            //_arfManager = FindObjectOfType<ARFManager>();

            _currentFrame = null;
            _currentFrame = new Texture2D(2, 2, _format, false);

            _arCameraManager = ARFManager.CurrentInstance.ArCamera.GetComponent<ARCameraManager>();
            _arCameraManager.frameReceived += OnFrameReceived;

            ARSession.stateChanged += ARSession_stateChanged;

            SturfeeEventManager.OnLocalizationStart += HandleResetLocalization;
            SturfeeEventManager.OnLocalizationDisabled += HandleResetLocalization;
            SturfeeEventManager.OnLocalizationSuccessful += HandleLocalizationSuccess;
        }

        public override void OnUnregister()
        {
            base.OnUnregister();

            _arCameraManager.frameReceived -= OnFrameReceived;

            ARSession.stateChanged -= ARSession_stateChanged;

            SturfeeEventManager.OnLocalizationStart -= HandleResetLocalization;
            SturfeeEventManager.OnLocalizationDisabled -= HandleResetLocalization;
            SturfeeEventManager.OnLocalizationSuccessful -= HandleLocalizationSuccess;
        }

        private void OnDestroy()
        {
            _arCameraManager.frameReceived -= OnFrameReceived;

            ARSession.stateChanged -= ARSession_stateChanged;

            SturfeeEventManager.OnLocalizationStart -= HandleResetLocalization;
            SturfeeEventManager.OnLocalizationDisabled -= HandleResetLocalization;
            SturfeeEventManager.OnLocalizationSuccessful -= HandleLocalizationSuccess;
        }

        private void HandleLocalizationSuccess()
        {
            _isLocalized = true;
        }

        private void HandleResetLocalization()
        {
            _isLocalized = false;
            _imageCounter = 0;
        }

        unsafe public override Texture2D GetCurrentFrame()
        {
            // Attempt to get the latest camera image. If this method succeds it acquires a native resource that must be disposed
            if (_arCameraManager == null) { return _currentFrame; }

            Texture2D frame = _currentFrame;

            _imageCounter++;

            var dirPath = Path.Combine(Application.persistentDataPath, "VPS-IMAGES"); // , $"{_saveId}");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (_arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage xrCpuImage))
            {
                if (xrCpuImage != null)
                {
                    try
                    {
                        if (ClearFrameData && _currentFrame != null) Destroy(_currentFrame);
                        // store the most recent frame received for use by the SDK     
                        _currentFrame = new Texture2D(xrCpuImage.width, xrCpuImage.height, _format, false);
                        var conversionParams = new XRCpuImage.ConversionParams(xrCpuImage, _format, XRCpuImage.Transformation.MirrorX);
                        var rawTextureData = _currentFrame.GetRawTextureData<byte>();
                        xrCpuImage.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
                        _currentFrame.Apply();

                        rawTextureData.Dispose();

                        // build the frame to share with the SDK (rotate, scale, etc)...

                        // rotate the texture clockwise 90 degrees
                        frame = RotateTexture(_currentFrame, true);
                        // resize to <WIDTH> x 720 -- maintain aspect
                        var aspect = (float)xrCpuImage.width / (float)xrCpuImage.height;
                        frame = ResizeTexture(frame, _maxWidth, (int)(_maxWidth * aspect));

                        ////then Save To Disk as JPG -- this is required for tracking fix (brent)
                        //byte[] bytes = frame.EncodeToJPG();
                        //File.WriteAllBytes(Path.Combine(dirPath, $"{_imageCounter}.jpg"), bytes);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        //throw;
                    }
                    finally
                    {
                        xrCpuImage.Dispose();
                    }

                    // save screen capture too -- this is required for tracking fix (brent)
                    RenderTexture tempRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                    tempRT.depth = 24;
                    try
                    {
                        Graphics.Blit(Texture2D.whiteTexture, tempRT, ARFManager.CurrentInstance.ArCamera.GetComponent<ARCameraBackground>().material);
                        // Copy the tempRT to a regular texture by reading from the current render target (i.e. tempRT)
                        var snap = new Texture2D(Screen.width, Screen.height);
                        snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false); // ReadPixels(Rect source, ...) ==> Rectangular region of the view to read from. ***Pixels are read from current render target.***
                        snap.Apply();

                        ////then Save To Disk as JPG -- this is required for tracking fix (brent)
                        //byte[] bytes2 = snap.EncodeToJPG();
                        //File.WriteAllBytes(Path.Combine(dirPath, $"SCREEN_{_imageCounter}.jpg"), bytes2);

                        //WriteImageDataToFile(snap, Path.Combine(dirPath, $"SCREEN_{_imageCounter}.jpg"));

                        Destroy(snap);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        //throw;
                    }
                    finally
                    {
                        Destroy(tempRT);
                    }
                }
            }

            if (_currentFrame != null)
            {
                if (_arCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
                {
                    _localizationProjectionMatrix = IntrinsicsToMatrix(cameraIntrinsics);//, width, height);
                }
            }

            return frame;
        }

        public override float GetFOV()
        {
            return 2.0f * Mathf.Atan(1.0f / GetProjectionMatrix()[1, 1]) * Mathf.Rad2Deg;
        }

        public override int GetHeight()
        {
            //return _currentFrame != null ? _currentFrame.height : 0;
            return _currentFrame != null ? _currentFrame.width : 0;
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
            //return _currentFrame != null ? _currentFrame.width : 0;
            return _currentFrame != null ? _currentFrame.height : 0;
        }

        public override bool IsPortrait()
        {
            return Screen.orientation == ScreenOrientation.Portrait;
        }

        unsafe private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (args.projectionMatrix.HasValue)
            {
                _xrCameraProjectionMatrix = args.projectionMatrix.Value;
            }

            if (_arCameraManager != null && _arCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
            {
                _localizationProjectionMatrix = IntrinsicsToMatrix(cameraIntrinsics);//, width, height);
            }
        }

        private void ARSession_stateChanged(ARSessionStateChangedEventArgs state)
        {
            Debug.Log($"[ARFVideoProvider] :: ARSession_stateChanged => {state}");

            if (state.state == ARSessionState.SessionTracking)
            {
                XrCamera.Camera.clearFlags = CameraClearFlags.Depth;
            }
        }

        private Matrix4x4 IntrinsicsToMatrix(XRCameraIntrinsics cameraIntrinsics) //, float inputWidth, float inputHeight)
        {
            var fx = cameraIntrinsics.focalLength.y;
            var fy = cameraIntrinsics.focalLength.x;
            var cx = cameraIntrinsics.principalPoint.y;
            var cy = cameraIntrinsics.principalPoint.x;

            var inputWidth = cameraIntrinsics.resolution.x;
            var inputHeight = cameraIntrinsics.resolution.y;

            // final image will be rotated, so width and height should swap
            var aspect = (float)inputWidth / (float)inputHeight;
            var width = (float)_maxWidth;
            var height = (float)(_maxWidth * aspect); // resize but maintain aspect          

            // deal with scaling everything based on the image resize
            var scalar = width / (float)inputHeight; // use height here because we swapped width and height
            fx *= scalar;
            fy *= scalar;
            cx *= scalar;
            cy *= scalar;

            var zNear = 0.01f;
            var zFar = 2000f;

            var result = new Matrix4x4();

            result.m00 = 2f * fx / width;
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = 0;

            result.m10 = 0;
            result.m11 = 2f * fy / height;
            result.m12 = 0;
            result.m13 = 0;

            result.m20 = 1f - 2f * cx / width;
            result.m21 = 2f * cy / height - 1f;
            result.m22 = (zFar + zNear) / (zNear - zFar);
            result.m23 = -1;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 2f * zFar * zNear / (zNear - zFar);
            result.m33 = 0;

            return result;
        }

        private Texture2D ResizeTexture(Texture2D input, int width, int height)
        {
            // Create a temporary render texture 
            RenderTexture tempRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            tempRT.depth = 24;

            Graphics.Blit(input, tempRT);

            // Copy the tempRT to a regular texture by reading from the current render target (i.e. tempRT)
            var snap = new Texture2D(width, height);
            snap.ReadPixels(new Rect(0, 0, width, height), 0, 0, false); // ReadPixels(Rect source, ...) ==> Rectangular region of the view to read from. ***Pixels are read from current render target.***
            snap.Apply();

            Destroy(tempRT);

            return snap;
        }

        private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }
    }
}
