using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK.Examples
{
    public class HDSiteItem : MonoBehaviour
    {
        [SerializeField]
        private RawImage _thumbnail;
        [SerializeField]
        private TextMeshProUGUI _title;
        [SerializeField]
        private GameObject _highlight;
        [SerializeField]
        private GameObject _download;
        [SerializeField]
        private GameObject _loader;

        private HDSiteTilesProvider _hdTilesProvider;
        private ThumbnailProvider _thumbnailProvider;

        private HDSite _site;
        private void Awake()
        {
            _thumbnailProvider = new ThumbnailProvider();
            _hdTilesProvider = GetComponent<HDSiteTilesProvider>();
        }

        private void Update()
        {
            _highlight.SetActive(HDSitesManager.CurrentInstance.CurrentSite == _site);
        }
        public async void SetSite(HDSite site)
        {
            _site = site;
            _title.text = site.siteName;
            _hdTilesProvider.SetSite(_site);
            var thumbnail = await DownloadThumbnail(Guid.Parse(site.site_meta_data.ThumbId));
            if (thumbnail != null)
            {
                _thumbnail.texture = thumbnail;
            }
            else
            {
                Debug.LogError($"Could not get thumbnail for site {_site.siteName}");
            }
            
            if (_hdTilesProvider.AvailableInCache())
            {
                _download.SetActive(false);
            }
        }

        public async void OnSiteSelected()
        {
            HDSitesManager.CurrentInstance.SetCurrentSite(_site);

            var tile = await _hdTilesProvider.LoadTiles();
            if (tile != null)
            {
                Vector3 shift = new Vector3(
                    (float)(_site.mesh.centerRef.x - PositioningUtils.GetReferenceUTM.X),
                    _site.mesh.heightOffset,
                    (float)(_site.mesh.centerRef.y - PositioningUtils.GetReferenceUTM.Y)
                );
                Debug.Log($" Shift : {shift}");
                tile.transform.position = shift;
            }

            HDSitesManager.CurrentInstance.SetSiteTile(tile);
        }

        public async void OnDownloadButton()
        {
            if(_site == null)
            {
                Debug.LogError($"[HDSiteItem] :: Cannot download site mesh. Site is NULL ");
                return;
            }

            _loader.SetActive(true);
            try
            {
                await _hdTilesProvider.DownloadAsync();
                _download.SetActive(false);
                _loader.SetActive(false);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                _loader.SetActive(false);   
            }
        }

        

        private async Task<Texture> DownloadThumbnail(Guid id)
        {
            try
            {
                var texture = await _thumbnailProvider.GetThumbnail(id, ImageFileType.jpg);
                return texture;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

            }

            return null;
        }

    } 
}
