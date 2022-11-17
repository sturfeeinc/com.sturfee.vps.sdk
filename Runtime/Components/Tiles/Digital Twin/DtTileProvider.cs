using Newtonsoft.Json;
using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Sturfee.DigitalTwin.Tiles
{
    public interface ITileProvider
    {
        Task<List<DigitalTwinTileItem>> FetchTileUrl(string geohash);
        Task<List<DigitalTwinTileItem>> FetchTileUrls(List<string> geohashes);
        Task<string> DownloadTileLayer(string geohash, string url, string filename);
    }


    public class DtTileProvider : ITileProvider
    {
        public DtTileProvider()
        {
            ServicePointManager.DefaultConnectionLimit = 1000; 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<string> DownloadTileLayer(string geohash, string layer, string url)
        {
            MyLogger.Log($"DtTileProvider :: Downloading Tile {geohash} => {layer} | {url} ");
            var dtTileCache = IOC.Resolve<ICacheProvider<CachedDtTile>>();

            var tileFolder = Path.Combine(dtTileCache.CacheDir, $"{geohash}");
            if (!Directory.Exists(tileFolder)) { Directory.CreateDirectory(tileFolder); }
            var newTilePath = Path.Combine(tileFolder, $"{layer}");

            try
            {               
                var uwr = new UnityWebRequest(url);
                uwr.method = UnityWebRequest.kHttpVerbGET;
                var dh = new DownloadHandlerFile($"{newTilePath}");
                dh.removeFileOnAbort = true;
                uwr.downloadHandler = dh;
                await uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError) //uwr.isNetworkError || uwr.isHttpError)
                {
                    MyLogger.LogError(uwr.error);
                    MyLogger.LogError($"DtTileProvider :: ERROR Downloading Tile {geohash} => {layer} | {url} \n{uwr.error}");
                }
                else
                {
                    MyLogger.Log("DtTileProvider :: Download saved to: " + newTilePath.Replace("/", "\\") + "\r\n" + uwr.error);
                }

            }
            catch (Exception e)
            {               
                MyLogger.LogError($"ERROR :: LoadTileLayer.DownloadFileTaskAsync => {e.Message}\n{e.StackTrace}");
                throw e;                
            }

            return newTilePath;
        }

        public Task<List<DigitalTwinTileItem>> FetchTileUrl(string geohash)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<DigitalTwinTileItem>> FetchTileUrls(List<string> geohashes)
        {
            MyLogger.Log($"Request = POST {DtConstants.DTE_API}/digitaltwin/tiles");

            // get download URL
            var keyPrepend = "char7geohash";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{DtConstants.DTE_API}/digitaltwin/tiles?keyPrepend={keyPrepend}");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            //await AuthHelper.AddXrcsTokenAuthHeader(request);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(geohashes);
                MyLogger.Log($"   FetchTileUrls geohashes = {json}");

                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = await request.GetResponseAsync() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MyLogger.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
            }

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();

            MyLogger.Log($"Download Asset Response from API:\n{jsonResponse}");
            var items = JsonConvert.DeserializeObject<List<DigitalTwinTileItem>>(jsonResponse);

            return items;
        }
    }
}
