using Newtonsoft.Json;
using System.IO;
using SturfeeVPS.Core;
using SturfeeVPS.Core.Models;
using SturfeeVPS.Core.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public delegate void SiteSelectedEvent(HDSite hDSite);

    /// <summary>
    /// Manager for HD sites.
    /// </summary>
    public class HDSitesManager : SceneSingleton<HDSitesManager>
    {        
        /// <summary>
        /// Event fired when HD site card is selected.
        /// </summary>
        public event SiteSelectedEvent OnHDSiteSelected;

        [Header("Config")]
        public HDSiteFilter Filter;
        [SerializeField]
        private GameObject _siteItems;

        public bool UseAppId = false;
        public bool UseDtHdId = false;
        public string DtHdId;
        public string UserId;

        [Header("SiteTile")]
        [SerializeField]
        private bool _showTileUI;
        [SerializeField]
        private Toggle _displayToggle;        
        [SerializeField]
        private Toggle _occlusionToggle;
        [SerializeField]
        private Material _occlusionMaterial;

        [Header("Internal")]
        [SerializeField]//[ReadOnly]
        private HDSite _currentSite;
        [SerializeField]//[ReadOnly]
        private HDSite[] _sites;
        [SerializeField][ReadOnly]
        private GameObject _currentTile;
        [SerializeField]//[ReadOnly]
        private Material _tileMaterial;


        private HDSitesProvider _hDSitesProvider;
        
        public HDSite[] Sites => _sites;
        public HDSite CurrentSite => _currentSite;
        public GameObject CurrentTile => _currentTile;

        private Action<HDSite> _onSiteSelectedCallback = null;

        private async void OnEnable()
        {
            _hDSitesProvider = new HDSitesProvider();
            await Task.Yield();

            DispaySites(false);
        }

        public async Task LoadSites(GeoLocation location = null)
        {
            if(Filter.SortOptions == SortOptions.Location)
            {
                if (location == null || location.Latitude == 0 || location.Longitude == 0)
                {
                    if (XrSessionManager.GetSession() != null)
                    {
                        Filter.Location = XrCamera.Pose.GeoLocation;
                    }
                    else
                    {
                        Filter.Location = new GeoLocation(Input.location.lastData);
                    }
                }
                else
                {                    
                    Filter.Location = location;
                }
                
                SturfeeDebug.Log($"HDSitesManager :: HDSItes location filter set to {Filter.Location.ToFormattedString()}");                                
            }
            try
            {
                if (UseAppId)
                {
                    _sites = await _hDSitesProvider.FetchHDSites(Filter);
                }
                else if (UseDtHdId)
                {
                    DtHdLayout layoutData = new DtHdLayout();
                    try
                    {
                        var uwr = new UnityWebRequest(Path.Combine(DtConstants.DTHD_LAYOUT, DtHdId, "?full_details=true"));

                        var dh = new DownloadHandlerBuffer();
                        uwr.downloadHandler = dh;

                        uwr.method = UnityWebRequest.kHttpVerbGET;
                        await uwr.SendWebRequest();

                        if (uwr.result == UnityWebRequest.Result.ConnectionError) //uwr.isNetworkError || uwr.isHttpError)
                        {
                            Debug.Log("error downloading");
                        }
                        else
                        {
                            Debug.Log($"Data: {uwr.downloadHandler.text}");
                            layoutData = JsonConvert.DeserializeObject<DtHdLayout>(uwr.downloadHandler.text, new JsonSerializerSettings {
                                NullValueHandling = NullValueHandling.Ignore
                            });   
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    
                    List<HDSite> siteList = new List<HDSite>();

                    foreach (ScanMesh i in layoutData.ScanMeshes)
                    {
                        var hdsite = i.VpsHdSite;
                        if (hdsite == null) continue;

                        var site = new HDSite();

                        site.siteName = hdsite.siteInfo.name;
                        site.siteId = hdsite.siteInfo.site_id;
                        site.latitude = hdsite.siteInfo.latitude;
                        site.longitude = hdsite.siteInfo.longitude;
                        site.ImageUrl = hdsite.thumbnailUrl;
                        site.mesh = new SitePointCloud();
                        site.mesh.ply = i.VpsHdSite.anchorMesh;
                        site.mesh.centerRef = new CenterRef();
                        site.mesh.centerRef.x = i.RefX;
                        site.mesh.centerRef.y = i.RefY;
                        site.mesh.heightOffset = 0;


                        site.site_meta_data = new SiteMetadata();
                        site.site_meta_data.UserId = layoutData.UserId;
                        // site.site_meta_data.RefId
                        site.site_meta_data.ThumbId = hdsite.siteInfo.thumbnail_id;
                        site.site_meta_data.CreatedDate = hdsite.siteInfo.createdDate.ToString();
                        if (hdsite.siteInfo.isIndoor)
                            site.site_meta_data.SpaceType = "Indoor";
                        else
                            site.site_meta_data.SpaceType = "Outdoor";

                        siteList.Add(site);
                    }

                    _sites = new HDSite[siteList.Count];
                    for (int i=0; i<siteList.Count; i++)
                    {
                        _sites[i] = siteList[i];
                    }
                    

                }
                else
                {
                    _sites = await _hDSitesProvider.FetchHDSites(UserId);
                }

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void SetCurrentSite(HDSite site)
        {
            _currentSite = site;
            _onSiteSelectedCallback?.Invoke(site);

            OnHDSiteSelected?.Invoke(site);

            DispaySites(false);
        }

        public void Close()
        {
            _currentSite = null;
            _onSiteSelectedCallback?.Invoke(null);

            OnHDSiteSelected?.Invoke(null);
        }

        public void DispaySites(bool show = true)
        {
            _siteItems.SetActive(show);
        }

        public async void ShowSitesBrowser(Action<HDSite> callback)
        {
            // Debug.Log($"[Card Management] Showing Sites Browser for {_sites}");
            // while (_sites.Length < 1)
            // {
            //     await Task.Yield();
            // }
            // Debug.Log($"[Card Management] Showing Sites Browser for {_sites}");
            _siteItems.SetActive(true);
            _onSiteSelectedCallback = callback;
        }

        public async void SetSiteTile(GameObject tile)
        {
            if(tile == null)
            {
                _currentTile = null;
                _occlusionToggle.gameObject.SetActive(false);
                _displayToggle.gameObject.SetActive(false);

                return;
            }

            ClearAllSiteTiles();
            await Task.Yield();

            _currentTile = tile;
            _currentTile.transform.parent = transform;
            _tileMaterial = tile.GetComponentInChildren<MeshRenderer>().material;

            _displayToggle.gameObject.SetActive(_showTileUI);
            _occlusionToggle.gameObject.SetActive(_showTileUI);

            // when toggle is ON we display the mesh as it is otherwise we occlude the mesh
            ToggleTileOcclusion(_occlusionToggle.isOn);
        }

        public void ToggleTileOcclusion(bool noOcclusion)
        {
            if (_currentTile != null)
            {
                foreach (var mr in _currentTile.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.material = noOcclusion ? _tileMaterial : _occlusionMaterial;
                }
            }
        }

        public void ToggleTileDisplay(bool isOn)
        {
            _currentTile?.SetActive(isOn);
        }

        private void ClearAllSiteTiles()
        {
            foreach (var siteMesh in GetComponentsInChildren<SiteMesh>())
            {
                DestroyImmediate(siteMesh.gameObject);
            }
        }
    }

    
}


