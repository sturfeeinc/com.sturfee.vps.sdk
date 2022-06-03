using SturfeeVPS.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SturfeeVPS.SDK;

namespace SturfeeVPS.UI
{
    [Serializable]
    public class StringResource
    {
        [XmlAttribute("id")]
        public string Id;
        [XmlAttribute("value")]
        public string Value;
    }

    [Serializable]
    public class StringResources
    {
        public StringResource[] Resources;
    }

    public class SturfeeLocalizationProvider : SimpleSingleton<SturfeeLocalizationProvider>
    {
        private Dictionary<string, string> _idToString;

        [SerializeField]
        private string _locale = "en-US";
        [SerializeField]
        private string _locale_fallback = "en-US";

        private List<string> _resources = new List<string>();
        public string Locale
        {
            get { return _locale; }
            set
            {
                _locale = value;
                TryLoadResources();
            }
        }

        private void Start()
        {
            //TryLoadResources();
        }

        public void Init(ThemeAsset theme)
        {
            Debug.Log($"Initializing String Localization with locale={theme.Locale}");
            _locale = theme.Locale;
            _resources = theme.StringResources;
            TryLoadResources();
        }

        public string GetString(string key, string fallback = null)
        {
            string str = GetString(key);
            if (str == null)
            {
                if (fallback == null)
                {
                    Debug.LogWarning($"Unable to find string resource for {key}");
                    return key;
                }
                return fallback;
            }
            return str;
        }

        public string GetString(string key)
        {
            key = key.Trim();
            string str;
            if (_idToString.TryGetValue(key, out str))
            {
                return str;
            }

            return null;
        }        

        public void LoadStringResources(string path)
        {
            string prefix = "." + _locale;
            TextAsset textAsset = Resources.Load<TextAsset>(path + prefix);
            if (textAsset == null)
            {
                Debug.LogErrorFormat("String resource file {0} was not found", path + prefix);
                return;
            }

            LoadStringResources(textAsset);
        }

        private void TryLoadResources()
        {
            Debug.Log($"Loading Localized Strings...");

            _idToString = new Dictionary<string, string>();

            var loadedCount = LoadResources(_locale);

            if (loadedCount < 1)
            {
                Debug.LogWarning($"Loading FALLBACK LOCALE Localized Strings...");
                LoadResources(_locale_fallback);
            }
        }

        private int LoadResources(string locale)
        {
            var count = 0;
            string prefix = "." + locale;
            foreach (string res in _resources) // SturfeeThemeProvider.Instance.Theme.StringResources)
            {
                TextAsset textAsset = Resources.Load<TextAsset>($"{res}{prefix}");
                if (textAsset == null)
                {
                    Debug.LogFormat("String resource file {0} was not found", res + prefix);
                    continue;
                }

                LoadStringResources(textAsset);

                count++;
            }
            return count;
        }

        private void LoadStringResources(TextAsset textAsset)
        {
            try
            {
                StringResources stringResources = XmlUtility.FromXml<StringResources>(textAsset.text);
                foreach (StringResource stringResource in stringResources.Resources)
                {
                    if (_idToString.ContainsKey(stringResource.Id))
                    {
                        string exisiting = _idToString[stringResource.Id];
                        Debug.LogWarning("Duplicate resource found " + stringResource.Id + " " + exisiting + ". Duplicate: " + stringResource.Value);
                        continue;
                    }

                    _idToString.Add(stringResource.Id, stringResource.Value);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        
    }
}

