using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using SturfeeVPS.Core;

namespace SturfeeVPS.SDK
{
    //[InitializeOnLoad]
    public class SturfeeConfigurationWindow : EditorWindow
    { 
        public delegate void SubscriptionSuccessful();
        public static SubscriptionSuccessful OnSubscriptionSuccessful;

        public GUISkin SturfeeSkin;
        public MonoScript source;
        public static int OpenToSection = 0;

        private string _apiKey = "N/A";        
        private int _accessLevel = 0;
        
        static string _configurationFile;
        private SturfeeConfiguration _config = null;
        private Vector2 _scrollPosition;
        private Texture2D _logo;
        private static int _currentTab = 0;

        private Color _sturfeePrimaryColor = new Color(25f / 255.0f, 190f / 255.0f, 200f / 255.0f);
        private Color _sturfeeSecondaryColor = new Color(238f / 255.0f, 66f / 255.0f, 102f / 255.0f);
        private Color _sturfeeErrorColor = new Color(183f / 255.0f, 48f / 255.0f, 48f / 255.0f);
        private Color _sturfeeDarkBackgroundColor = new Color(35f / 255.0f, 35f / 255.0f, 35f / 255.0f);

        private bool _loadingSubscription = false;
        private bool _accessTokenValid = false;
        private bool _setupFinished = false;
        private static UnityWebRequest _www;
        
        // Config
        private Languages _language = Languages.English;
        private TileSize _tileSize = TileSize.Small;


        [MenuItem("Sturfee/Configure", false, 0)]
        public static void ShowWindow()
        {
            _configurationFile = Path.Combine(Paths.SturfeeResourcesAbsolute, Paths.ConfigFile);

            SturfeeConfigurationWindow window = EditorWindow.GetWindow<SturfeeConfigurationWindow>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.sturfee.vps.sdk/Editor/Images/sturfee_official_icon-black.png");
            GUIContent customTitleContent = new GUIContent("Sturfee", icon);
            window.titleContent = customTitleContent;
            window.Show();

            _currentTab = OpenToSection;
            //GUI.UnfocusWindow();
        }

        void OnGUI()
        {
            if (_config == null)
            {
                LoadConfig();
            }

            //GUI.skin = BuildSturfeeSkin();
            GUI.skin = SturfeeSkin;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                var boxStyle = new GUIStyle(GUI.skin.label);
                boxStyle.normal.background = EditorGUIUtility.whiteTexture;

                EditorGUILayout.BeginVertical(boxStyle);// GUI.skin.box);
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();//.FlexibleSpace();
                    _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        "Packages/com.sturfee.vps.sdk/Editor/Images/sturfee_official_logo-black_small.png");
                    GUILayout.Label(_logo, GUILayout.MaxHeight(64), GUILayout.Width(200));
                    EditorGUILayout.Space();//.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    var buttonStyle = new GUIStyle(GUI.skin.button);//.label);
                    buttonStyle.normal.textColor = Color.white;// _sturfeePrimaryColor;
                    buttonStyle.normal.background = MakeTex(2, 2, _sturfeePrimaryColor); //EditorGUIUtility.whiteTexture;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Sturfee.com", buttonStyle, GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        Application.OpenURL("https://sturfee.com");
                    }
                    if (GUILayout.Button("Developer Portal", buttonStyle, GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        Application.OpenURL("https://developer.sturfee.com");
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();

                    if (_accessTokenValid)
                    {
                        AddSpace(4);
                    }
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    var dllFile = new FileInfo(@"Packages/com.sturfee.vps.sdk/Plugins/VPS Core/SturfeeVPS.Core.dll");
                    string assemblyVersion = Assembly.LoadFile(dllFile.FullName).GetName().Version.ToString();
                    GUILayout.Label("v" + assemblyVersion);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                if (!_accessTokenValid && !_loadingSubscription)
                {
                    var errorStyle = new GUIStyle(GUI.skin.label);
                    errorStyle.normal.background = MakeTex(2, 2, new Color(160 / 255.0f, 40 / 255.0f, 40 / 255.0f));
                    errorStyle.normal.textColor = Color.white;
                    errorStyle.alignment = TextAnchor.MiddleCenter;
                    EditorGUILayout.BeginHorizontal(errorStyle);
                    EditorGUILayout.PrefixLabel("Invalid Subscription", errorStyle);
                    EditorGUILayout.EndHorizontal();

                    _currentTab = GUILayout.Toolbar(_currentTab, new string[] { "Subscription" });
                    _currentTab = 0;
                }
                else if (!_accessTokenValid && _loadingSubscription)
                {
                    var loadingStyle = new GUIStyle(GUI.skin.label);
                    loadingStyle.normal.background = MakeTex(2, 2, _sturfeeSecondaryColor);
                    loadingStyle.normal.textColor = Color.white;
                    loadingStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.BeginHorizontal(loadingStyle);
                    GUI.contentColor = Color.white;
                    EditorGUILayout.PrefixLabel("Loading Subscription...", loadingStyle);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    //var successStyle = new GUIStyle(GUI.skin.label);
                    //successStyle.normal.background = MakeTex(2, 2, _sturfeePrimaryColor);
                    //successStyle.normal.textColor = Color.white;
                    //successStyle.alignment = TextAnchor.MiddleCenter;
                    //GUILayout.BeginHorizontal(successStyle);
                    //GUI.contentColor = Color.white;
                    //EditorGUILayout.PrefixLabel("Active Subscription", successStyle);
                    //GUILayout.EndHorizontal();                
                    _currentTab = GUILayout.Toolbar(_currentTab, new string[] { "Subscription", "Config", "Objects" });
                }

                switch (_currentTab)
                {
                    case 0:
                        GUILayout.Label("Subscription Settings", EditorStyles.boldLabel);

                        EditorGUILayout.BeginVertical();
                        {
                            _apiKey = EditorGUILayout.TextField("API Key", _apiKey);
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Request Access", GUILayout.Width(100), GUILayout.Height(30)))
                                {
                                    SaveConfiguration();

                                    // get access from the API
                                    _loadingSubscription = true;
                                    CheckSubscription(_apiKey, HandleSubscriptionResult, HandleSubscriptionError);
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Label("Available Features", EditorStyles.boldLabel);
                            EditorGUILayout.LabelField("Access Level: Tier " + _accessLevel);

                            Texture2D tierImage;
                            var tierImageH = 375;
                            var tierImageW = 450;
                            switch (_accessLevel)
                            {
                                case 1:
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        tierImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                        "Packages/com.sturfee.vps.sdk/Editor/Images/Tier-1.png");
                                        GUILayout.Label(tierImage, GUILayout.MaxHeight(tierImageH), GUILayout.Width(tierImageW));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical();
                                        {
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.LabelField(" - Localization");
                                            EditorGUILayout.LabelField(" - World Anchors");
                                            EditorGUILayout.LabelField(" - Basic Light System");
                                            EditorGUILayout.LabelField(" - Basic Surface Detection");
                                            GUILayout.FlexibleSpace();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    break;
                                case 2:
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        tierImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                        "Packages/com.sturfee.vps.sdk/Editor/Images/Tier-2.png");
                                        GUILayout.Label(tierImage, GUILayout.MaxHeight(tierImageH), GUILayout.Width(tierImageW));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical();
                                        {
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.LabelField(" - Localization");
                                            EditorGUILayout.LabelField(" - World Anchors");
                                            EditorGUILayout.LabelField(" - Basic Light System");
                                            EditorGUILayout.LabelField(" - Basic Surface Detection");
                                            EditorGUILayout.LabelField(" - Full Terrain Detection");
                                            GUILayout.FlexibleSpace();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    break;
                                case 3:
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        tierImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                                                            "Packages/com.sturfee.vps.sdk/Editor/Images/Tier-3.png");
                                        GUILayout.Label(tierImage, GUILayout.MaxHeight(tierImageH), GUILayout.Width(tierImageW));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical();
                                        {
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.LabelField(" - Localization");
                                            EditorGUILayout.LabelField(" - World Anchors");
                                            EditorGUILayout.LabelField(" - Basic Light System");
                                            EditorGUILayout.LabelField(" - Basic Surface Detection");
                                            EditorGUILayout.LabelField(" - Full Terrain Detection");
                                            EditorGUILayout.LabelField(" - Full Building Detection");
                                            GUILayout.FlexibleSpace();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    break;
                                case 4:
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        tierImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                        "Packages/com.sturfee.vps.sdk/Editor/Images/Tier-4.png");
                                        GUILayout.Label(tierImage, GUILayout.MaxHeight(tierImageH), GUILayout.Width(tierImageW));
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical();
                                        {
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.LabelField(" - Localization");
                                            EditorGUILayout.LabelField(" - World Anchors");
                                            EditorGUILayout.LabelField(" - Basic Light System");
                                            EditorGUILayout.LabelField(" - Basic Surface Detection");
                                            EditorGUILayout.LabelField(" - Full Terrain Detection");
                                            EditorGUILayout.LabelField(" - Full Building Detection");
                                            EditorGUILayout.LabelField(" - Dynamic Objects");
                                            GUILayout.FlexibleSpace();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    break;
                                default:
                                    break;
                            }
                        }
                        EditorGUILayout.EndVertical();
                        break;

                    case 1:
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                            //  Tiles
                            GUILayout.Label("Tiles", EditorStyles.boldLabel);
                            EditorGUILayout.BeginHorizontal();

                            // Tile Size
                            var tileSize = (TileSize) EditorGUILayout.EnumPopup("Select Tile Load Size", _tileSize);
                            if(tileSize != _tileSize)
                            {
                                SturfeeMenu.ClearTileCache();
                            }
                            _tileSize = tileSize;

                            AddSpace(5);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            // Caching
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Caching", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Clear Tile Cache", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                SturfeeMenu.ClearTileCache();
                            }
                            EditorGUILayout.EndHorizontal();

                            // Language
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            _language = (Languages)EditorGUILayout.EnumPopup("Select Language", _language);
                            //AddSpace(1);
                            EditorGUILayout.LabelField("\t Language to be used to show vps related messages on screen");                            
                            EditorGUILayout.EndHorizontal();


                            SaveConfiguration();

                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        break;

                    case 2:
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        if (!_setupFinished && !SturfeeLayersExist())
                        {
                            GUILayout.Label("Unity Setup", EditorStyles.boldLabel);
                            if (GUILayout.Button("First Time Setup", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                CreateSturfeeLayers();
                                _setupFinished = true;
                            }
                        }
                        else
                        {
                            GUILayout.Label("XR Objects", EditorStyles.boldLabel);
                            if (GUILayout.Button("Create XRSession", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/SturfeeVPS/SDK/XRSession/SturfeeXRSession.prefab", typeof(GameObject));
                                GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                                clone.name = prefab.name;
                            }
                            if (GUILayout.Button("Create XRCamera", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/SturfeeVPS/SDK/XRSession/XRCamera.prefab", typeof(GameObject));
                                GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                                clone.name = prefab.name;
                            }
                            if (GUILayout.Button("Create XRLight", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/SturfeeVPS/SDK/XRSession/XRLight.prefab", typeof(GameObject));
                                GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                                clone.name = prefab.name;
                            }

                            if (GUILayout.Button("Create WorldAnchor", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                var anchor = new GameObject().AddComponent<WorldAnchor>();
                                anchor.gameObject.name = "WorldAnchor";
                            }
                        }
                        break;
                }

                //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
                //myBool = EditorGUILayout.Toggle("Toggle", myBool);
                //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
                //EditorGUILayout.EndToggleGroup();

            }
            EditorGUILayout.EndScrollView();
        }

        private void HandleSubscriptionResult(SturfeeSubscriptionInfo info)
        {
            _loadingSubscription = false;

            _accessLevel = info.Tier;

            if (info.Tier > 0)
            {
                _accessTokenValid = true;
            }   
            else
            {
                _accessTokenValid = false;
            }

            EditorApplication.update -= SturfeeSubscriptionManager.OnResponse;

            OnSubscriptionSuccessful?.Invoke();

            EditorWindow.GetWindow<SturfeeConfigurationWindow>().Focus();
        }

        private void HandleSubscriptionError(string error)
        {
            _loadingSubscription = false;

            _accessLevel = 0;

            _accessTokenValid = false;
            EditorApplication.update -= SturfeeSubscriptionManager.OnResponse;
            EditorWindow.GetWindow<SturfeeConfigurationWindow>().Focus();

        }

        private void SaveConfiguration()
        {
            var configuration = new SturfeeConfiguration
            {
                AccessToken = _apiKey,
                FileCacheLimit = 0,
                MemoryCacheLimit = 0,
                TileSize = _tileSize,
                Language = _language
                //SpatialRefGps = _spatialRefGps                
            };

            var json = JsonUtility.ToJson(configuration);
            File.WriteAllText(_configurationFile, json);
            AssetDatabase.Refresh();
            Repaint();

            // set the config
            _config = configuration;
        }

        private void LoadConfig()
        {
            _configurationFile = Path.Combine(Paths.SturfeeResourcesAbsolute, Paths.ConfigFile);

            // create the directory, if needed
            if (!Directory.Exists(Paths.SturfeeResourcesAbsolute))
            {
                Directory.CreateDirectory(Paths.SturfeeResourcesAbsolute);
            }

            // create a new config file
            if (!File.Exists(_configurationFile))
            {
                var json = JsonUtility.ToJson(new SturfeeConfiguration
                {
                    AccessToken = "N/A"
                });
                File.WriteAllText(_configurationFile, json);
            }

            TextAsset configurationTextAsset = Resources.Load<TextAsset>(Paths.SturfeeResourcesRelative);// _configurationFile);// Paths.SturfeeResourcesAbsolute);

            _config = configurationTextAsset == null ? null : JsonUtility.FromJson<SturfeeConfiguration>(configurationTextAsset.text);

            if (_config != null)
            {
                _apiKey = _config.AccessToken;
                //_spatialRefGps = _config.SpatialRefGps;
                _tileSize = _config.TileSize;
                _language = _config.Language;
                _loadingSubscription = true;
                CheckSubscription(_apiKey, HandleSubscriptionResult, HandleSubscriptionError); // validates against the server
                                                                      //var subscriptionInfo = SturfeeSubscriptionManager.GetSubscriptionInfo(_apiKey); // local
                                                                      //HandleSubscriptionResult(subscriptionInfo);
            }
        }

        private GUISkin BuildSturfeeSkin()
        {
            var skin = new GUISkin();
            //skin.box.normal.background = 

            return skin;
        }

        private static bool SturfeeLayersExist()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            Type type = typeof(SturfeeLayers);
            foreach (var layer in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var layerValue = layer.GetValue(null).ToString();
                for (int i = 8; i < layers.arraySize; i++)
                {
                    SerializedProperty layerSp = layers.GetArrayElementAtIndex(i);
                    if (layerSp.stringValue == layerValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void CreateSturfeeLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            Type type = typeof(SturfeeLayers);
            foreach (var layer in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var layerValue = layer.GetValue(null).ToString();
                bool existLayer = false;
                for (int i = 8; i < layers.arraySize; i++)
                {
                    SerializedProperty layerSp = layers.GetArrayElementAtIndex(i);
                    if (layerSp.stringValue == layerValue)
                    {
                        existLayer = true;
                        break;
                    }
                }
                for (int j = 8; j < layers.arraySize; j++)
                {
                    SerializedProperty layerSP = layers.GetArrayElementAtIndex(j);
                    if (layerSP.stringValue == "" && !existLayer)
                    {
                        layerSP.stringValue = layerValue;
                        tagManager.ApplyModifiedProperties();

                        break;
                    }
                }
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void AddSpace(int numberOfSpaces = 1)
        {
            for (int i = 0; i < numberOfSpaces; i++)
            {
                EditorGUILayout.Space();
            }
        }

        public static void CheckSubscription(string accessToken, Action<SturfeeSubscriptionInfo> callback, Action<string> errorCallback)
        {
            _www = SturfeeSubscriptionManager.GetSubscription(accessToken, callback, errorCallback);

            if (_www != null)
            {
                EditorApplication.update += SturfeeSubscriptionManager.OnResponse;
            }

            //_www.SendWebRequest();
        }
    }

}