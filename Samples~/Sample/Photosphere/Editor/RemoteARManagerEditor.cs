#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK.Samples
{
    [CustomEditor(typeof(RemoteARManager))]
    public class RemoteARManagerEditor : UnityEditor.Editor
    {
        protected SerializedProperty SampleNameProperty;
        private SerializedProperty _cacheProperty;

        public virtual void OnEnable()
        {   
            _cacheProperty = serializedObject.FindProperty("Cache");
            SampleNameProperty = serializedObject.FindProperty("SampleName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            // Sample Name
            EditorGUILayout.BeginHorizontal();

            SampleNameProperty.stringValue = EditorGUILayout.TextField(SampleNameProperty.displayName,
            SampleNameProperty.stringValue);

            if (GUILayout.Button("Reset"))
            {
                OnSampleNameReset();
            }

            EditorGUILayout.EndHorizontal();


            // Cache
            EditorGUILayout.BeginHorizontal();

            _cacheProperty.boolValue = EditorGUILayout.Toggle(_cacheProperty.displayName,
                _cacheProperty.boolValue);
            EditorGUILayout.Space();

            if (GUILayout.Button("Clear cache"))
            {
                ((RemoteARManager)target).ClearCache();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

        }

        public virtual void OnSampleNameReset()
        {

        }
    }
}
#endif