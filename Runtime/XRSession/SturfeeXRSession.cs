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

        public bool PlayOnStart = true;

        [HideInInspector]
        public int SelectedProvider;

        private void Start()
        {
            SturfeeThemeProvider.Instance.ApplyTheme();
            
            if (PlayOnStart)
            {
                CreateSession();
            }
        }

        private void Update()
        {
            Status = XRSessionManager.GetSession().Status;
        }

        public void CreateSession()
        {
            XRSessionConfig config = new XRSessionConfig
            {
                GpsProvider = GpsProvider,
                PoseProvider = PoseProvider,
                VideoProvider = VideoProvider
            };

            XRSessionManager.CreateSession(config);
        }
        private void OnDestroy()
        {
            XRSessionManager.DestroySession();
        }
    }
}
