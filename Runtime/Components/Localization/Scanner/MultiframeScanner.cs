using Google.Protobuf;
using SturfeeVPS.Core;
using SturfeeVPS.Core.Proto;
using SturfeeVPS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public class MultiframeScanner : Scanner, IScanner
    {
        [SerializeField]
        private ScanConfig _scanConfig;
        [SerializeField]
        protected ScannerUI _scannerUI;
        [SerializeField]
        protected string _serviceUrl;

        public bool IsScanning;

        [Header("Internal")]
        [SerializeField][ReadOnly]
        protected uint _requestNum = 0;
        [SerializeField][ReadOnly]
        protected uint _requestId = 0;
        [SerializeField][ReadOnly]
        protected LocalizationService _localizationService;

        protected DateTime _requestTimeStamp;

        public ScanConfig ScanConfig => _scanConfig;
        public override OffsetType OffsetType => ScanConfig.OffsetType;
        public override ScanType ScanType => ScanConfig.ScanType;

        public override async Task Initialize(uint requestNum)
        {
            _requestNum = requestNum;
            _scannerUI.ReadyForScan();
        }

        public override void StartScan()
        {
            IsScanning = true;
                        
            _scannerUI.StartScan(ScanConfig);
            _requestId = (_requestNum * 10) + (int)OperationMessages.Alignment;
            _requestNum++;

            CaptureAndSendAsync();

            TriggerScanStartEvent();
        }

        public override void StopScan()
        {
            if (!IsScanning)
            {
                _localizationService?.Disconnect();
            }

            IsScanning = false;
            _scannerUI.StopScan();

            TriggerScanStopEvent();
        }

        protected virtual async void CaptureAndSendAsync()
        {
            float startYaw = Camera.transform.eulerAngles.y;
            int frameOrder = 0;
            int numOfFrames = ScanConfig.TargetCount;
            float currentTargetYaw = 0;
            int diff = ScanType == ScanType.HD ? 1 : 3;
            _requestId = (_requestNum * 10) + (int)OperationMessages.Alignment;

            await Task.Yield();

            // Wait till all the frames are captured 
            while (frameOrder < numOfFrames && IsScanning)
            {
                float currentYaw = Camera.transform.eulerAngles.y;
                float yawDiff = GetYawDiff(currentYaw, startYaw);
                //Debug.Log($"Scanning => start yaw {startYaw}, frameorder : {frameOrder}, numOfFrames : {numOfFrames}, currentTargetYaw : {currentTargetYaw}, currentYaw : {currentYaw}, yawDiff : {yawDiff}");

                if (currentTargetYaw - yawDiff < diff && currentTargetYaw - yawDiff > -diff)
                {
                    if (CannotCapture())
                    {
                        // Wait till device is adjusted to come out of error
                        await Task.Yield();
                    }
                    else
                    {                        
                        var request = Capture((uint)frameOrder, (uint)numOfFrames);
                        Send(request);

                        frameOrder++;
                        currentTargetYaw = frameOrder * ScanConfig.YawAngle;
                    }
                }

                await Task.Yield();
            }

            // Loading is skipped if server responds before all requests are sent
            if (frameOrder >= numOfFrames)
            {
                _scannerUI.ScanLoading();

                TriggerScanLoadingEvent();
            }
        }

        protected virtual Request Capture(uint frameOrder, uint numOfFrames)
        {
            _scannerUI.Capture();

            var request = BuildRequest(frameOrder, numOfFrames);
            var localizationRequest = LocalizationRequest.ParseProtobufRequest(request);
            TriggerScanCaptureEvent(localizationRequest);

            return request;
        }

        protected virtual void Send(Request request)
        {
            if(_localizationService == null)
            {
                throw new Exception($"[MultiframeScanner] ::Cannot send reuest. LocalizationService is NULL");
            }
            
            byte[] buffer = request.ToByteArray();
            _localizationService.Send(buffer, (success) =>
            {
                if (success)
                {
                    SturfeeDebug.Log($"[MultiframeScanner] ::Localiation request sent successfully...");
                    if (request.FrameOrder >= request.TotalNumOfFrames)
                    {
                        _requestTimeStamp = DateTime.Now;
                    }
                }
                else
                {
                    SturfeeDebug.LogError($" [MultiframeScanner] ::Localiation request errored out while sending..");
                }
            });            
        }

        protected virtual void OnResponse(ResponseMessage responseMessage)
        {
            _scannerUI.ScanComplete();

            LocalizationResponseMessage localizationResponseMessage = LocalizationResponseMessage.ParseProtobufResponseMessage(responseMessage);
            TriggerScanCompleteEvent(localizationResponseMessage);
        }

        protected virtual Request BuildRequest(uint frameOrder, uint numOfFrames)
        {
            var xrSession = XrSessionManager.GetSession();

            Request request = new Request
            {
                Operation = OperationMessages.Alignment,
                RequestId = _requestId,
                ExternalParameters = new Core.Proto.ExternalParameters
                {
                    Position = new Position
                    {
                        Lat = xrSession.Location.Latitude,
                        Lon = xrSession.Location.Longitude,
                        Height = xrSession.Location.Altitude
                    },
                    Quaternion = new Core.Proto.Quaternion
                    {
                        X = xrSession.GetProvider<IPoseProvider>().GetRotation().x,
                        Y = xrSession.GetProvider<IPoseProvider>().GetRotation().y,
                        Z = xrSession.GetProvider<IPoseProvider>().GetRotation().z,
                        W = xrSession.GetProvider<IPoseProvider>().GetRotation().w
                    }
                },
                InternalParameters = new Core.Proto.InternalParameters
                {
                    SceneHeight = (uint)xrSession.GetProvider<IVideoProvider>().GetHeight(),
                    SceneWidth = (uint)xrSession.GetProvider<IVideoProvider>().GetWidth(),
                    Fov = xrSession.GetProvider<IVideoProvider>().GetFOV(),
                    ProjectionMatrix = {
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m00,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m01,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m02,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m03,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m10,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m11,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m12,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m13,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m20,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m21,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m22,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m23,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m30,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m31,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m32,
                        xrSession.GetProvider<IVideoProvider>().GetProjectionMatrix().m33,
                    }
                },
                DevRadius = 30,
                FrameOrder = frameOrder,
                TotalNumOfFrames = numOfFrames,

                // Image
                SourceImage = ByteString.CopyFrom(
                   xrSession.GetProvider<IVideoProvider>().GetCurrentFrame().EncodeToJPG()),
            };

            return request;
        }

        protected virtual LocalizationService CreateLocalizationService()
        {
            if (_localizationService != null)
            {
                _localizationService.Disconnect();
                _localizationService = null;
            }

            _localizationService = new LocalizationService();
            _localizationService.OnSocketOpen += OnSocketOpen;
            _localizationService.OnSocketReceive += OnSocketRecieve;
            _localizationService.OnSocketError += OnSocketError;
            _localizationService.OnSocketClose += OnSocketClose;

            return _localizationService;
        }

        protected virtual async Task WaitForSessionProviders()
        {
            SturfeeDebug.Log($" Waiting for session providers...");
            await Task.WhenAll(
                    WaitForProvider<IPoseProvider>(),
                    WaitForProvider<IVideoProvider>(),
                    WaitForProvider<IGpsProvider>()
            );            
        }

        protected virtual async Task<bool> CheckCoverage(GeoLocation location)
        {
            try
            {
                await VpsServices.CheckCoverage(location, TokenUtils.GetVpsToken());
                return true;
            }
            catch (Exception e)
            {
                if (e is HttpException)
                {
                    var ex = (HttpException)e;
                    if (ex.ErrorCode == 501)
                    {
                        return false;
                    }

                    throw ex.ErrorCode switch
                    {

                        400 => new IdException(ErrorMessages.Error400),
                        403 => new IdException(ErrorMessages.Error403),
                        500 => new IdException(ErrorMessages.Error500),
                        _ => new IdException(ErrorMessages.HttpErrorGeneric),
                    };
                }

                throw new IdException(ErrorMessages.HttpErrorGeneric);
            }
        }

        protected virtual async Task ValidateToken(string token)
        {
            try
            {
                await VpsServices.ValidateToken(token);
            }
            catch (Exception e)
            {
                if (e is HttpException)
                {
                    var ex = (HttpException)e;
                    throw ex.ErrorCode switch
                    {
                        501 => new IdException(ErrorMessages.NoCoverageArea),
                        400 => new IdException(ErrorMessages.Error400),
                        403 => new IdException(ErrorMessages.Error403),
                        500 => new IdException(ErrorMessages.Error500),
                        _ => new IdException(ErrorMessages.HttpErrorGeneric),
                    };
                }

                throw new IdException(ErrorMessages.HttpErrorGeneric);
            }

        }

        protected virtual async Task WaitForProvider<T>() where T : IProvider
        {
            var xrsession = XrSessionManager.GetSession();
            var provider = xrsession.GetProvider<T>();
            if (provider == null)
            {
                throw new Exception($" Cannot Scan. {typeof(T).Name} not registered to session.");
            }
            switch (provider.GetProviderStatus())
            {
                case ProviderStatus.NotSupported:
                    throw new Exception($" Cannot Scan. {provider.GetType().Name} is not supported.");
                case ProviderStatus.Stopped:
                    // wait till timeout 
                    break;
                case ProviderStatus.Initializing:
                    // wait till timeout 
                    break;
                case ProviderStatus.Ready:
                    return;
            }

            float start = Time.time;
            while (true)
            {
                if (provider.GetProviderStatus() == ProviderStatus.Ready)
                {
                    SturfeeDebug.Log($"{provider.GetType().Name} ready");
                    return;
                }

                if (Time.time - start > 10)
                {
                    throw new Exception($"{provider.GetType().Name} timed out");
                }

                SturfeeDebug.Log($" Waiting 1 second for {provider.GetType().Name}. Current status : {provider.GetProviderStatus()}");
                await Task.Delay(1000);
            }
        }

        protected virtual float GetYawDiff(float yaw1, float yaw2)
        {            
            float yawDiff = yaw1 - yaw2;
            float absYawDiff = Mathf.Abs(yawDiff);

            if (absYawDiff > 180)
            {
                yawDiff = yawDiff > 0 ? -(360 - absYawDiff) : 360 - absYawDiff;
            }

            //If our capture range goes above 180
            float captureRange = (ScanConfig.TargetCount - 1) * ScanConfig.YawAngle;
            if (yawDiff < 0 && captureRange > 180)
            {
                if (yawDiff > -180 && yawDiff <= captureRange - 360 + 5)    // + 5 is added for sanity just in case we want cursorPos beyond last gaze target
                {
                    yawDiff += 360;
                }
            }

            return yawDiff;
        }

        protected virtual bool CannotCapture()
        {
            var request = Camera.transform.eulerAngles;

            float pitch = request.x;
            float roll = request.z;

            if (pitch > 180)
            {
                pitch -= 360;
            }

            //Debug.Log($" pitch : {pitch}");
            if (pitch < ScanConfig.PitchMin || pitch > ScanConfig.PitchMax)
            {
                return true;
            }

            if (roll > 180)
            {
                roll -= 360;
            }

            roll = -roll;
            //Debug.Log($" roll : {roll}");
            if (roll < ScanConfig.RollMin || roll > ScanConfig.RollMax)
            {
                return true;
            }

            return false;
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

        #region Socket Events
        protected void OnSocketOpen()
        {

        }

        protected void OnSocketRecieve()
        {
            SturfeeDebug.Log($"[MultiframeScanner] :: Socket response recieved");
            IsScanning = false;

            var responseTine = DateTime.Now;
            var latency = (responseTine - _requestTimeStamp).Milliseconds;
            SturfeeDebug.Log($"latency : {latency} ms");

            byte[] data =_localizationService.Recv();
            ResponseMessage response = ResponseMessage.Parser.ParseFrom(data);
            OnResponse(response);

            _localizationService?.Disconnect();
        }

        protected void OnSocketError(string error)
        {
            IsScanning = false;
            string localizedError = SturfeeLocalizationProvider.Instance.GetString(ErrorMessages.SocketConnectionFail.Item1, error);
            TriggerScanErrorEvent(localizedError);
        }

        protected void OnSocketClose(string reason)
        {
            IsScanning = false;

            string localizedError = SturfeeLocalizationProvider.Instance.GetString(ErrorMessages.SocketConnectionFail.Item1, reason);
            TriggerScanErrorEvent(localizedError);
            //TriggerScanErrorEvent(reason);
        }
        #endregion

    }
}