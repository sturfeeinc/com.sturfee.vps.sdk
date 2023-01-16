// using Google.Protobuf;
// using SturfeeVPS.Core;
// using SturfeeVPS.Core.Proto;
// using SturfeeVPS.UI;
// using System;
// using System.IO;
// using System.Collections;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.Networking;
// using UnityEngine.UI;
// using Newtonsoft.Json;

// namespace SturfeeVPS.SDK
// {
//     public class PhotoAppScannerOriginal : Scanner, IScanner
//     {
//         [SerializeField] private ScanConfig _scanConfig;
//         [SerializeField] protected ScannerUI _scannerUI;

//         public bool IsScanning;

//         protected DateTime _requestTimeStamp;
        
//         public ScanConfig ScanConfig => _scanConfig;
//         public override OffsetType OffsetType => ScanConfig.OffsetType;
//         public override ScanType ScanType => ScanConfig.ScanType;

//         VPSData vpsData = null;

//         public override void StartScan()
//         {
//             IsScanning = true;
//             _scannerUI.StartScan(ScanConfig);

//             CaptureAndStoreAsync();
//             TriggerScanStartEvent();
//         }

//         public override void StopScan()
//         {
//             IsScanning = false;
//             _scannerUI.StopScan();

//             TriggerScanStopEvent();
//         }

//         protected virtual async void CaptureAndStoreAsync()
//         {
//             float startYaw = Camera.transform.eulerAngles.y;
//             int frameOrder = 0;
//             int numOfFrames = ScanConfig.TargetCount;
//             float currentTargetYaw = 0;
//             int diff = 1;

//             while (frameOrder < numOfFrames && IsScanning)
//             {
//                 float currentYaw = Camera.transform.eulerAngles.y;
//                 float yawDiff = GetYawDiff(currentYaw, startYaw);

//                 if (currentTargetYaw - yawDiff < diff && currentTargetYaw - yawDiff > -diff)
//                 {
//                     if (!CannotCapture())
//                     {
//                         // SaveCapture(Capture());
//                         frameOrder++;
//                         currentTargetYaw = frameOrder * ScanConfig.YawAngle;
//                     }
//                 }

//                 await Task.Yield();
//             }

//             if (frameOrder >= numOfFrames)
//             {
//                 // do something at the end
//             }
//         }

//         protected virtual VPSData Capture(uint frameOrder, uint numOfFrames)
//         {
//             _scannerUI.Capture();
            
//             var xrSession = XrSessionManager.GetSession();

//             var localizationProvider = IOC.Resolve<ILocalizationProvider>();
//             if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
//             {
//                 var traceID = localizationProvider.trackingID;
//                 InitVPSData(Path.Combine(Application.persistentDataPath, traceID, traceID+".json"));

//                 // add new frame

//                 return vpsData;
//             }

//             return null;
//         }

//         protected virtual void InitVPSData(string fileName)
//         {
//             if (vpsData == null)
//             {
//                 if (File.Exists(fileName))
//                 {
//                     using (StreamReader reader = new StreamReader(fileName))
//                     {
//                         string jsonResponse = reader.ReadToEnd();

//                         if (!string.IsNullOrEmpty(jsonResponse))
//                         {
//                             vpsData = JsonConvert.DeserializeObject<VPSData>(jsonResponse);
//                             return;
//                         }
//                     }
//                 }
                
//                 var localizationProvider = IOC.Resolve<ILocalizationProvider>();
//                 var poseProvider = XrSessionManager.GetSession().GetProvider<IPoseProvider>();
                
//                 if (localizationProvider != null && localizationProvider.GetProviderStatus() == ProviderStatus.Ready)
//                 {
//                     if(poseProvider != null && poseProvider.GetProviderStatus() == ProviderStatus.Ready)
//                     {
//                         vpsData = new VPSData
//                         {
//                             trackingId = localizationProvider.trackingID,
//                             centerX = PositioningUtils.GetReferenceUTM.X,
//                             centerY = PositioningUtils.GetReferenceUTM.Y,
//                             arfShift = new VPSTransform
//                             {
//                                 location = poseProvider.GetPosition(out _),
//                                 orientation = ToWXYZ(poseProvider.GetRotation())
//                             },
//                             frames = new List<frame>()
//                         };
//                     }
//                 }

//                 using (var streamWriter = new StreamWriter(fileName))
//                 {
//                     string json = JsonConvert.SerializeObject(vpsData);
//                     streamWriter.Write(json);
//                     streamWriter.Flush();
//                 }
//             }
//         }

//         protected virtual Vector4 ToWXYZ(UnityEngine.Quaternion rotation)
//         {
//             return new Vector4(rotation.w, rotation.x, rotation.y, rotation.z);
//         }

//         protected virtual float GetYawDiff(float yaw1, float yaw2)
//         {            
//             float yawDiff = yaw1 - yaw2;
//             float absYawDiff = Mathf.Abs(yawDiff);

//             if (absYawDiff > 180)
//             {
//                 yawDiff = yawDiff > 0 ? -(360 - absYawDiff) : 360 - absYawDiff;
//             }

//             //If our capture range goes above 180
//             float captureRange = (ScanConfig.TargetCount - 1) * ScanConfig.YawAngle;
//             if (yawDiff < 0 && captureRange > 180)
//             {
//                 if (yawDiff > -180 && yawDiff <= captureRange - 360 + 5)    // + 5 is added for sanity just in case we want cursorPos beyond last gaze target
//                 {
//                     yawDiff += 360;
//                 }
//             }

//             return yawDiff;
//         }

//         protected virtual bool CannotCapture()
//         {
//             var request = Camera.transform.eulerAngles;

//             float pitch = request.x;
//             float roll = request.z;

//             if (pitch > 180)
//             {
//                 pitch -= 360;
//             }

//             //Debug.Log($" pitch : {pitch}");
//             if (pitch < ScanConfig.PitchMin || pitch > ScanConfig.PitchMax)
//             {
//                 return true;
//             }

//             if (roll > 180)
//             {
//                 roll -= 360;
//             }

//             roll = -roll;
//             //Debug.Log($" roll : {roll}");
//             if (roll < ScanConfig.RollMin || roll > ScanConfig.RollMax)
//             {
//                 return true;
//             }

//             return false;
//         }

//         protected virtual Camera Camera
//         {
//             get
//             {
//                 if (XrCamera.Camera != null && XrSessionManager.GetSession() != null)
//                 {
//                     return XrCamera.Camera;
//                 }
//                 return Camera.main;
//             }
//         }
//     }
// }
