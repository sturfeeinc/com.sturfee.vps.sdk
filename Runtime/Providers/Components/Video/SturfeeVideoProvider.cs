using System.Collections;
using SturfeeVPS.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


namespace SturfeeVPS.Providers
{
    /// <summary>
    /// Sturfee's VideoProvider.
    /// </summary>
    public class SturfeeVideoProvider : VideoProviderBase
    {
        private WebCamTexture _activeCameraTexture;
        private RawImage _rawImage;

        private Camera _backgroundCamera;
        private AspectRatioFitter _imageFitter;


        // Image rotation
        Vector3 rotationVector = new Vector3(0f, 0f, 0f);

        // Image uvRect
        Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
        Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

        // Image Parent's scale
        Vector3 defaultScale = new Vector3(1f, 1f, 1f);
        Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

        private bool _initialized = false;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => XRSessionManager.GetSession() != null);

            CreateBackgroundCameraAndCanvas();

            Initialize();
        }

        // Update is called once per frame
        public void Update()
        {
            if (!_initialized)
            {
                return;
            }

            // Skip making adjustment for incorrect camera data
            if (_activeCameraTexture.width < 100)
            {
                return;
            }

            FixCameraTexture();
        }

        public void OnDestroy()
        {
            Debug.Log("Stopping Camera...");
            _activeCameraTexture.Stop();
        }

        public override Texture2D GetCurrentFrame()
        {
            var snap = new Texture2D(_activeCameraTexture.width, _activeCameraTexture.height);
            snap.SetPixels(_activeCameraTexture.GetPixels());
            snap.Apply();

            return snap;
        }

        public override int GetHeight()
        {
            return Screen.height;
        }

        public override int GetWidth()
        {
            return Screen.width;
        }

        public override bool IsPortrait()
        {
            return Screen.orientation == ScreenOrientation.Portrait;
        }

        public override Matrix4x4 GetProjectionMatrix()
        {
            return GameObject.FindWithTag(SturfeeObjects.XRCamera).GetComponent<Camera>().projectionMatrix;
        }

        public override float GetFOV()
        {
            return GameObject.FindWithTag(SturfeeObjects.XRCamera).GetComponent<Camera>().fieldOfView;
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
            UnityEngine.Object.Destroy(GameObject.Find("Sturfee Video Provider Bg Render Canvas"));
            UnityEngine.Object.Destroy(GameObject.Find(SturfeeObjects.BackgroundCamera));
        }

        private void Initialize()
        {

            //set up camera
            WebCamDevice[] devices = WebCamTexture.devices;
            string backCamName = "";
            for (int i = 0; i < devices.Length; i++)
            {

                if (!devices[i].isFrontFacing)
                {
                    backCamName = devices[i].name;
                }
            }

            //			_activeCameraTexture = new WebCamTexture(backCamName, Screen.width, Screen.height, 24);
            _activeCameraTexture = new WebCamTexture(backCamName, Screen.width, Screen.height, 24);
            _activeCameraTexture.Play();
            _rawImage.texture = _activeCameraTexture;

            _initialized = true;
        }

        private void CreateBackgroundCameraAndCanvas()
        {
            _backgroundCamera = new GameObject().AddComponent<Camera>();
            _backgroundCamera.name = SturfeeObjects.BackgroundCamera;
            _backgroundCamera.depth = -100;
            _backgroundCamera.nearClipPlane = 0.1f;
            _backgroundCamera.farClipPlane = 2000f;
            _backgroundCamera.orthographic = true;
            _backgroundCamera.clearFlags = CameraClearFlags.Color;
            _backgroundCamera.backgroundColor = Color.black;
            _backgroundCamera.renderingPath = RenderingPath.Forward;

            // add to proper layer and set culling properties
            _backgroundCamera.gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.Background);
            _backgroundCamera.cullingMask = 1 << LayerMask.NameToLayer(SturfeeLayers.Background);


            var canvas = new GameObject().AddComponent<Canvas>();
            canvas.name = "Sturfee Video Provider Bg Render Canvas";
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            canvas.worldCamera = _backgroundCamera;
            //canvas.planeDistance = XRSessionManager.GetSession().CameraProvider.GetFarClippingPlane() - 50.0f;
            canvas.gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.Background);

            _rawImage = new GameObject().AddComponent<RawImage>();
            _rawImage.name = "Raw Image";
            _rawImage.transform.parent = canvas.transform;
            _rawImage.transform.localPosition = Vector3.zero;

            _imageFitter = _rawImage.gameObject.AddComponent<AspectRatioFitter>();
            //_imageFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            _imageFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            //_imageFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                //_imageFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                _rawImage.rectTransform.sizeDelta = new Vector2(0, Screen.width);
            }
            else
            {
                //_imageFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                _rawImage.rectTransform.sizeDelta = new Vector2(0, Screen.height);
            }

            _backgroundCamera.gameObject.AddComponent<BackgroundCameraRenderer>();
        }

        private void FixCameraTexture()
        {
            //            if (_activeCameraTexture.videoRotationAngle == 180)
            //            {
            //                Debug.Log("Camera is horizontally and vertically flipped. (angle=" + _activeCameraTexture.videoRotationAngle + "). Fixing...");
            //                FlipCameraY();
            //                FlipCameraX();
            //            }
            //            else if (_activeCameraTexture.videoRotationAngle == 270)
            //            {
            //                Debug.Log("Camera is horizontally and vertically flipped. (angle=" + _activeCameraTexture.videoRotationAngle + "). Fixing...");
            //                FlipCameraY();
            //                FlipCameraX();
            //            }
            //
            //            if (Platform == PlatformType.IOS)
            //            {
            //                FlipCameraY();
            //            }
            //

            // Set AspectRatioFitter's ratio
            float videoRatio = (float)_activeCameraTexture.width / (float)_activeCameraTexture.height;
            //float videoRatio = (float)_activeCameraTexture.height / (float)_activeCameraTexture.width;

            _imageFitter.aspectRatio = videoRatio;

            //Screen.SetResolution(_activeCameraTexture.width, _activeCameraTexture.height, true);

            //Check if we need to flip
            float scaleY = _activeCameraTexture.videoVerticallyMirrored ? -1f : 1f;
            _rawImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

            //Adjust based on orientation
            int orient = -_activeCameraTexture.videoRotationAngle;
            //int orient = -90; // for testing portrait mode

            _rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        }
    
    }

}
