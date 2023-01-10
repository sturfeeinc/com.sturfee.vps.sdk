﻿using Newtonsoft.Json;
using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.SDK
{
    public interface IHDSitesProvider
    {
        Task<HDSite[]> FetchHDSites(HDSiteFilter siteFilter);
    }

    public class HDSitesProvider : IHDSitesProvider
    {
        private string _baseUrl = "https://sharedspaces-api.sturfee.com/hd-sites/group";

        public async Task<HDSite[]> FetchHDSites(HDSiteFilter siteFilter)
        {
            if(string.IsNullOrEmpty(siteFilter.AppId))
            {
                throw new Exception("AppId is NULL. Enter an appId/userId in HDSitesManager");
            }
            
            string json = await ServicesDownloadSitesMeta(siteFilter.AppId);

            HDSite[] sites = JsonConvert.DeserializeObject<SiteResponse>(json,new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore }).Items;

            if(sites == null || sites.Length < 1)
            {
                Debug.LogWarning($"sites empty ! (Id = {siteFilter.AppId})");                
            }

            switch (siteFilter.SortOptions)
            {
                case SortOptions.Location: SortByLocation(sites, siteFilter.Location); break;
                case SortOptions.CreatedDate: SortByDate(sites, false); break;
                case SortOptions.MostRecent: SortByDate(sites); break;
                case SortOptions.Name: SortByName(sites); break;
            }

            return sites;
        }

        private void SortByLocation(HDSite[] sites, GeoLocation location)
        {
            Array.Sort(sites, (site1, site2) =>
            {
                var site1Location = new GeoLocation { Latitude = site1.latitude, Longitude = site1.longitude };
                var site2Location = new GeoLocation { Latitude = site2.latitude, Longitude = site2.longitude };

                var distance1 = GeoLocation.Distance(site1Location, location);
                var distance2 = GeoLocation.Distance(site2Location, location);

                return distance1.CompareTo(distance2);
            });
        }

        private void SortByDate(HDSite[] sites, bool latest = true)
        {
            Array.Sort(sites, (site1, site2) =>
            {
                return latest ? 
                    site2.site_meta_data.CreatedDateTime.CompareTo(site1.site_meta_data.CreatedDateTime) :
                    site1.site_meta_data.CreatedDateTime.CompareTo(site2.site_meta_data.CreatedDateTime);
            });
        }

        private void SortByName(HDSite[] sites)
        {
            Array.Sort(sites, (site1, site2) =>
            {
                return site1.siteName.CompareTo(site2.siteName);
            });
        }

        private async Task<string> DownloadSitesMetaData(string userId)
        {
            SturfeeDebug.Log($"Downloading sites meta data for userId= {userId}");

            UnityWebRequest request = UnityWebRequest.Get($"{_baseUrl}/{userId}.txt");
            await request.SendWebRequest();

            SturfeeDebug.Log($"Request => {request.uri}");


            if (string.IsNullOrEmpty(request.error))
            {
                return request.downloadHandler.text;
            }

            SturfeeDebug.LogError($" {request.responseCode} Error : {request.error}");

            if (request.responseCode == 404 || request.responseCode == 403)
            {
                throw new Exception("No sites avaialble for this user");
            }

            throw new Exception("Internal Error! Please try again later.");
        }

        private async Task<string> ServicesDownloadSitesMeta(string userId)
        {
            SturfeeDebug.Log($"Downloading sites meta data for userId= {userId}");

            UnityWebRequest request = UnityWebRequest.Get($"{_baseUrl}/{userId}");
            await request.SendWebRequest();

            SturfeeDebug.Log($"Request => {request.uri}");


            if (string.IsNullOrEmpty(request.error))
            {
                return request.downloadHandler.text;
            }

            SturfeeDebug.LogError($" {request.responseCode} Error : {request.error}");

            if (request.responseCode == 404 || request.responseCode == 403)
            {
                throw new Exception("No sites avaialble for this user");
            }

            throw new Exception("Internal Error! Please try again later.");
        }
    }
}
