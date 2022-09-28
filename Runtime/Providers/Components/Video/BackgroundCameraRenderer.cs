using UnityEngine;

namespace SturfeeVPS.Providers
{
    public class BackgroundCameraRenderer : MonoBehaviour
    {
        public RenderTexture EncoderRT;
        private RenderTexture _displayRT;
        //public Texture2D PortraitTexture { get { return _portraitTex; } }

        private Camera _camera;
        private ScreenOrientation _lastOrientation = ScreenOrientation.Unknown;
        
        private float _targetAspect = 16.0f / 9.0f;

        private Material _uvAspectMaterial;
        private Vector2 _aspectScale = Vector2.one;

        // Use this for initialization
        private void Start()
        {
            _uvAspectMaterial = Resources.Load("Materials/UvAspect") as Material;

            _camera = GetComponent<Camera>();

            SetCameraViewport();
            //_uvAspectMaterial.SetVector("_PlaneScale", new Vector4(0.5f, 1, 0, 0));
        }

        private void Update()
        {
            if (Screen.orientation != _lastOrientation)
            {
                int width = 1280;
                int height = 720;

                if (Screen.orientation == ScreenOrientation.Portrait)
                {
                    width = 720;
                    height = 1280;
                }
                else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
                {
                    width = 1280;
                    height = 720;
                }

                _displayRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                _displayRT.wrapMode = TextureWrapMode.Clamp;
                _displayRT.filterMode = FilterMode.Point;
                _displayRT.Create();

                EncoderRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                EncoderRT.wrapMode = TextureWrapMode.Clamp;
                EncoderRT.filterMode = FilterMode.Point;
                EncoderRT.Create();

                //SetCameraViewport();

                _lastOrientation = Screen.orientation;
            }
        }

        private void OnPreRender()
        {
            // render what the camera sees to the render texture
            _camera.targetTexture = _displayRT;
            
            _camera.aspect = (float)Screen.width / (float)Screen.height;

            SetCameraViewport();

            Graphics.Blit(_displayRT, EncoderRT, _uvAspectMaterial);
        }

        private void OnPostRender()
        {
            // You have to set target texture to null for the Blit below to work
            _camera.targetTexture = null;
            Graphics.Blit(_displayRT, null as RenderTexture);            
        }

        private void SetCameraViewport()
        {
            _targetAspect = 16.0f / 9.0f;

            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                _targetAspect = 9.0f / 16.0f;
            }

            // determine the game window's current aspect ratio
            float windowaspect = (float)Screen.width / (float)Screen.height;

            // current viewport height should be scaled by this amount
            float scaleheight = windowaspect / _targetAspect;

            // if scaled height is less than current height, add letterbox
            if (scaleheight < 1.0f)
            {
                Rect rect = _camera.rect;

                rect.width = 1.0f;
                rect.height = scaleheight;
                rect.x = 0;
                rect.y = (1.0f - scaleheight) / 2.0f;

                //_camera.rect = rect;
                _uvAspectMaterial.SetVector("_PlaneScale", new Vector4(1, scaleheight, 0, 0));

                _aspectScale.x = 1;
                _aspectScale.y = scaleheight;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleheight;

                Rect rect = _camera.rect;

                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;

                //_camera.rect = rect;
                _uvAspectMaterial.SetVector("_PlaneScale", new Vector4(scalewidth, 1, 0, 0));

                _aspectScale.x = scalewidth;
                _aspectScale.y = 1;
            }
        }
    }
}