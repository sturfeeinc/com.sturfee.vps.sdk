using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class XrSessionDebugger : MonoBehaviour
    {

        [SerializeField]
        private GeoLocation _xrLocation;
        [SerializeField]
        private Quaternion _xrOrientation;
        [SerializeField]
        private Vector3 _positionOffset;
        [SerializeField]
        private Quaternion _rotationOffset;
        [SerializeField]
        private string _gpsProvider;
        [SerializeField]
        private string _poseProvider;
        [SerializeField]
        private string _videoProvider;
        [SerializeField]
        private string _tilesProvider;
        [SerializeField]
        private string _localizatinoProvider;
        [SerializeField]
        private BaseProvider _gps;
        [SerializeField]
        private BaseProvider _pose;
        [SerializeField]
        private BaseProvider _video;
        [SerializeField]
        private BaseProvider _tiles;
        [SerializeField]
        private BaseProvider _localization;

        private void Update()
        {
            if (XrSessionManager.GetSession() == null)
                return;

            _xrLocation = XrSessionManager.GetSession()?.Location;
            _xrOrientation = XrSessionManager.GetSession() == null ? Quaternion.identity : XrSessionManager.GetSession().Orientation;
            _positionOffset = Converters.WorldToUnityPosition(XrSessionManager.GetSession().PositionOffset);
            _rotationOffset = Converters.WorldToUnityRotation(XrSessionManager.GetSession().RotationOffset);

            var gpsProvider = XrSessionManager.GetSession().GetProvider<IGpsProvider>();
            if (gpsProvider is BaseProvider)
            {
                _gps = (BaseProvider)gpsProvider;
            }

            var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
            if (poseProvider is BaseProvider)
            {
                _pose = (BaseProvider)poseProvider;
            }

            var videoProvider = XrSessionManager.GetSession().GetProvider<IVideoProvider>();
            if (videoProvider is BaseProvider)
            {
                _video = (BaseProvider)videoProvider;
            }

            var tilesProvider = XrSessionManager.GetSession().GetProvider<ITilesProvider>();
            if (tilesProvider is BaseProvider)
            {
                _tiles = (BaseProvider)tilesProvider;
            }

            var localizationProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>();
            if (localizationProvider is BaseProvider)
            {
                _localization = (BaseProvider)localizationProvider;
            }
            _gpsProvider = XrSessionManager.GetSession().GetProvider<IGpsProvider>()?.GetType().Name;
            _poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>()?.GetType().Name;
            _videoProvider = XrSessionManager.GetSession().GetProvider<IVideoProvider>()?.GetType().Name;
            _tilesProvider = XrSessionManager.GetSession().GetProvider<ITilesProvider>()?.GetType().Name;
            _localizatinoProvider = XrSessionManager.GetSession().GetProvider<ILocalizationProvider>()?.GetType().Name;
        }
    }
}
