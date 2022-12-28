using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SturfeeVPS.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.SDK.Samples
{
    public class RemoteARManager : MonoBehaviour
    {
        [Tooltip(" It is recommended to use Web Server mode only in Editor mode.")]
        public LocalizationMode LocalizationMode;
        [HideInInspector]
        public string SampleName;
        [HideInInspector]
        public bool Cache;

        public RemoteARData RemoteData { get; private set; }

        public string CacheDirectory
        {
            get
            {
                string directory = Path.Combine(Application.persistentDataPath, "Remote Data", SampleName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return directory;
            }
        }

        protected virtual async void Start()
        {
#if !UNITY_EDITOR
            LocalizationMode = LocalizationMode.Local;
#endif

            await GetRemoteDataAsync();
            var sturfeeXrSession = FindObjectOfType<SturfeeXrSession>();
            sturfeeXrSession.Location = RemoteData.sensorExternalParameters.location;
            sturfeeXrSession.CreateSession();

        }

        public async Task<RemoteARData> GetRemoteDataAsync()
        {
            if(RemoteData == null)
            {
                RemoteData = await LoadFileAsync("Data.txt");
            }

            return RemoteData;            
        }

        public void ClearCache()
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, true);

                Debug.Log("Cache cleared");
            }
        }

        private string ReadFromCache(string fileName)
        {
            string file = Path.Combine(CacheDirectory, fileName);

            if (File.Exists(file))
            {
                string dataJson = File.ReadAllText(file);
                return dataJson;
            }

            return null;
        }

        private async Task<RemoteARData> LoadFileAsync(string fileName)
        {
            string data = ReadFromCache(fileName);

            // Check cache
            if (!string.IsNullOrEmpty(data))
            {
                return JsonUtility.FromJson<RemoteARData>(data);
            }
            else
            {
                // Download from AWS                
                return await DownloadRemoteData(fileName);
            }
        }

        private async Task<RemoteARData> DownloadRemoteData(string filename)
        {
            string path = "https://sdk-remote-scene.s3.amazonaws.com/" + SampleName;
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(path + "/" + filename);
            await unityWebRequest.SendWebRequest();

            Debug.Log(unityWebRequest.url);

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                if (unityWebRequest.responseCode == 404)
                {
                    return null;
                }
                else
                {
                    Debug.LogError(unityWebRequest.error);
                }
            }
            else
            {
                string data = unityWebRequest.downloadHandler.text;
                // Save to Cache
                File.WriteAllText(CacheDirectory + "/" + filename, data);

                return JsonUtility.FromJson<RemoteARData>(data);
            }

            return null;
        }
    }
}


