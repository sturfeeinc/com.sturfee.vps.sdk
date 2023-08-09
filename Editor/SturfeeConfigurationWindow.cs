using SturfeeVPS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using SturfeeVPS.Core;

namespace SturfeeVPS.SDK.Editor
{
    public class SturfeeConfigurationWindow : EditorWindow
    {
        // header
        private Texture2D _logo;

        // Colors
        private Color _sturfeePrimaryColor = new Color(25f / 255.0f, 190f / 255.0f, 200f / 255.0f);
        private Color _sturfeeSecondaryColor = new Color(238f / 255.0f, 66f / 255.0f, 102f / 255.0f);
        private Color _sturfeeErrorColor = new Color(183f / 255.0f, 48f / 255.0f, 48f / 255.0f);
        private Color _sturfeeDarkBackgroundColor = new Color(35f / 255.0f, 35f / 255.0f, 35f / 255.0f);

        protected string[] _tabs = new string[] {"Auth", "Config"};

        // paths
        protected static string _editorPath = @"Packages/com.sturfee.vps.sdk/Editor";
        protected static string _runtimePath = @"Packages/com.sturfee.vps.sdk/Runtime";
        protected static string _packagePath = @"Packages/com.sturfee.vps.sdk";
        protected static int _currentTab = 0;

        private SturfeeWindowAuth _sturfeeWindowAuth;
        private SturfeeWindowConfig _sturfeeWindowConfig;

        // VPS token
        private bool _loadingTokenValidation = false;
        private bool _vpsTokenValid = false;

        // Editor fallback location
        private string _locationTxt;


        [MenuItem("Sturfee/Configure", false, 0)]
        public static void ShowWindow()
        {
            //_configurationFile = Path.Combine(Paths.SturfeeResourcesAbsolute, Paths.ConfigFile);

            SturfeeConfigurationWindow window = GetWindow<SturfeeConfigurationWindow>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(_editorPath + "/Images/sturfee_official_icon-black.png");
            GUIContent customTitleContent = new GUIContent("Sturfee", icon);
            window.titleContent = customTitleContent;
            window.Show();

            //_currentTab = OpenToSection;
            ////GUI.UnfocusWindow();
        }

        protected virtual void OnGUI()
        {
            if(_sturfeeWindowAuth == null)
            {
                _sturfeeWindowAuth = SturfeeWindow.Auth;
                ValidateVpsToken(_sturfeeWindowAuth.VpsToken);
            }

            if (_sturfeeWindowConfig == null)
            {
                _sturfeeWindowConfig = SturfeeWindow.Config;
            }

            ShowHeader();

            _currentTab = GUILayout.Toolbar(_currentTab, _tabs);
            switch (_currentTab)
            {
                case 0:CreateTab(OnAuthTab); break;
                case 1:CreateTab(OnConfigTab); break;  
            }            
        }

        protected virtual void ShowHeader()
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

                //if (_accessTokenValid)
                //{
                //    AddSpace(4);
                //}
                EditorGUILayout.Space();


                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void OnAuthTab()
        {
            HandleVpsToken();
        }
        
        protected virtual void OnConfigTab()
        {
            HandleTheme();
            HandleEditorFallbackLocation();
            HandleVpsLayers();
        }

        protected virtual void HandleVpsToken()
        {

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("VPS Token", GUILayout.Width(100));
                _sturfeeWindowAuth.VpsToken = GUILayout.TextArea(_sturfeeWindowAuth.VpsToken);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (_loadingTokenValidation)
                {
                    GUILayout.Label("Loading..", GUILayout.Height(30));
                }
                else
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = _vpsTokenValid ? Color.green : Color.red;                    
                    GUILayout.Label(_vpsTokenValid ? "Token is valid" : "Invalid token", style, GUILayout.Height(30));

                }
                GUILayout.FlexibleSpace();


                if (GUILayout.Button("Request Access", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    SaveAuth();
                    ValidateVpsToken(_sturfeeWindowAuth.VpsToken);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        }

        protected virtual void HandleTheme()
        {
            EditorGUILayout.BeginHorizontal();
            {
                var themeAsset = Resources.Load<ThemeAsset>(_sturfeeWindowConfig.Theme);
                var selected = (ThemeAsset)EditorGUILayout.ObjectField("Theme",themeAsset, typeof(ThemeAsset), false, GUILayout.Width(500));
                if (selected != themeAsset)
                {
                    if (selected != null)
                    {
                        string path = AssetDatabase.GetAssetPath(selected);
                        string resourcesPath = path.Substring(path.IndexOf("Resources") + "Resources".Length + 1);
                        resourcesPath = resourcesPath.Split('.')[0]; // remove extension
                        _sturfeeWindowConfig.Theme = resourcesPath;
                    }
                    else
                    {
                        _sturfeeWindowConfig.Theme = "";
                    }

                    themeAsset = selected;

                    SaveConfig();
                }

                if (themeAsset != null)
                {
                    if (GUILayout.Button("Theme Settings", GUILayout.Width(150)))
                    {
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = themeAsset;
                    }
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        protected virtual void HandleEditorFallbackLocation()
        {
            EditorGUILayout.BeginHorizontal();
            {
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
                EditorGUILayout.LabelField("\t This loation can only be used in editor by calling \"EditorUtils.EditorfallbackLocation\"");                
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(20)))
            {
                _locationTxt = $"{EditorUtils.EditorFallbackLocation.Latitude},{EditorUtils.EditorFallbackLocation.Longitude}";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        protected virtual void HandleVpsLayers()
        {
            GUILayout.Label("VPS Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            if (GUILayout.Button("Install VPS Layers", GUILayout.Width(150)))
            {
                InstallVpsLayers();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        protected virtual void SaveAuth()
        {
            if (_sturfeeWindowAuth != null)
            {
                string path = Path.Combine(Paths.SturfeeResourcesAbsolute, "SturfeeWindowAuth.txt");
                File.WriteAllText(path, JsonConvert.SerializeObject(_sturfeeWindowAuth));
                AssetDatabase.Refresh();
                Repaint();
            }
        }

        protected virtual void SaveConfig()
        {
            if (_sturfeeWindowConfig != null)
            {
                string path = Path.Combine(Paths.SturfeeResourcesAbsolute, "SturfeeWindowConfig.txt");
                File.WriteAllText(path, JsonConvert.SerializeObject(_sturfeeWindowConfig));
                AssetDatabase.Refresh();
                Repaint();
            }
        }

        private async void ValidateVpsToken(string token)
        {
            _loadingTokenValidation = true;
            try
            {
                await VpsServices.ValidateToken(_sturfeeWindowAuth.VpsToken);
                _vpsTokenValid = true;
                _loadingTokenValidation = false;
            }
            catch (Exception ex)
            {                
                _vpsTokenValid = false;
                _loadingTokenValidation = false;
            }
        }

        private void CreateTab(Action onTabCreated)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            {
                onTabCreated();
            }
            EditorGUILayout.EndVertical();
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

        public static void InstallVpsLayers()
        {
            Debug.Log($"Installing VPS layers..");
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
            Debug.Log($"Done!");
        }
    }

    [Serializable]
    public class DTSturfeeWindowAuth : SturfeeWindowAuth
    {
        public AppKeyConfig Android;
        public AppKeyConfig IOS;
        public AppKeyConfig Desktop;
    }

    [Serializable]
    public class AppKeyConfig
    {
        public string ApiKey;
        public string SourceHeader;
        public string SourceId;
    }

    public enum SupportedLocales 
    {        
        en,
        ja
    }

    public static class SupportedLocaless   
    {
        public static string English = "en-US";
        public static string Japanese = "ja-JP";

        public static string[] All = new string[] { English, Japanese };
    }
}
