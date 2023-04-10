using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{

    [Serializable]
    public class ProviderSet
    {
        public string Name;
        public string DisplayName;
        public XrSessionProviders Providers;
    }

    /// <summary>
    /// Providers used during an XR session: GPS, Pose, Video, Tiles and Localization providers. They are registered to SturfeeEventManager at the start of XR session or when provider set is switched.
    /// </summary>
    [Serializable]
    public class XrSessionProviders
    {
        public BaseGpsProvider GpsProvider;
        public BasePoseProvider PoseProvider;
        public BaseVideoProvider VideoProvider;
        public BaseTilesProvider TilesProvider;
        public BaseLocalizationProvider LocalizationProvider;

        public void ReplacePrefabWithInstance<T>(T provider) where T : IProvider
        {
            switch (provider)
            {
                case BaseGpsProvider gps:
                    if (GpsProvider != null)
                        GpsProvider = gps;
                    break;
                case BasePoseProvider pose:
                    if (PoseProvider != null)
                        PoseProvider = pose;
                    break;
                case BaseVideoProvider video:
                    if (VideoProvider != null)
                        VideoProvider = video;
                    break;
                case BaseTilesProvider tiles:
                    if (TilesProvider != null)
                        TilesProvider = tiles;
                    break;
                case BaseLocalizationProvider localization:
                    if (LocalizationProvider != null)
                        LocalizationProvider = localization;
                    break;
            }
        }
    }

}
