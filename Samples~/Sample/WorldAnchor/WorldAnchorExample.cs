using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDk.Samples
{
    public class WorldAnchorExample : MonoBehaviour
    {
        public GeoLocation[] AnchorsToCreateAtRuntime;

        private List<WorldAnchor> _worldAnchors = new List<WorldAnchor>();


        private void Awake()
        {
            SturfeeEventManager.OnSessionReady += OnSessionReady;
        }

        private void OnSessionReady()
        {
            _worldAnchors.AddRange(FindObjectsOfType<WorldAnchor>());

            // create runtime anchors
            foreach (var location in AnchorsToCreateAtRuntime)
            {
                WorldAnchor worldAnchor = new GameObject().AddComponent<WorldAnchor>();
                worldAnchor.Location = location;
                worldAnchor.name = "WorldAnchor_Runtime";
                _worldAnchors.Add(worldAnchor);
            }

        }

        private void Update()
        {
            if (XrSessionManager.GetSession() == null)
                return;

            var localizationProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>();
            if (localizationProvider != null)
            {
                _worldAnchors.ForEach(wa => wa.gameObject.SetActive(localizationProvider.GetProviderStatus() == ProviderStatus.Ready));
            }
            else
            {
                _worldAnchors.ForEach(wa => wa.gameObject.SetActive(true));
            }
        }

    }
}