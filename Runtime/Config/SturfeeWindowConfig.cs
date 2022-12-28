using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class SturfeeWindowConfig
    {
        [JsonProperty]
        public string Theme { get; internal set; }
        
        [JsonProperty]
        public string Locale { get; internal set; }

        public static SturfeeWindowConfig Config
        {
            get
            {
                SturfeeWindowConfig config;

                string json = Resources.Load<TextAsset>("Sturfee/SturfeeWindowConfig")?.text;
                if (string.IsNullOrEmpty(json))
                {
                    config = new SturfeeWindowConfig();
                    string path = Path.Combine(Paths.SturfeeResourcesAbsolute, "SturfeeWindowConfig.txt");
                    File.WriteAllText(path, JsonConvert.SerializeObject(config));
                }
                config = JsonConvert.DeserializeObject<SturfeeWindowConfig>(Resources.Load<TextAsset>("Sturfee/SturfeeWindowConfig")?.text);
                return config;
            }
        }
    }
}
