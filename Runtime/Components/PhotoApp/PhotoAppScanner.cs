using SturfeeVPS.Core;
using SturfeeVPS.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;

using Newtonsoft.Json;

namespace SturfeeVPS.SDK
{
    public class PhotoAppScanner : MonoBehaviour
    {
        protected DateTime _requestTimeStamp;
        VPSData vpsData = null;

        [SerializeField] private GameObject PhotoScanButton;
        [SerializeField] private GameObject ExitButton;
        [SerializeField] private VpsButton VpsButton = null;

        async void Awake()
        {
            PhotoScanButton.SetActive(false);
            ExitButton.SetActive(false);
            SturfeeEventManager.OnLocalizationSuccessful += OnLocalizationSuccessful;
            SturfeeEventManager.OnLocalizationDisabled += OnLocalizationDisabled;

            while (VpsButton == null)
            {
                Debug.Log("[PhotoAppScanner.cs] "+VpsButton);
                try
                {
                    VpsButton = FindObjectOfType<VpsButton>().GetComponent<VpsButton>();
                }
                catch {}
                
                await Task.Yield();
            }
        }


        public void CaptureAndStore()
        {
            Capture();
            Save();
        }

        public void Exit()
        {
            // var localizationProvider = IOC.Resolve<ILocalizationProvider>();
            // if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            // {
            //     localizationProvider.DisableLocalization();
            // }
            VpsButton._HandleClick();
        }

        public void OnLocalizationSuccessful()
        {
            PhotoScanButton.SetActive(true);
            ExitButton.SetActive(true);
        }
        public void OnLocalizationDisabled()
        {
            PhotoScanButton.SetActive(false);
            ExitButton.SetActive(false);
        }

        protected virtual void Save()
        {
            var localizationProvider = IOC.Resolve<ILocalizationProvider>();
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                var traceID = localizationProvider.trackingID;
                var fileName = Path.Combine(Application.persistentDataPath, traceID, traceID+".json");
                using (var streamWriter = new StreamWriter(fileName))
                {
                    string json = JsonConvert.SerializeObject(vpsData); //, new JsonSerializerSettings
                    // {
                    //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    // });
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
            }
        }

        protected virtual VPSData Capture()
        {            
            var xrSession = XrSessionManager.GetSession();

            var localizationProvider = IOC.Resolve<ILocalizationProvider>();
            var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
            if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
            {
                if(poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    var traceID = localizationProvider.trackingID;
                    InitVPSData(Path.Combine(Application.persistentDataPath, traceID, traceID+".json"));

                    Debug.Log(xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m00.GetType());

                    // add new frame
                    vpsData.frames.Add(new frame
                    {
                        frameId = vpsData.frames.Count,
                        frameName = $"image_{vpsData.frames.Count.ToString("D5")}.jpg",
                        timeStamp = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"),
                        frameIntrinsic = new VPSMatrix4x4
                        {
                            m00 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m00,
                            m01 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m01,
                            m02 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m02,
                            m03 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m03,
                            m10 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m10,
                            m11 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m11,
                            m12 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m12,
                            m13 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m13,
                            m20 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m20,
                            m21 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m21,
                            m22 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m22,
                            m23 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m23,
                            m30 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m30,
                            m31 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m31,
                            m32 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m32,
                            m33 = xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m33,
                        },
                        worldPose = new WorldPoseContainer
                        {
                            locationUTM = GeoCoordinateConverter.GpsToUtm(Converters.UnityToGeoLocation(Camera.transform.position)),
                            mappedOrientation = new VPSQuaternion(Converters.UnityToWorldRotation(Camera.transform.rotation))
                        },
                        arfPose = new VPSTransform
                        {
                            location = new VPSVector3(poseProvider.GetPosition(out _)),
                            orientation = new VPSQuaternion(poseProvider.GetRotation())
                        }

                    });

                    byte[] SourceImage = xrSession.GetProvider<IVideoProvider>().GetCurrentFrame().EncodeToJPG();
                    string _file = Path.Combine(Application.persistentDataPath, traceID, $"image_{vpsData.frames.Count.ToString("D5")}.jpg");
                    // using (var streamWriter = new StreamWriter(_file, false))
                    // {
                    //     streamWriter.Write(SourceImage);
                    //     streamWriter.Flush();
                    // }

                    using (var stream = File.Open(_file, FileMode.Create))
                    {
                        using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                        {
                            writer.Write(SourceImage);
                        }
                    }
                }

                return vpsData;
            }

            return null;
        }

        protected virtual void InitVPSData(string fileName)
        {
            if (vpsData == null)
            {
                if (File.Exists(fileName))
                {
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        string json = reader.ReadToEnd();

                        if (!string.IsNullOrEmpty(json))
                        {
                            vpsData = JsonConvert.DeserializeObject<VPSData>(json);//, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
                            return;
                        }
                    }
                }
                
                var xrSession = XrSessionManager.GetSession();

                var localizationProvider = IOC.Resolve<ILocalizationProvider>();
                var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
                
                if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    if(poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
                    {
                        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, localizationProvider.trackingID))) 
                            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, localizationProvider.trackingID));

                        vpsData = new VPSData
                        {
                            trackingId = localizationProvider.trackingID,
                            centerX = PositioningUtils.GetReferenceUTM.X,
                            centerY = PositioningUtils.GetReferenceUTM.Y,
                            arfShift = new VPSTransform
                            {
                                location = new VPSVector3(xrSession.Shift),
                                orientation = new VPSQuaternion(xrSession.ShiftRotation)
                            },
                            frames = new List<frame>()
                        };
                    }
                }

                
                using (var streamWriter = new StreamWriter(fileName, false))
                {
                    string json = JsonConvert.SerializeObject(vpsData); //, new JsonSerializerSettings
                    // {
                    //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    // });
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
            }
        }

        protected virtual Camera Camera
        {
            get
            {
                if (XrCamera.Camera != null && XrSessionManager.GetSession() != null)
                {
                    return XrCamera.Camera;
                }
                return Camera.main;
            }
        }

    }
}
