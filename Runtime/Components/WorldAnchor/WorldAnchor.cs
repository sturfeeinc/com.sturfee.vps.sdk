using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Positions XR assets of a space to it's geo-location in the Unity scene
    /// </summary>
    public class WorldAnchor : MonoBehaviour
    {
        public GeoLocation Location;
        [HideInInspector]
        public bool _editLocation;


        public void Update()
        {
            if (_editLocation)
            {
                UpdateLocation();
            }
            else
            {
                UpdatePosition();
            }
        }

        /// <summary>
        /// Called in update loop to set the position of the asset. Geo-location is passed through SturfeeVPS.SDK.Converters.GeoToUnityPosition to get position of the asset in Unity space.
        /// </summary>
        public void UpdatePosition()
        {
            if(PositioningUtils.GetReferenceUTM == null)
            {
                if(Application.isPlaying)
                {
                    return;
                }

#if UNITY_EDITOR
                PositioningUtils.Init(EditorUtils.EditorFallbackLocation);
#endif
            }

            transform.position = Converters.GeoToUnityPosition(Location);
        }

        /// <summary>
        /// Called in update loop if _editLocation is set to True. It synchronizes Location variable to the asset's current position. 
        /// </summary>
        public void UpdateLocation()
        {
            if (PositioningUtils.GetReferenceUTM == null)
            {
                if (Application.isPlaying)
                {
                    return;                    
                }
#if UNITY_EDITOR    
                PositioningUtils.Init(EditorUtils.EditorFallbackLocation);
#endif
            }

            Location = Converters.UnityToGeoLocation(transform.position);
        }


        #region Editor UI
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Sturfee/WorldAnchor-Icon.png", true);
        }

        [MenuItem("GameObject/SturfeeXR/WorldAnchor")]
        static void CreateNewWorldAnchor(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("WorldAnchor");
            go.AddComponent<WorldAnchor>().Location = EditorUtils.EditorFallbackLocation;
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
          
            var worldAnchor = (WorldAnchor)target;
            worldAnchor.Update();
            GUILayout.BeginHorizontal();
            {
                // Start updating GPSPosition when local position changes

                GUI.backgroundColor = worldAnchor._editLocation ? Color.green : Color.white;
                if (GUILayout.Button("Edit"))
                {
                    worldAnchor._editLocation = !worldAnchor._editLocation;
                }

                GUI.backgroundColor = Color.white;
                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Save"))
                    {
                        if (worldAnchor._editLocation)
                        {
                            EditorApplication.playModeStateChanged += EditGps;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            
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

            worldAnchor._editLocation = false;
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