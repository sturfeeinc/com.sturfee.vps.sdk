#if UNITY_EDITOR
using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SturfeeVPS.SDK
{
    public static class EditorUtils
    {
        public static readonly GeoLocation DefaultLocation = new GeoLocation { Latitude = 37.332093d, Longitude = -121.890137d };

        public static GeoLocation EditorFallbackLocation
        {
            get
            {
                var json = EditorPrefs.GetString(PlayerPrefsKeys.EditorFallbackLocation, JsonUtility.ToJson(DefaultLocation));
                return JsonUtility.FromJson<GeoLocation>(json);
            }
            set
            {
                if (value != null)
                {
                    EditorPrefs.SetString(PlayerPrefsKeys.EditorFallbackLocation, JsonUtility.ToJson(value));
                }
            }

        }
    }
}
#endif