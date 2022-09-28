using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public delegate void SiteSelectedEvent(HDSite hDSite);

    public class HDSitesManager : SceneSingleton<HDSitesManager>
    {        
        public event SiteSelectedEvent OnHDSiteSelected;

        [Header("Config")]
        [SerializeField]
        private HDSiteFilter _filter;
        [SerializeField]
        private GameObject _siteItems;

        [Header("Internal")]
        [SerializeField]
        private HDSite _currentSite;
        [SerializeField]
        private HDSite[] _sites;
        
        
        private HDSitesProvider _hDSitesProvider;
        
        public HDSite[] Sites => _sites;
        public HDSite CurrentSite => _currentSite;

        private Action<HDSite> _onSiteSelectedCallback = null;

        private async void OnEnable()
        {
#if UNITY_EDITOR
            var defaultLocation = new GeoLocation { Latitude = 37.332093d, Longitude = -121.890137d };
            PlayerPrefs.SetString(PlayerPrefsKeys.EditorFallbackLocation, JsonUtility.ToJson(defaultLocation));

#endif

            _hDSitesProvider = new HDSitesProvider();
            await LoadSites();
        }

        public async Task LoadSites()
        {
            if(_filter.SortOptions == SortOptions.Location)
            {
                var location = await GetLocationFromGpsProvider();
                if(location == null)
                {
                    Debug.Log($"HDSitesManager :: Location not obtained from GpsProvider");
#if !UNITY_EDITOR
                    location = new GeoLocation(Input.location.lastData);
#else
                    location = EditorUtils.EditorFallbackLocation;
#endif                    
                }

                _filter.Location = location;
                Debug.Log($"HDSitesManager :: HDSItes location filter set to {location.ToFormattedString()}");
            }

            try
            {
                _sites = await _hDSitesProvider.FetchHDSites(_filter);
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

        public void ClearSelectedSite()
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

        private async Task<GeoLocation> GetLocationFromGpsProvider()
        {
            await Task.Delay(1000);

            if (XRSessionManager.GetSession() != null)
            {
                var seconds = 0;
                var location =new GeoLocation();
                while (seconds < 2 && XRSessionManager.GetSession().GpsProvider.GetProviderStatus() != ProviderStatus.Ready)
                {                    
                    Debug.Log($"HDSitesManager :: Waiting 1 second for GPS...{seconds}");
                    seconds++;

                    await Task.Delay(1000);                    
                }

                if (XRSessionManager.GetSession()?.GpsProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    location = XRSessionManager.GetSession().GpsProvider.GetCurrentLocation();
                    return location;
                }

                Debug.Log($" Done waiting for GPS");
                return null;
            }
            return null;
        }
    }

    
}


