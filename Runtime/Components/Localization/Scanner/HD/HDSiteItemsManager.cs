using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SturfeeVPS.SDK.Examples
{
    public class HDSiteItemsManager : MonoBehaviour
    {
        [Header("Site Items")]
        [SerializeField]
        private HDSiteItem _siteItemPrefab;
        [SerializeField]
        private Transform _content;

        private async void OnEnable()
        {
            if (!HDSitesManager.CurrentInstance.Sites.Any())
            {
                await HDSitesManager.CurrentInstance.LoadSites();
            }

            LoadSites();
        }

        public void LoadSites()
        {
            Debug.Log($"Loading site items");

            ClearAllSites();

            var sites = HDSitesManager.CurrentInstance.Sites;
            if (sites != null)
            {
                if (sites.Any())
                {
                    foreach (HDSite site in sites)
                    {
                        var siteItem = Instantiate(_siteItemPrefab, _content);
                        siteItem.SetSite(site);
                    }
                }
            }
        }

        public async void Reload()
        {
            Debug.Log($" Reload");

            await HDSitesManager.CurrentInstance.LoadSites();
            LoadSites();
        }
        public void ClearAllSites()
        {
            foreach (HDSiteItem siteItem in _content.GetComponentsInChildren<HDSiteItem>())
            {
                DestroyImmediate(siteItem.gameObject);
            }
        }
    }

}