using SturfeeVPS.Core;
using SturfeeVPS.SDK.Providers;
using SturfeeVPS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SturfeeXrSession : MonoBehaviour
    {
        public GeoLocation Location;
        public bool CreateOnStart;
        public int StartSet;        
        public ProviderSet[] ProviderSets;

        [SerializeField][ReadOnly]
        private int _currentSet;


        private void Start()
        {
            SturfeeThemeProvider.Instance.ApplyTheme();

            if (CreateOnStart)
            {
                CreateSession();
            }
        }

        private void OnDestroy()
        {
            XrSessionManager.DestroySession();
        }

        public async void CreateSession()
        {
            _currentSet = StartSet;

            if(Location.Latitude == 0 || Location.Longitude == 0)
            {
#if UNITY_EDITOR
                Location = EditorUtils.EditorFallbackLocation;
#else
                // FOR DEBUG
                SturfeeDebug.Log($"[SturfeeXrSession.cs] Latitude: {Location.Latitude}, Longitude: {Location.Longitude}");
                await InitLocation();
                Location = new GeoLocation
                {
                    Latitude = Input.location.lastData.latitude,
                    Longitude = Input.location.lastData.longitude
                };    
#endif
            }

            if (Location.Latitude == 0 || Location.Longitude == 0)
            {
                SturfeeDebug.LogError($" Cannot create session. Location is 0,0");
                return;
            }

            XrSessionManager.CreateSession(Location);
            RegisterProviders();
        }   

        public void SwitchProviderSet(int index)
        {
            if (_currentSet == index)
                return;

            UnRegisterProviders();

            _currentSet = index;

            RegisterProviders();
        }

        public void RegisterProviders()
        {
            var providerSet = ProviderSets[_currentSet];

            // FOR DEBUG
            // Debug.Log($"[SturfeeXrSession.cs] Current Set: {_currentSet}");

            RegisterProvider<IGpsProvider>(providerSet.Providers.GpsProvider);
            RegisterProvider<IPoseProvider>(providerSet.Providers.PoseProvider);
            RegisterProvider<IVideoProvider>(providerSet.Providers.VideoProvider);
            RegisterProvider<ITilesProvider>(providerSet.Providers.TilesProvider);
            RegisterProvider<ILocalizationProvider>(providerSet.Providers.LocalizationProvider);
        }

        public void UnRegisterProviders()
        {
            // Gps
            if (XrSessionManager.GetSession().GetProvider<IGpsProvider>() != null)
            {
                XrSessionManager.GetSession().UnregisterProvider<IGpsProvider>();
            }
            // Pose
            if (XrSessionManager.GetSession().GetProvider<IPoseProvider>() != null)
            {
                XrSessionManager.GetSession().UnregisterProvider<IPoseProvider>();
            }
            // Video
            if (XrSessionManager.GetSession().GetProvider<IVideoProvider>() != null)
            {
                XrSessionManager.GetSession().UnregisterProvider<IVideoProvider>();
            }
            // Tiles
            if (XrSessionManager.GetSession().GetProvider<ITilesProvider>() != null)
            {
                XrSessionManager.GetSession().UnregisterProvider<ITilesProvider>();
            }
            // Localization
            if (XrSessionManager.GetSession().GetProvider<ILocalizationProvider>() != null)
            {
                XrSessionManager.GetSession().UnregisterProvider<ILocalizationProvider>();
            }
        }

        private void RegisterProvider<T>(T provider) where T: IProvider
        {
            if (provider == null)
            {
                Debug.Log($"No {typeof(T).Name} to register for {ProviderSets[_currentSet].Name}");
                return;
            }

            switch (provider)
            {
                case BaseGpsProvider gps:
                    XrSessionManager.GetSession().RegisterProvider<IGpsProvider>(ReplacePrefab(gps));
                    break;
                case BasePoseProvider pose:
                    XrSessionManager.GetSession().RegisterProvider<IPoseProvider>(ReplacePrefab(pose));
                    break;
                case BaseVideoProvider video:
                    XrSessionManager.GetSession().RegisterProvider<IVideoProvider>(ReplacePrefab(video));
                    break;
                case BaseTilesProvider tiles:
                    XrSessionManager.GetSession().RegisterProvider<ITilesProvider>(ReplacePrefab(tiles));
                    break;
                case BaseLocalizationProvider localization:
                    XrSessionManager.GetSession().RegisterProvider<ILocalizationProvider>(ReplacePrefab(localization));
                    break;
                default:
                    Debug.Log($"No {typeof(T).Name} to register for {ProviderSets[_currentSet].Name}");
                    break;
            }
        }

        private T ReplacePrefab<T>(T provider) where T: BaseProvider
        {
            var providerSet = ProviderSets[_currentSet];
            var instance = provider;
            if (provider.gameObject.scene.name == null)     // It's a prefab
            {
                SturfeeDebug.Log($"[SturfeeXrSession] :: Instantiating {provider.name} of \"{ProviderSets[_currentSet].Name}\" provider set");
                instance = Instantiate(provider, transform);
                providerSet.Providers.ReplacePrefabWithInstance(instance);
            }
            return instance;
        }

        private async Task InitLocation()
        {
            SturfeeNativeGps.RequestLocationUpdates();

            // Check if the user has location service enabled.
            if (!Input.location.isEnabledByUser)
                return;

            // Starts the location service.
            Input.location.Start();

            // Waits until the location service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                await Task.Delay(1000);
                maxWait--;
            }

            // If the service didn't initialize in 20 seconds this cancels location service use.
            if (maxWait < 1)
            {
                print("Timed out");
                return;
            }

            // If the connection failed this cancels location service use.
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                print("Unable to determine device location");
                return;
            }
            else
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            }

            // Stops the location service if there is no need to query location updates continuously.
            //Input.location.Stop();
        }

    }
}
