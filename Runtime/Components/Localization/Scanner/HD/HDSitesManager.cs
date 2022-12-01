using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public delegate void SiteSelectedEvent(HDSite hDSite);

    public class HDSitesManager : SceneSingleton<HDSitesManager>
    {        
        public event SiteSelectedEvent OnHDSiteSelected;

        [Header("Config")]
        public HDSiteFilter Filter;
        [SerializeField]
        private GameObject _siteItems;

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
        [SerializeField][ReadOnly]
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
            await LoadSites();

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
                _sites = await _hDSitesProvider.FetchHDSites(Filter);
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

        public void ShowSitesBrowser(Action<HDSite> callback)
        {
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


