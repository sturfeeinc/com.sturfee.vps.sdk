using UnityEngine;
using System.Collections;
using SturfeeVPS.Core;
using System.Threading.Tasks;
using SturfeeVPS.UI;

namespace SturfeeVPS.SDK
{
    public class SturfeeXRSession : MonoBehaviour
    {
        public XRSessionStatus Status;
        public GpsProviderBase GpsProvider;
        public PoseProviderBase PoseProvider;
        public VideoProviderBase VideoProvider;

        [HideInInspector]
        public int SelectedProvider;

        private void Start()
        {
            SturfeeThemeProvider.Instance.ApplyTheme();
            PlayerPrefs.DeleteKey("SturfeeVPS.Core.CustomApi.Api");
            PlayerPrefs.DeleteKey("SturfeeVPS.Core.CustomApi.Websocket");
            XRSessionConfig config = new XRSessionConfig
            {
                GpsProvider = GpsProvider,
                PoseProvider = PoseProvider,
                VideoProvider = VideoProvider
            };

            XRSessionManager.CreateSession(config);            
        }

        private void Update()
        {
            Status = XRSessionManager.GetSession().Status;

        }

        private void OnDestroy()
        {
            XRSessionManager.DestroySession();
        }
    }
}
