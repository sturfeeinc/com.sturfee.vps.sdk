using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class HDSitesManager : SceneSingleton<HDSitesManager>
    {        
        [SerializeField]
        private HDSiteFilter _filter;
        [SerializeField]
        private HDSite _currentSite;
        [SerializeField]
        private HDSite[] _sites;
        [SerializeField]
        private GameObject _siteItems;
        
        private HDSitesProvider _hDSitesProvider;
        
        public HDSite[] Sites => _sites;
        public HDSite CurrentSite => _currentSite;

        private async void OnEnable()
        {
            _hDSitesProvider = new HDSitesProvider();
            await LoadSites();
        }

        public async Task LoadSites()
        {
            if(_filter.SortOptions == SortOptions.Location)
            {

#if !UNITY_EDITOR
                var location = new GeoLocation(Input.location.lastData);
                _filter.Location = location;                                
#endif
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
            DispaySites(false);

            ScanConfig scanConfig = new ScanConfig
            {
                HD = new HD
                {
                    SiteId = site.siteId,
                    Location = new GeoLocation { Latitude = site.latitude, Longitude = site.longitude },
                }
            };

            XRSessionManager.GetSession()?.EnableVPS(ScanType.HD, scanConfig);
        }

        public void DispaySites(bool show = true)
        {
            _siteItems.SetActive(show);
        }
    }
}


