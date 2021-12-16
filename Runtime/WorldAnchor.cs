using System.Collections;
using System.Collections.Generic;
using SturfeeVPS.Core;
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class WorldAnchor : MonoBehaviour
    {
        public enum SetPositionOn
        {
            TilesLoaded,
            SessionReady,
            LocalizationSuccessful
        }
        [Tooltip("When should the GPS location be used: after the XR Session is ready or after Localization")]
        public SetPositionOn SetPositionWhen = SetPositionOn.SessionReady;

        [SerializeField]
        private GeoLocation _location;
        [HideInInspector]
        public bool _editGPS;

        private void Start()
        {
            SturfeeEventManager.Instance.OnTilesLoaded += OnTilesLoaded;
            SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
            SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful; 
        }

        private void LateUpdate()
        {
            if (_editGPS)
            {
                _location = Converters.UnityToGeoLocation(transform.position);
            }
        }

        private void OnEnable()
        {
            SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
            SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful;
        }

        public GeoLocation Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;

                // Not needed in Edit mode
                if (Application.isPlaying)
                {
                    SetLocation();
                }
            }
        }

        private void OnTilesLoaded()
        {
            if (SetPositionWhen == SetPositionOn.TilesLoaded)
            {
                UpdatePos();
            }
        }

        private void OnSessionReady()
        {
            if(SetPositionWhen == SetPositionOn.SessionReady)
            {
                UpdatePos();
            }
        }

        private void OnLocalizationSuccessful()
        {
            UpdatePos();
        }

        private void UpdatePos()
        {
            if(_location.Latitude == 0 && _location.Longitude == 0)
            {
                SturfeeDebug.LogError("WorldAnchor Location is not set");
                return;
            }
            transform.position = Converters.GeoToUnityPosition(_location);
        }

        private void SetLocation()
        {
            var xrsession = XRSessionManager.GetSession();
            if (xrsession != null)
            {
                if (xrsession?.Status < XRSessionStatus.Ready)
                {
                    SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
                    SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful; ;
                }
                else if (SetPositionWhen == SetPositionOn.SessionReady)
                {
                    UpdatePos();
                }
                else
                {
                    SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful;
                }
            }
        }

        #region Editor UI
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Sturfee/WorldAnchor-Icon.png", true);
        }

        [MenuItem("GameObject/SturfeeVPS/WorldAnchor", false, 11)]
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
                        if (PrefabUtility.GetPrefabInstanceStatus(wa) == PrefabInstanceStatus.Connected)
                        //if (PrefabUtility.GetPrefabType(wa) == PrefabType.PrefabInstance)
                        {
                            PrefabUtility.UnpackPrefabInstance(wa.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
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