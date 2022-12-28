#if UNITY_EDITOR
using UnityEditor;

namespace SturfeeVPS.SDK.Samples
{
    [CustomEditor(typeof(PhotosphereManager))]
    public class PhotosphereManagerEditor : RemoteARManagerEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();            
        }
    }
}
#endif