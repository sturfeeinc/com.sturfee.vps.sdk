using SturfeeVPS.Core;
using SturfeeVPS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityGLTF;
using UnityGLTF.Loader;

namespace SturfeeVPS.SDK.Providers
{
    public class SturgTilesProvider : BaseTilesProvider
    {
        public int CacheRadius = 100;
        public int CacheExpiry = 10;    // number of days
        public float TileRadius = 300;
        public Material Material;
        public bool ShowUI = true;

        [SerializeField]
        private Toggle _displayToggle;
        [SerializeField]
        private Toggle _occlusionToggle;
        [SerializeField]
        private GameObject _projector;

        private string _cacheDir;
        private readonly string _baseUrl = "https://api.sturfee.com/api/0.2.0";

        [SerializeField][ReadOnly]
        private GameObject _tilesGameObject;
        [SerializeField][ReadOnly]
        private ProviderStatus _providerStatus;

        public async override void OnRegister()
        {
            base.OnRegister();

            _displayToggle?.gameObject.SetActive(ShowUI);
            _occlusionToggle?.gameObject.SetActive(ShowUI);

            if (_tilesGameObject != null)
            {
                _tilesGameObject.SetActive(true);
                _providerStatus = ProviderStatus.Ready;
                return;
            }

            _providerStatus = ProviderStatus.Initializing;

            // load tiles at 0,0,0
            var location = Converters.UnityToGeoLocation(Vector3.zero);
            try
            {
                await VpsServices.CheckCoverage(location, TokenUtils.GetVpsToken());
                _tilesGameObject = await LoadTiles(location, TileRadius, TokenUtils.GetVpsToken());

                _providerStatus = ProviderStatus.Ready;
                SturfeeDebug.Log("SturgTileProvider ready");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                _displayToggle?.gameObject.SetActive(false);
                _occlusionToggle?.gameObject.SetActive(false);

                if (ex is HttpException httpException)
                {
                    if (httpException.ErrorCode == 501)
                    {
                        // not in coverage
                        SturfeeDebug.Log($"Cannot load Sturg. Not in coverage area");
                        _providerStatus = ProviderStatus.NotSupported;
                        return;
                    }

                    string localizedError = SturfeeLocalizationProvider.Instance.GetString(ErrorMessages.TileLoadingError.Item1, ErrorMessages.TileLoadingError.Item2);
                    TriggerTileLoadingFailEvent(localizedError);
                }
                throw;
            }
        }

        public override async Task<GameObject> GetTiles(GeoLocation location, float radius = 300, CancellationToken cancellationToken = default)
        {
            if (_tilesGameObject != null)
            {
                return _tilesGameObject;
            }

            return await LoadTiles(location, radius, TokenUtils.GetVpsToken(), cancellationToken);
        }

        public override Task<GameObject> GetTiles(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }        

        public override float GetElevation(GeoLocation location)
        {
            RaycastHit hit;

            Vector3 unityPos = Converters.GeoToUnityPosition(location);
            unityPos.y += 100;

            Ray ray = new Ray(unityPos, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 10000, Color.red, 2000);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(SturfeeLayers.SturgTerrain)))
            {
                float elevation = hit.point.y;
                //SturfeeDebug.Log("Elevation : " + elevation);
                return elevation;
            }

            return 0;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }

        public void ToggleOcclusion(bool noOcclusion)
        {
            _projector.SetActive(noOcclusion);
        }

        public void ToggleDisplay(bool isOn)
        {
            _tilesGameObject?.SetActive(isOn);
        }

        private async Task<GameObject> LoadTiles(GeoLocation location, float radius, string vpsToken, CancellationToken cancellationToken = default)
        {
            SturfeeDebug.Log(" Loading tiles...");

            if (_tilesGameObject != null)
            {
                GameObject.Destroy(_tilesGameObject);
            }

            _tilesGameObject = new GameObject(SturfeeObjects.SturgTiles);
            _tilesGameObject.transform.rotation = Quaternion.Euler(-90, 180, 0);
            _tilesGameObject.transform.parent = transform;

            string uri = Path.Combine(CacheDir, "sturfee_tiles.gltf");
            if (InCache(location))
            {
                SturfeeDebug.Log(" Loading from Cache");
                await LoadGltf(_tilesGameObject, uri, cancellationToken);
            }
            else
            {
                SturfeeDebug.Log(" Downloading tiles");
                try
                {
                    // FOR DEBUG
                    // Debug.Log($"AR Location: Latitude: {location.Latitude}, Longitude: {location.Longitude}");

                    byte[] gltf = await DownloadTiles(location, radius, vpsToken);
                    SaveToCache(gltf, location);
                }
                catch (HttpException e)
                {
                    SturfeeDebug.LogError(e.Message);
                    throw new IdException(ErrorMessages.TileDownloadingError);
                }

                await LoadGltf(_tilesGameObject, uri, cancellationToken);
            }

            SetupTiles(_tilesGameObject);
            SturfeeDebug.Log("Tiles loaded");

            await Task.Yield();

            TriggerTileLoadEvent();

            return _tilesGameObject;
        }

        private async Task LoadGltf(GameObject tilesGO, string uri, CancellationToken cancellationToken = default)
        {
            SturfeeDebug.Log(" Loading Gltf...");

            try
            {
                var importOptions = new ImportOptions
                {
                    AsyncCoroutineHelper = tilesGO.GetComponent<AsyncCoroutineHelper>() ?? tilesGO.AddComponent<AsyncCoroutineHelper>()
                };

                GLTFSceneImporter sceneImporter = null;

                ImporterFactory factory = ScriptableObject.CreateInstance<DefaultImporterFactory>();

                string directoryPath = URIHelper.GetDirectoryName(uri);
                importOptions.DataLoader = new FileLoader(directoryPath);
                sceneImporter = factory.CreateSceneImporter(
                    Path.GetFileName(uri),
                    importOptions
                    );

                sceneImporter.SceneParent = tilesGO.transform;
                sceneImporter.Collider = GLTFSceneImporter.ColliderType.Mesh;
                sceneImporter.MaximumLod = 300;
                sceneImporter.Timeout = 100;
                sceneImporter.IsMultithreaded = false;
                sceneImporter.CustomShaderName = "VR/SpatialMapping/Occlusion";

                await sceneImporter.LoadSceneAsync(0, true, null, cancellationToken);
                await sceneImporter.LoadSceneAsync(1, true, null, cancellationToken);

                if (importOptions.DataLoader != null)
                {
                    sceneImporter?.Dispose();
                    sceneImporter = null;
                    importOptions.DataLoader = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw new IdException(ErrorMessages.TileLoadingError);
            }
        }

        private bool InCache(GeoLocation location)
        {
            string tiles = Path.Combine(CacheDir, "sturfee_tiles.gltf");
            string tileCache = Path.Combine(CacheDir, "Cache.txt");

            if (!Directory.Exists(CacheDir))
                return false;

            if (File.Exists(tiles) && File.Exists(tileCache))
            {
                string json = File.ReadAllText(Path.Combine(CacheDir, "Cache.txt"));

                var tileCacheMetaData = JsonUtility.FromJson<TileCacheMetaData>(json);

                var centerReference = tileCacheMetaData.Location;

                if (centerReference == null)
                {
                    throw new IdException(ErrorMessages.TileLoadingErrorFromCache);
                }

                SturfeeDebug.Log("Center Ref (Cache): " + centerReference.ToFormattedString(), false);

                UtmPosition centerUTM = GeoCoordinateConverter.GpsToUtm(centerReference);
                UtmPosition utmPosition = GeoCoordinateConverter.GpsToUtm(location);

                // if within cache radius
                if (Vector3.Distance(utmPosition.ToUnityVector3(), centerUTM.ToUnityVector3()) > CacheRadius)
                {
                    return false;
                }

                // if within expiry time-limt
                DateTime cached = new DateTime();
                if (DateTime.TryParse(tileCacheMetaData.TimeStamp, out cached))
                {
                    if ((DateTime.Now - cached).TotalDays > CacheExpiry)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;

        }

        private void SaveToCache(byte[] gltf, GeoLocation location)
        {
            SturfeeDebug.Log("Center Ref (Server): " + location.ToFormattedString(), false);

            if (!Directory.Exists(CacheDir))
            {
                Directory.CreateDirectory(CacheDir);
            }

            string gltfPath = Path.Combine(CacheDir, "sturfee_tiles.gltf");
            if (File.Exists(gltfPath))
            {
                File.Delete(gltfPath);
            }

            File.WriteAllBytes(gltfPath, gltf);
            TileCacheMetaData tileCacheMetaData = new TileCacheMetaData
            {
                Location = location,
                TimeStamp = DateTime.Now.ToString("O")
            };

            File.WriteAllText(Path.Combine(CacheDir, "Cache.txt"), JsonUtility.ToJson(tileCacheMetaData));
        }

        private void SetupTiles(GameObject tilesGO)
        {
            Transform building = tilesGO.transform.GetChild(0);
            if (building != null)
            {
                building.gameObject.name = "Building";
                for (int i = 0; i < building.childCount; i++)
                {
                    building.GetChild(i).gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.SturgBuilding);
                    building.GetChild(i).GetComponent<MeshRenderer>().material = Material;
                        //Resources.Load<Material>("Materials/BuildingHide");
                    building.GetChild(i).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            Transform terrain = tilesGO.transform.GetChild(1);
            if (terrain != null)
            {
                terrain.gameObject.name = "Terrain";
                for (int i = 0; i < terrain.childCount; i++)
                {
                    terrain.GetChild(i).gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.SturgTerrain);
                    terrain.GetChild(i).GetComponent<MeshRenderer>().material = Material;
                        //Resources.Load<Material>("Materials/BuildingHide");
                    terrain.GetChild(i).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

        }

        private async Task<byte[]> DownloadTiles(GeoLocation location, double radius, string token)
        {
            string uri = $"{_baseUrl}/get_gltf/?lat=${location.Latitude}&lng={location.Longitude}&radius={radius}&token={token}";

            UnityWebRequest unityWebRequest = UnityWebRequest.Get(uri);
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + token);
            unityWebRequest.SetRequestHeader("latitude", location.Latitude.ToString());
            unityWebRequest.SetRequestHeader("longitude", location.Longitude.ToString());

            await unityWebRequest.SendWebRequest();

            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                var filebytes = unityWebRequest.downloadHandler.data;
                return filebytes;
            }
            else
            {
                throw new HttpException(unityWebRequest.responseCode, unityWebRequest.error);
            }
        }

        private string CacheDir
        {
            get
            {
                if (string.IsNullOrEmpty(_cacheDir))
                {
                    _cacheDir = Path.Combine(Application.persistentDataPath, "Tiles", "SturG");
                    if (!Directory.Exists(_cacheDir))
                    {
                        Directory.CreateDirectory(_cacheDir);
                    }
                }
                return _cacheDir;
            }
        }
    }
}