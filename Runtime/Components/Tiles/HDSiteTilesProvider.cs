using Amazon.CognitoIdentity;
using Sturfee.External.AWS;
using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityGLTF;
using UnityGLTF.Loader;

namespace SturfeeVPS.SDK
{
    public class HDSiteTilesProvider : BaseTilesProvider
    {
        [SerializeField][ReadOnly]
        private ProviderStatus _providerStatus = ProviderStatus.Ready;
        [SerializeField][ReadOnly]
        private HDSite _site;
     

        private AsyncCoroutineHelper _asyncCoroutineHelper;
        private S3Service _s3Service;

        public event TilesLoadedAction OnTileLoaded;


        private void Start()
        {
            // var credentials = new CognitoAWSCredentials("COGNITO_ID", Amazon.RegionEndpoint.USEast1);
            var credentials = new CognitoAWSCredentials("us-east-1:e7cb8a9e-8224-4015-94ab-1c156b3c94df", Amazon.RegionEndpoint.USEast1);
            _s3Service = new S3Service(credentials, Amazon.RegionEndpoint.USEast1);
        }

        public override async Task<GameObject> GetTiles(GeoLocation location, float radius = 0, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<GameObject> GetTiles(CancellationToken cancellationToken = default)
        {
            return await LoadTiles();
        }

        public override float GetElevation(GeoLocation location)
        {
            throw new System.NotImplementedException();
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }
        
        public HDSite Site
        {
            get
            {
                if (_site == null)
                {
                    _site = HDSitesManager.CurrentInstance.CurrentSite;
                }

                if (_site == null)
                {
                    Debug.LogError("[SiteMeshProvider] :: No site set");
                    return null;
                }

                return _site;
            }
        }

        public void SetSite(HDSite site)
        {
            _site = site;
        }

        public async void Download()
        {
            await DownloadAsync();
        }

        public async void Load()
        {
            await LoadAsync();            
        }
        
        public async Task DownloadAsync()
        {
            string dir = Path.Combine(CacheDir, Site.siteId, "Mesh");
            string meshFile = Path.Combine(dir, Path.GetFileName(Site.mesh.ply));

            // mesh
            var mesh = await DownloadMesh(Site.mesh.ply);
            File.WriteAllBytes(meshFile, mesh);
            Debug.Log($" Downloaded scan mesh (glb) {Path.GetFileName(meshFile)} to {meshFile}");

            // terrain
            if (Site.scanTerrainMesh != null && !string.IsNullOrEmpty(Site.scanTerrainMesh.ply))
            {
                string terrainFile = Path.Combine(dir, Path.GetFileName(Site.scanTerrainMesh.ply));
                var terrain = await DownloadMesh(Site.scanTerrainMesh.ply);
                File.WriteAllBytes(terrainFile, terrain);
                Debug.Log($" Downloaded terrain mesh (glb) {Path.GetFileName(terrainFile)} to {terrainFile}");
            }
        }

        public async Task<GameObject> LoadTiles(CancellationToken cancellationToken = default)
        {
            if (!AvailableInCache())
            {
                await DownloadAsync();
            }

            var siteMesh = await LoadAsync();
            return siteMesh.gameObject;
        }

        public async Task<GameObject> LoadAsync()
        {
            ClearAll();
            await Task.Yield();

            string dir = Path.Combine(CacheDir, Site.siteId, "Mesh");
            string meshFile = Path.Combine(dir, Path.GetFileName(Site.mesh.ply));

            SiteMesh siteMesh = new GameObject(Site.siteName).AddComponent<SiteMesh>();
            siteMesh.SiteId = Site.siteId;
            siteMesh.SiteName = Site.siteName;

            Debug.Log($" Loading scan mesh (glb) {Path.GetFileName(meshFile)} from {meshFile}");
            var meshGO = await LoadGlb(meshFile);
            meshGO.transform.parent = siteMesh.transform;

            // terrain
            if (Site.scanTerrainMesh != null && !string.IsNullOrEmpty(Site.scanTerrainMesh.ply))
            {
                string terrainFile = Path.Combine(dir, Path.GetFileName(Site.scanTerrainMesh.ply));

                Debug.Log($" Loading terrain mesh (glb) {Path.GetFileName(terrainFile)} from {terrainFile}");

                var terrainGO = await LoadGlb(terrainFile);
                foreach (var mr in terrainGO.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.gameObject.layer = LayerMask.NameToLayer(SturfeeLayers.HDSiteTerrain);
                }
                terrainGO.transform.parent = siteMesh.transform;
            }

            siteMesh.transform.parent = transform;
            return siteMesh.gameObject;
        }

        public bool AvailableInCache()
        {
            var dir = Path.Combine(Application.persistentDataPath, "Sites", Site.siteId, "Mesh");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // mesh
            var meshFile = Path.Combine(dir, Path.GetFileName(Site.mesh.ply));

            return File.Exists(meshFile);
        }

        private async Task<GameObject> LoadGlb(string filePath)
        {
            var importOptions = new ImportOptions
            {
                DataLoader = new FileLoader(Path.GetDirectoryName(filePath)),
                AsyncCoroutineHelper = AsyncCoroutineHelper,
            };

            Debug.Log($"HDSiteTilesProvider :: Khronos => Loading file = {filePath}");

            try
            {
                _providerStatus = ProviderStatus.Initializing;
                var filename = Path.GetFileNameWithoutExtension(filePath);
                var importer = new GLTFSceneImporter(filePath, importOptions);

                importer.Collider = GLTFSceneImporter.ColliderType.Mesh;

                GameObject meshGameObject = null;

                await importer.LoadSceneAsync(-1, true, (go, err) =>
                {
                    if (go == null)
                    {
                        Debug.LogError($"HDSiteTilesProvider :: No Mesh Found for Import ({filePath}");
                        return;
                    }

                    meshGameObject = go;
                    meshGameObject.transform.rotation = Quaternion.Euler(-90, 180, 0);

                    Debug.Log($"HDSiteTilesProvider :: Finished importing mesh gltf : {go.name}");
                });

                while (meshGameObject == null)
                {
                    await Task.Yield();
                }

                _providerStatus = ProviderStatus.Ready;
                return meshGameObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SiteMeshProvider :: ERROR LOADING GLTF");
                Debug.LogException(ex);
                //throw;
            }

            return null;
        }

        private async Task<byte[]> DownloadMesh(string url)
        {
            try
            {
                _providerStatus = ProviderStatus.Initializing;
                Debug.Log($"HDSiteTilesProvider :: Downloading mesh => {url}");
                var meshFile = await _s3Service.GetObjectAsync(url);
                Debug.Log($"HDSiteTilesProvider :: Downloading complete");
                _providerStatus = ProviderStatus.Ready;
                return meshFile;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private void ClearAll()
        {
            foreach (var siteMesh in GetComponentsInChildren<SiteMesh>())
            {
                DestroyImmediate(siteMesh.gameObject);
            }
        }

        private string CacheDir
        {
            get
            {
                string dir = Path.Combine(Application.persistentDataPath, "Sites");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        private AsyncCoroutineHelper AsyncCoroutineHelper
        {
            get
            {
                if (_asyncCoroutineHelper == null)
                {
                    _asyncCoroutineHelper = new GameObject("_AsyncCoroutineHelper").AddComponent<AsyncCoroutineHelper>();
                }
                return _asyncCoroutineHelper;
            }
        }
    }
}
