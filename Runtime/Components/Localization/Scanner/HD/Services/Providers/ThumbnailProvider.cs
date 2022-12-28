using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SturfeeVPS.SDK
{
    public enum ImageFileType
    {
        png, // the default
        jpg
    }

    public interface IThumbnailProvider
    {
        Task<Texture> GetThumbnail(Guid id, ImageFileType ext = ImageFileType.png);
    }
    public class ThumbnailProvider : IThumbnailProvider
    {
        private string _storageUrl = $"https://{SturfeeConstants.S3_PUBLIC_BUCKET}.s3.{SturfeeConstants.S3_REGION}.amazonaws.com/thumbnails";

        public async Task<Texture> GetThumbnail(Guid id, ImageFileType ext = ImageFileType.png)
        {
            var baseDirectory = Path.Combine(Application.persistentDataPath, "Thumbnails");
            if (!Directory.Exists(baseDirectory)) { Directory.CreateDirectory(baseDirectory); }

            // try to get thumbnail locally
            var thumbFile = $"{baseDirectory}/{id}.{ext}";

            if (File.Exists(thumbFile))
            {
                var fileData = File.ReadAllBytes(thumbFile);
                var image = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                image.LoadImage(fileData); // ..this will auto-resize the texture dimensions.
                return image;
            }
            else
            {
                // get from the server and store locally
                //var baseUrl = $"https://xrcs-public.s3.us-east-2.amazonaws.com/thumbnails";
                var url = $"{_storageUrl}/{id}.{ext}";

                //MyLogger.Log($"Loading thumbnail for {id}.{ext}");

                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
                await uwr.SendWebRequest();

                //MyLogger.Log($"   thumbnail fetched...");

                if (uwr.result == UnityWebRequest.Result.ConnectionError) //uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                    return null;
                }
                else if (uwr.result == UnityWebRequest.Result.Success)
                {
                    var image = ((DownloadHandlerTexture)uwr.downloadHandler).texture;

                    // save to file system
                    byte[] bytes = ext == ImageFileType.png ? image.EncodeToPNG() : image.EncodeToJPG();
                    File.WriteAllBytes($"{baseDirectory}/{id}.{ext}", bytes);

                    return image;
                }
            }

            return null;
        }
    }
}
