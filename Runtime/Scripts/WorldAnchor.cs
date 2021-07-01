using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using SturfeeVPS.Core;

namespace SturfeeVPS.SDK
{
    public class WorldAnchor : MonoBehaviour
    {
        public enum SetPositionOn
        {
            SessionReady,
            LocalizationSuccessful
        }
        [Tooltip("When should the GPS location be used: after the XR Session is ready or after Localization")]
        public SetPositionOn SetPositionWhen = SetPositionOn.SessionReady;
        public GeoLocation Location;

        [HideInInspector]
        public bool _editGPS;

        private void Awake()
        {
            SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
            SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful;
        }

        private void LateUpdate()
        {
            if (_editGPS)
            {
                Location = Converters.UnityToGeoLocation(transform.position);
            }
        }

        private void OnDestroy()
        {
            SturfeeEventManager.Instance.OnSessionReady -= OnSessionReady;
            SturfeeEventManager.Instance.OnLocalizationSuccessful -= OnLocalizationSuccessful;
        }

        private void OnSessionReady()
        {
            if (SetPositionWhen == SetPositionOn.SessionReady)
            {
                transform.position = Converters.GeoToUnityPosition(Location);
            }
        }

        private void OnLocalizationSuccessful()
        {
            if (SetPositionWhen == SetPositionOn.LocalizationSuccessful)
            {
                transform.position = Converters.GeoToUnityPosition(Location);                    
            }
        }

        #region Editor UI
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Sturfee/WorldAnchor-Icon.png", true);
        }

        [MenuItem("GameObject/Create Other/Sturfee/WorldAnchor")]
        static void CreateNewWorldAnchor(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("WorldAnchor");
            go.AddComponent<WorldAnchor>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif
        #endregion
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(WorldAnchor))]
    [CanEditMultipleObjects]
    public class WorldAnchorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                var worldAnchor = (WorldAnchor)target;

                // Start updating GPSPosition when local position changes
                if (GUILayout.Button("Edit"))
                {
                    worldAnchor._editGPS = true;
                }

                if (GUILayout.Button("Save"))
                {
                    if (worldAnchor._editGPS)
                    {
                        EditorApplication.playModeStateChanged += EditGps;
                    }
                }
            }
        }

        private void EditGps(PlayModeStateChange state)
        {
            WorldAnchor worldAnchor = (WorldAnchor)target;

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // Save current GPSPosition of target WorldAnchor to EditorPrefs
                string saveKey = "WorldAnchorSaveData_" + worldAnchor.GetInstanceID();
                string serializedGPS = EditorJsonUtility.ToJson(worldAnchor.Location);
                EditorPrefs.SetString(saveKey, serializedGPS);
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Assign GPSPosition to all the worldAnchors in the scene by reading its value from EditorPrefs
                foreach (WorldAnchor wa in FindObjectsOfType<WorldAnchor>())
                {
                    string savedKey = "WorldAnchorSaveData_" + wa.GetInstanceID();
                    if (EditorPrefs.HasKey(savedKey))
                    {
                        string serializedGPS = EditorPrefs.GetString(savedKey);
                        var updatedGps = new GeoLocation();
                        EditorJsonUtility.FromJsonOverwrite(serializedGPS, updatedGps);

                        // Disconnect Prefab connection(if any) otherwise values will reset to what it was before Play
                        if (PrefabUtility.GetPrefabType(wa) == PrefabType.PrefabInstance)
                        {
                            PrefabUtility.DisconnectPrefabInstance(wa);
                        }

                        wa.Location = updatedGps;
                    }
                }
            }

            worldAnchor._editGPS = false;
        }

        private void OnSceneGUI()
        {
            WorldAnchor _target = (WorldAnchor)target;

            Handles.CircleHandleCap(
                0,
                _target.transform.position + new Vector3(0f, -0.5f, 0f),
                _target.transform.rotation * Quaternion.LookRotation(Vector3.up),
                2,
                EventType.Repaint
            );
        }
    }

#endif
}
