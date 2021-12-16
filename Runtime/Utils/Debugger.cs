using System.Collections;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class Debugger : MonoBehaviour
    {
        [SerializeField]
        private GameObject _projector;

        private GameObject _sturfeeTiles;

        private void Awake()
        {
            SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
        }

        public void ToggleDebugBuildings()
        {
            _projector.SetActive(!_projector.activeSelf);
        }

        public void ToggleOcclusion()
        {
            _sturfeeTiles.SetActive(!_sturfeeTiles.activeSelf);
        }

        private void OnSessionReady()
        {
            StartCoroutine(SetPositionDelayed());
        }

        private IEnumerator SetPositionDelayed()
        {
            yield return new WaitForEndOfFrame();

            transform.position = XRCamera.Pose.Position;
            _sturfeeTiles = GameObject.Find(SturfeeObjects.SturfeeTilesObject);
        }
    }
}