using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK.Localization
{
    public class ScanThumbnail : MonoBehaviour
    {
        [SerializeField]
        private GameObject _thumbnail;
        [SerializeField]
        private RawImage _thumbnailRawImage;
        [SerializeField]
        private GameObject _instruction;

        private HDSitesManager _sitesManager;
        private ThumbnailProvider _thumbnailProvider;
        private void Start()
        {
            _sitesManager = FindObjectOfType<HDSitesManager>();
            _thumbnailProvider = new ThumbnailProvider();
        }

        private async void OnEnable()
        {
            await System.Threading.Tasks.Task.Yield();

            _thumbnail.gameObject.SetActive(false);
            _instruction.SetActive(false);

            if (_sitesManager.CurrentSite == null)
            {
                Debug.Log("Cannot set scan thumbnail. No site selected");
                return;
            }

            var thumbnail = await _thumbnailProvider.GetThumbnail(Guid.Parse(_sitesManager.CurrentSite.site_meta_data.ThumbId), ImageFileType.jpg);
            if (thumbnail != null)
            {
                _thumbnail.SetActive(true);
                _thumbnailRawImage.texture = thumbnail;
                _instruction.SetActive(true);
            }
        }
    }
}