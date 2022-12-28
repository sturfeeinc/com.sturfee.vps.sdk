using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SturfeeVPS.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.SDK.Samples
{
    public class PhotosphereVideoProvider : BaseVideoProvider
    {
        [SerializeField]
        private GameObject _sphere;
        [SerializeField]
        private Camera _camera;

        private ProviderStatus _providerStatus = ProviderStatus.Initializing;
        private RemoteARManager _remoteManager;

        private void Update()
        {
            if (_camera != null)
            {
                _camera.projectionMatrix = GetProjectionMatrix();
                _camera.transform.position = XrCamera.Pose.Position;
                _camera.transform.rotation = XrCamera.Pose.Rotation;
            }

            if(_sphere != null)
            {
                _sphere.transform.position = XrCamera.Pose.Position;
            }            
        }

        public override async void OnRegister()
        {
            _remoteManager = FindObjectOfType<PhotosphereManager>();
            await _remoteManager.GetRemoteDataAsync();
            var image = await LoadImageAsync();
            SetupPhotosphere(image);

            //_remoteManager = GetComponent<RemoteARManager>();
            //_remoteManager.OnRemoteDataReady += () =>
            //{
            //    LoadImage((image) =>
            //    {
            //        SetupPhotosphere(image);
            //    });
            //};
            //_remoteManager.LoadData();
        }

        public override Texture2D GetCurrentFrame()
        {
            int width =  GetWidth();
            int height = GetHeight();


            RenderTexture activeRenderTexture = RenderTexture.active;

            // Create a temporary render texture 
            RenderTexture tempRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            _camera.targetTexture = tempRT;
            _camera.Render();
            RenderTexture.active = tempRT;

            // Copy the tempRT to a regular texture by reading from the current render target (i.e. tempRT)
            var snap = new Texture2D(width, height, TextureFormat.ARGB32, false);
            snap.ReadPixels(new Rect(0, 0, width, height), 0, 0); // ReadPixels(Rect source, ...) ==> Rectangular region of the view to read from. ***Pixels are read from current render target.***
            snap.Apply();

            RenderTexture.ReleaseTemporary(tempRT);

            _camera.targetTexture = null;
            RenderTexture.active = activeRenderTexture;

            return snap;
        }

        public override Matrix4x4 GetProjectionMatrix()
        {
            return _camera.projectionMatrix;
        }

        public override float GetFOV()
        {
            return 2.0f * Mathf.Atan(1.0f / GetProjectionMatrix()[1, 1]) * Mathf.Rad2Deg;
        }

        public override int GetWidth()
        {
            return (int)ResizedResolution().x;
        }

        public override int GetHeight()
        {
            return (int)ResizedResolution().y;
        }

        public override bool IsPortrait()
        {
            return Screen.orientation == ScreenOrientation.Portrait;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }

        private void LoadImage(Action<Texture2D> callback){

            var image = LoadFromCache();

            if(image != null)
            {
                callback(image);
            }
            else
            {
                StartCoroutine(DownloadImage((texture2D) =>
                {
                    callback(texture2D);
                }));
            }
        }

        private async Task<Texture2D> LoadImageAsync()
        {
            var image = LoadFromCache();

            if (image == null)
            {
                image = await DownloadImageAsync();                
            }

            return image;            
        }

        private Texture2D LoadFromCache(){
            string file = Path.Combine(_remoteManager.CacheDirectory, "0.jpg");

            if (File.Exists(file))
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(File.ReadAllBytes(Path.Combine(_remoteManager.CacheDirectory, "0.jpg")));

                return texture2D;
            }

            return null;
        }        

        private IEnumerator DownloadImage(Action<Texture2D> callback)
        {
            string url = "https://sdk-remote-scene.s3.amazonaws.com/" + _remoteManager.SampleName + "/0.jpg";

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
            yield return unityWebRequest.SendWebRequest();

            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(unityWebRequest.downloadHandler.data);

                if (_remoteManager.Cache)
                {
                    File.WriteAllBytes(Path.Combine(_remoteManager.CacheDirectory, "0.jpg"), unityWebRequest.downloadHandler.data);
                }

                callback(texture2D);
            }
            else
            {
                Debug.LogError(unityWebRequest.error);
            }
        }

        private async Task<Texture2D> DownloadImageAsync()
        {
            string url = "https://sdk-remote-scene.s3.amazonaws.com/" + _remoteManager.SampleName + "/0.jpg";
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
            await unityWebRequest.SendWebRequest();

            if(string.IsNullOrEmpty(unityWebRequest.error))
            {
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(unityWebRequest.downloadHandler.data);

                if (_remoteManager.Cache)
                {
                    File.WriteAllBytes(Path.Combine(_remoteManager.CacheDirectory, "0.jpg"), unityWebRequest.downloadHandler.data);
                }

                return texture2D;
            }
            else
            {
                Debug.LogError(unityWebRequest.error);
            }

            return null;
        }

        private void SetupPhotosphere(Texture2D equirectangularImage)
        {
            _sphere.GetComponent<MeshRenderer>().material.mainTexture = equirectangularImage;
            _sphere.layer = LayerMask.NameToLayer(SturfeeLayers.Background);
            _camera.cullingMask |= 1 << LayerMask.NameToLayer(SturfeeLayers.Background);

            _providerStatus = ProviderStatus.Ready;
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