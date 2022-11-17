using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class SturfeeWindowAuth
    {
        [JsonProperty]
        public string VpsToken { get; internal set; }

        public static SturfeeWindowAuth Auth
        {
            get
            {
                SturfeeWindowAuth auth;

                string json = Resources.Load<TextAsset>("Sturfee/SturfeeWindowAuth")?.text;
                if (string.IsNullOrEmpty(json))
                {
                    auth = new SturfeeWindowAuth();
                    string path = Path.Combine(Paths.SturfeeResourcesAbsolute, "SturfeeWindowAuth.txt");
                    File.WriteAllText(path, JsonConvert.SerializeObject(auth));
                }
                auth = JsonConvert.DeserializeObject<SturfeeWindowAuth>(Resources.Load<TextAsset>("Sturfee/SturfeeWindowAuth")?.text);
                return auth;                
            }
        }
    }
}
