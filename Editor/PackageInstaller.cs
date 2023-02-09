using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK.Editor
{
    public class PackageInstaller : MonoBehaviour
    {
        private static string _editorPath = @"Packages/com.sturfee.vps.sdk/Editor";

        [InitializeOnLoadMethod]
        public static void Install()
        {
            InstallLayers();
            InstallStringResources();
            InstallTheme();
        }


        [MenuItem("Sturfee/DigitalTwin/Set Layers")]
        public static void InstallLayers()
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

        public static void InstallStringResources()
        {
            string[] supportedLocales = new string[] { "en-US", "ja-JP" };
            // Copy string resources from package to local
            foreach (string locale in supportedLocales)
            {
                string src = Path.Combine(_editorPath, $"Resources/Strings/Sturfee.StringResources.{locale}.xml");

                string stringsDir = Path.Combine(Paths.SturfeeResourcesAbsolute, "Strings");
                if (!Directory.Exists(stringsDir)) Directory.CreateDirectory(stringsDir);
                string dest = $"{stringsDir}/Sturfee.StringResources.{locale}.xml";

                if (!File.Exists(dest))
                {
                    Debug.Log($" Installing string resource for Locale {locale}");
                    File.Copy(src, dest, true);
                }
            }
        }

        public static void InstallTheme()
        {
            string src = Path.Combine(_editorPath, $"Resources/Themes/SturfeeTheme.asset");
            string themesDir = Path.Combine(Paths.SturfeeResourcesAbsolute, "Themes");
            if (!Directory.Exists(themesDir)) Directory.CreateDirectory(themesDir);

            string dest = $"{themesDir}/SturfeeTheme.asset";

            if (!File.Exists(dest))
            {
                Debug.Log($" Installing SturfeeTheme");
                File.Copy(src, dest, true);
            }

        }
    }
}
