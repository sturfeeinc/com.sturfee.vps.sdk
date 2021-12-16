using UnityEngine;
using UnityEditor;
using System.Collections;
using SturfeeVPS.Core;
using System.Linq;
using System.Reflection;
using System;

namespace SturfeeVPS.SDK
{
    [ExecuteInEditMode]    
    public class ProviderManager : MonoBehaviour
    {         
        private SturfeeXRSession _xrSession;

        private void OnEnable()
        {            
            _xrSession = GetComponent<SturfeeXRSession>();
            hideFlags = HideFlags.HideInInspector;
        }

        public void OnProviderSetChanged(ProviderSet providerSet)
        {
            Clear();
            CopyProviderComponents(providerSet.gameObject, _xrSession.gameObject);
            StartCoroutine(SetProviderReferences());
        }        

        private void Clear()
        {
            _xrSession.PoseProvider = null;
            _xrSession.GpsProvider = null;
            _xrSession.VideoProvider = null;

#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(gameObject);
            var prop = serializedObject.FindProperty("m_Component");

            RemoveAllComponents();  

            serializedObject.ApplyModifiedProperties();
#endif
        }

        private void CopyProviderComponents(GameObject sourceGO, GameObject targetGO)
        {
            foreach (var component in sourceGO.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                var componentType = component.GetType();
                if (componentType != typeof(Transform) &&
                    componentType != typeof(ProviderSet)
                )
                {
#if UNITY_EDITOR
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGO);
#endif
                }
            }
        }

        private IEnumerator SetProviderReferences()
        {
            yield return new WaitForEndOfFrame();

            // Set SturfeeXrSession provider references
            _xrSession.PoseProvider = _xrSession.gameObject.GetComponent<PoseProviderBase>();
            _xrSession.GpsProvider = _xrSession.gameObject.GetComponent<GpsProviderBase>();
            _xrSession.VideoProvider = _xrSession.gameObject.GetComponent<VideoProviderBase>();
        }

        private void RemoveAllComponents()
        {
            int length = GetComponents<Component>().Length;
            if (length<= 3) // Transform, SturfeeXRSession, ProviderManager
            {
                return;
            }

            foreach (var comp in GetComponents<Component>())
            {
                //Don't remove the Transform  and SturfeeXRSession component
                if (!(comp is Transform) && !(comp is SturfeeXRSession))
                {
                    //Don't remove this script
                    if (!(comp is ProviderManager))
                    {
                        if (!IsDependentOnAnotherComponent(comp))
                        {
                            DestroyImmediate(comp);
                        }
                    }
                }
            }

            RemoveAllComponents();
        }

        private bool IsDependentOnAnotherComponent(Component component)
        {
            // Look for all the components that has "RequireComponent" atttribute
            foreach (var comp in GetComponents<Component>())
            {
                if (comp.GetType().IsDefined(typeof(RequireComponent), false))
                {
                    foreach(RequireComponent rc in comp.GetType().GetCustomAttributes<RequireComponent>())
                    { 
                        Type rcType = rc.m_Type0 != null ? rc.m_Type0 : (rc.m_Type1 != null ? rc.m_Type1 : (rc.m_Type2 != null ? rc.m_Type2 : null));
                        if(rcType == null)
                        {
                            Debug.LogError(comp.GetType().ToString() + " has RequireComponent attribute whose valuse is NULL");
                        }

                        if (rcType == component.GetType())
                        {
                            //Debug.Log(component.GetType().ToString() + " has a dependency on " + comp.GetType().ToString());
                            return true;
                        }
                    }

                    //Debug.Log(rc.m_Type1.ToString());
                    //Debug.Log(rc.m_Type2.ToString());
                }
            }

            return false;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SturfeeXRSession))]
    public class XrSessionProvidersEditor : Editor
    {
        private ProviderSet[] _providerSets;
        private string[] _providerSetNames;

        private SerializedProperty _selected;


        private void OnEnable()
        {

            _providerSets = Resources.LoadAll<ProviderSet>("Provider Sets");
            _providerSetNames = new string[_providerSets.Length];
            for(int i=0; i < _providerSets.Length; i++)
            {
                _providerSetNames[i] = _providerSets[i].name;
            }

            _selected = serializedObject.FindProperty("SelectedProvider");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            _selected.intValue = EditorGUILayout.Popup("Provider Set", _selected.intValue, _providerSetNames, GUILayout.Height(20));

            SturfeeXRSession sturfeeXRSession = ((SturfeeXRSession)target);
            if (_selected.intValue != sturfeeXRSession.SelectedProvider)
            {
                sturfeeXRSession.GetComponent<ProviderManager>().OnProviderSetChanged(_providerSets[_selected.intValue]);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}