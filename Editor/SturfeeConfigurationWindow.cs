using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using SturfeeVPS.Core;
using SturfeeVPS.UI;
using System.Reflection;
using UnityEditor.PackageManager;
using System.Threading;

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
        private EditorConfiguration _config = null;
        private Vector2 _scrollPosition;
        private Texture2D _logo;
        protected static int _currentTab = 0;

        private Color _sturfeePrimaryColor = new Color(25f / 255.0f, 190f / 255.0f, 200f / 255.0f);
        private Color _sturfeeSecondaryColor = new Color(238f / 255.0f, 66f / 255.0f, 102f / 255.0f);
        private Color _sturfeeErrorColor = new Color(183f / 255.0f, 48f / 255.0f, 48f / 255.0f);
        private Color _sturfeeDarkBackgroundColor = new Color(35f / 255.0f, 35f / 255.0f, 35f / 255.0f);

        private bool _loadingSubscription = false;
        private bool _accessTokenValid = false;
        private bool _setupFinished = false;
        private static UnityWebRequest _www;

        private static string _editorPath = @"Packages/com.sturfee.vps.sdk/Editor";
        private static string _runtimePath = @"Packages/com.sturfee.vps.sdk/Runtime";
        private static string _packagePath = @"Packages/com.sturfee.vps.sdk";



        // Config
        //private Languages _language = Languages.English;
        private TileSize _tileSize = TileSize.Small;
        private int _cacheDistance = 100;
        private int _cacheExpiration = 10;

        // Theme
        private ThemeAsset _themeAsset;

        // location
        private string _locationTxt;

        [MenuItem("Sturfee/Configure", false, 0)]
        public static void ShowWindow()
        {
            _configurationFile = Path.Combine(Paths.SturfeeResourcesAbsolute, Paths.ConfigFile);

            SturfeeConfigurationWindow window = EditorWindow.GetWindow<SturfeeConfigurationWindow>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(_editorPath + "/Images/sturfee_official_icon-black.png");
            GUIContent customTitleContent = new GUIContent("Sturfee", icon);
            window.titleContent = customTitleContent;
            window.Show();

            _currentTab = OpenToSection;
            //GUI.UnfocusWindow();
        }

        [InitializeOnLoadMethod]
        public static void Install()
        {
            if (!Directory.Exists(Paths.SturfeeResourcesAbsolute))
                Directory.CreateDirectory(Paths.SturfeeResourcesAbsolute);

            InstallLayers();
            InstallPackage();
            InstallLocales();
        }


        protected virtual void OnGUI()
        {
            if (_config == null)
            {
                LoadConfig();
            }

            GUI.skin = SturfeeSkin;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                ShowHeader();

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
                    _currentTab = GUILayout.Toolbar(_currentTab, new string[] { "Subscription", "Config", "Objects" });
                }

                switch (_currentTab)
                {
                    case 0:
                        SubscriptionTab();
                        break;
                    case 1:
                        ConfigTab();
                        break;
                    case 2:
                        ObjectsTab();
                        break;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void AddCustomizedConfiguration()
        {

        }

        private void ShowHeader()
        {
            var boxStyle = new GUIStyle(GUI.skin.label);
            boxStyle.normal.background = EditorGUIUtility.whiteTexture;

            EditorGUILayout.BeginVertical(boxStyle);// GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();//.FlexibleSpace();
                _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(_editorPath + "/Images/sturfee_official_logo-black_small.png");
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
                //                GUILayout.FlexibleSpace();
                //#if VPS_SRC
                //                var dllFile = new FileInfo($"Library/ScriptAssemblies/SturfeeVPS.Core.dll");
                //#else
                //                var dllFile = new FileInfo($"{_runtimePath}/Plugins/SturfeeVPS.Core.dll");
                //#endif
                //                string assemblyVersion = Assembly.LoadFile(dllFile.FullName).GetName().Version.ToString();
                //                GUILayout.Label("v" + assemblyVersion);
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
        }

        private void SubscriptionTab()
        {
            GUILayout.Label("Subscription Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("API Key", GUILayout.Width(100));
                _apiKey = GUILayout.TextArea(_apiKey);
                EditorGUILayout.EndHorizontal();
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
                            _editorPath + "/Images/Tier-1.png");
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
                            _editorPath + "/Images/Tier-2.png");
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
                                _editorPath + "/Images/Tier-3.png");
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
                            _editorPath + "/Images/Tier-4.png");
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
        }

        private void ConfigTab()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                //  Tiles
                {
                    GUILayout.Label("Tiles", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();

                    // Tile Size
                    var tileSize = (TileSize)EditorGUILayout.EnumPopup("Select Tile Load Size", _tileSize);
                    if (tileSize != _tileSize)
                    {
                        SturfeeMenu.ClearTileCache();
                    }
                    _tileSize = tileSize;

                    AddSpace(5);
                    EditorGUILayout.EndHorizontal();
                    AddSpace(2);

                    // Caching
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Caching", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Clear if", GUILayout.Width(100));
                    _cacheDistance = EditorGUILayout.IntSlider(_cacheDistance, 50, 300, GUILayout.Width(200));
                    EditorGUILayout.Space(1, false);
                    EditorGUILayout.LabelField("meters away from previous session location");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Clear after ", GUILayout.Width(100));
                    _cacheExpiration = EditorGUILayout.IntSlider(_cacheExpiration, 5, 50, GUILayout.Width(200));
                    EditorGUILayout.Space(1, false);
                    EditorGUILayout.LabelField("days");
                    //AddSpace(15);
                    EditorGUILayout.EndHorizontal();
                    AddSpace(2);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Clear Tile Cache", GUILayout.Width(200), GUILayout.Height(25)))
                    {
                        SturfeeMenu.ClearTileCache();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Theme
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.BeginHorizontal();
                    _themeAsset = (ThemeAsset)EditorGUILayout.ObjectField("Theme", _themeAsset, typeof(ThemeAsset), false, GUILayout.Width(500));

                    string path = AssetDatabase.GetAssetPath(_themeAsset);
                    if (_themeAsset == null || !path.Contains("Resources"))
                    {
                        _themeAsset = Resources.Load<ThemeAsset>("Config/SturfeeTheme");
                    }

                    EditorGUILayout.LabelField("\t Please select a theme from Resources folder only");
                    EditorGUILayout.EndHorizontal();
                }

                // Location
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.BeginHorizontal();
                    if (string.IsNullOrEmpty(_locationTxt))
                    {
                        _locationTxt = $"{EditorUtils.EditorFallbackLocation.Latitude},{EditorUtils.EditorFallbackLocation.Longitude}";
                    }

                    _locationTxt = EditorGUILayout.TextField("Fallback Location", _locationTxt, GUILayout.Width(500));
                    if (GUILayout.Button("Set", GUILayout.Width(100), GUILayout.Height(20)))
                    {
                        var location = new GeoLocation
                        {
                            Latitude = double.Parse(_locationTxt.Split(',')[0]),
                            Longitude = double.Parse(_locationTxt.Split(',')[1])
                        };
                        
                        EditorUtils.EditorFallbackLocation = location;
                        Debug.Log($"Editor fallback location set to {location.ToFormattedString()}");
                    }
                    EditorGUILayout.LabelField("\t This loation will be used when GPSProvider's gps takes more than 10 seconds to be ready");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(20)))
                    {
                        _locationTxt = $"{EditorUtils.EditorFallbackLocation.Latitude},{EditorUtils.EditorFallbackLocation.Longitude}";
                    }
                    EditorGUILayout.EndHorizontal();
                }


                AddCustomizedConfiguration();

                SaveConfiguration();

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void ObjectsTab()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            if (!_setupFinished && !SturfeeLayersExist())
            {
                GUILayout.Label("Unity Setup", EditorStyles.boldLabel);
                if (GUILayout.Button("First Time Setup", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    InstallLayers();
                    _setupFinished = true;
                }
            }
            else
            {
                GUILayout.Label("XR Objects", EditorStyles.boldLabel);
                if (GUILayout.Button("Create XRSession", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath($"{_runtimePath}/XRSession/SturfeeXRSession.prefab", typeof(GameObject));
                    GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    clone.name = prefab.name;
                }
                if (GUILayout.Button("Create XRCamera", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath($"{_runtimePath}/XRSession/XRCamera.prefab", typeof(GameObject));
                    GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    clone.name = prefab.name;
                }
                if (GUILayout.Button("Create XRLight", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath($"{_runtimePath}/XRSession/XRLight.prefab", typeof(GameObject));
                    GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    clone.name = prefab.name;
                }

                if (GUILayout.Button("Create WorldAnchor", GUILayout.Width(200), GUILayout.Height(30)))
                {
                    var anchor = new GameObject().AddComponent<WorldAnchor>();
                    anchor.gameObject.name = "WorldAnchor";
                }
            }
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
            if (_configurationFile == null)
                _configurationFile = Path.Combine(Paths.SturfeeResourcesAbsolute, Paths.ConfigFile);


            var configuration = new EditorConfiguration
            {
                AccessToken = _apiKey,
                TileSize = _tileSize,
            };

            if (_themeAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(_themeAsset);
                string resourcesPath = path.Substring(path.IndexOf("Resources") + "Resources".Length + 1);
                configuration.Theme = new Theme
                {
                    Path = resourcesPath,
                    Locale = _themeAsset.Locale
                };
            };

            var json = JsonUtility.ToJson(configuration);
            File.WriteAllText(_configurationFile, json);
            //AssetDatabase.Refresh();
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
                var json = JsonUtility.ToJson(new EditorConfiguration
                {
                    AccessToken = "N/A",
                    Theme = new Theme
                    {
                        Path = "Config/SturfeeTheme.asset",
                        Locale = "en-Us"
                    }
                });
                File.WriteAllText(_configurationFile, json);
                AssetDatabase.Refresh();
                Repaint();
            }

            TextAsset configurationTextAsset = Resources.Load<TextAsset>(Paths.SturfeeResourcesRelative);// _configurationFile);// Paths.SturfeeResourcesAbsolute);

            _config = configurationTextAsset == null ? null : JsonUtility.FromJson<EditorConfiguration>(configurationTextAsset.text);

            if (_config != null)
            {
                _apiKey = _config.AccessToken;
                //_spatialRefGps = _config.SpatialRefGps;
                _tileSize = _config.TileSize;
                _themeAsset = Resources.Load<ThemeAsset>(_config.Theme.Path.Split('.')[0]);
                _loadingSubscription = true;
                CheckSubscription(_apiKey, HandleSubscriptionResult, HandleSubscriptionError); // validates against the server
                                                                                               //var subscriptionInfo = SturfeeSubscriptionManager.GetSubscriptionInfo(_apiKey); // local
                                                                                               //HandleSubscriptionResult(subscriptionInfo);
            }
        }

        private static string LanguageToLocale(Languages language)
        {
            switch (language)
            {
                case Languages.English: return "en-US";
                case Languages.Japanese: return "ja";
                default: return "en-US";
            }
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

        private static void InstallLayers()
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

        protected void AddSpace(int numberOfSpaces = 1)
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

        private static void InstallPackage()
        {
            // major.minor[.build[.revision] 
            // build => 0 -> beta, 1 -> production 

            if (!Directory.Exists(Paths.SturfeeResourcesAbsolute)) Directory.CreateDirectory(Paths.SturfeeResourcesAbsolute);

            var listRequest = Client.List(true, true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(100);

            if (listRequest.Error != null)
            {
                Debug.Log("Error: " + listRequest.Error.message);
                return;
            }
            string pkg = "";
            var packages = listRequest.Result;
            foreach (var package in packages)
            {
                if (package.name == "com.sturfee.vps.sdk")
                {
                    pkg = package.version;
                }
            }

            if (string.IsNullOrEmpty(pkg))
            {
                Debug.LogError(" No package found for com.sturfee.vps.sdk");
                return;
            }

            Debug.Log($"pkg version => {pkg}");

            if (pkg.Contains("pre"))
            {
                pkg = ParsePreviewPackage(pkg);
            }
            var packageVersion = new Version(pkg);

            if (!File.Exists($"{Paths.SturfeeResourcesAbsolute}/version.txt"))
            {
                AssetDatabase.ImportPackage($"{_packagePath}/Sturfee-VPS-SDK.unityPackage", true);
                File.WriteAllText($"{Paths.SturfeeResourcesAbsolute}/version.txt", pkg.ToString());
            }

            string current = File.ReadAllText($"{Paths.SturfeeResourcesAbsolute}/version.txt");
            var currentVersion = new Version(current);
            Debug.Log($"current version => {current}");

            if (currentVersion.CompareTo(packageVersion) < 0)
            {
                Debug.Log(" Updating Sturfee VPS unitypackage");
                AssetDatabase.ImportPackage($"{_packagePath}/Sturfee-VPS-SDK.unityPackage", true);
                File.WriteAllText($"{Paths.SturfeeResourcesAbsolute}/version.txt", pkg.ToString());
            }
        }

        private static void InstallLocales()
        {
            string[] defaultLocales = new string[] { "en-US", "ja-JP" };
            // Copy string resources from package to local
            foreach (string locale in defaultLocales)
            {
                string src = Path.Combine(Application.dataPath, $"SturfeeVPS/Resources/Strings/Sturfee.StringResources.{locale}.xml");

                string stringsDir = Path.Combine(Paths.SturfeeResourcesAbsolute, "Strings");
                if (!Directory.Exists(stringsDir)) Directory.CreateDirectory(stringsDir);
                string dest = $"{stringsDir}/Sturfee.StringResources.{locale}.xml";

                if (!File.Exists(dest))
                {
                    Debug.Log($" Installing Locale {locale}");
                    File.Copy(src, dest, true);
                }
            }
        }

        private static string ParsePreviewPackage(string previewPackage)
        {
            // major.minor.build-pre.rev => major.minor.buildrev
            // ex: 3.2.0-prev.1 => 3.2.0.1
            string[] parts = previewPackage.Split('.');
            var major = parts[0];
            var minor = parts[1];
            var build = parts[2].Split('-')[0];
            var revision = parts[3];

            string package = $"{major}.{minor}.{build}.{revision}";
            Debug.Log($"{previewPackage} updated to {package}");

            return package;
        }
    }

}