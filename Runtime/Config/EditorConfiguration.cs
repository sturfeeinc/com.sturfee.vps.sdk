using SturfeeVPS.Core;
using System;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class EditorConfiguration
    {
        public string AccessToken;
        public TileSize TileSize = TileSize.Small;
        public Theme Theme;
        public Cache Cache;

        public static EditorConfiguration Config
        {
            get
            {
                TextAsset configTextAsset = Resources.Load<TextAsset>(Paths.SturfeeResourcesRelative);
                if (configTextAsset != null)
                {
                    return JsonUtility.FromJson<EditorConfiguration>(configTextAsset.text);
                }
                SturfeeDebug.LogError(" Cannot load editor config");
                return null;
            }
        }
    }

    [Serializable]
    public class Cache
    {
        public float Distance;
        public int ExpirationTime;        
    }

    [Serializable]
    public class Theme
    {
        public string Path;
        public string Locale;
    }

    public enum TileSize
    {
        Small = 300,
        Large = 600
    }
}
