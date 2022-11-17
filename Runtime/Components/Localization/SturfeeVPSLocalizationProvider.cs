using Google.Protobuf;
using Newtonsoft.Json;
using SturfeeVPS.Core;
using SturfeeVPS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SturfeeVPSLocalizationProvider : BaseLocalizationProvider
    {
        [SerializeField]
        private ScanSelector _scanSelector;

        [Header("Internal")]
        [SerializeField][ReadOnly]
        protected Scanner _scanner;
        [SerializeField][ReadOnly]
        protected GeoLocation _vpsLocation;
        [SerializeField][ReadOnly]
        protected uint _requestNum = 1;
        [SerializeField][ReadOnly]
        protected ProviderStatus _providerStatus;
        
        #region overrides

        public override OffsetType OffsetType { protected set { } get { return _scanner.OffsetType; } }
        public override Quaternion YawOffset { protected set; get; }
        public override Quaternion PitchOffset { protected set; get; }
        public override Quaternion RollOffset { protected set; get; }
        public override Vector3 EulerOffset { protected set; get; }        

        public override void EnableLocalization()
        {
            _providerStatus = ProviderStatus.Initializing;
            TriggerLocalizationRequestedEvent();

            _scanSelector.SelectScanner(OnScannerSelected);                
        }

        public override void StopLocalization()
        {
            _providerStatus = ProviderStatus.Stopped;
            Scanner.StopScan();
        }

        public override void DisableLocalization()
        {
            _providerStatus = ProviderStatus.Stopped;
            TriggerLocalizationDisabledEvent();
        }

        public override GeoLocation GetVpsLocation(out bool includesElevation)
        {
            includesElevation = true;
            return _vpsLocation;
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }

        #endregion

        #region Scanner

        public Scanner Scanner => _scanner;

        public void SetScanner(Scanner scanner)
        {
            if(_scanner != null)
            {
                _scanner.OnScanStart -= OnScanStart;
                _scanner.OnScanCapture -= OnScanCapture;
                _scanner.OnScanLoading -= OnScanLoading;
                _scanner.OnScanStop -= OnScanStop;
                _scanner.OnScanError -= OnScanError;
                _scanner.OnScanComplete -= OnScanComplete;

                _scanner = null;
            }

            _scanner = scanner;
            _scanner.OnScanStart += OnScanStart;
            _scanner.OnScanCapture += OnScanCapture;
            _scanner.OnScanLoading += OnScanLoading;
            _scanner.OnScanStop += OnScanStop;
            _scanner.OnScanError += OnScanError;
            _scanner.OnScanComplete += OnScanComplete;
        }

        private async void OnScannerSelected(Scanner scanner)
        {
            SetScanner(scanner);
            try
            {
                await _scanner.Initialize(_requestNum);
                VpsButton.CurrentInstance.SetState(VpsScanState.ReadyToScan);
            }
            catch(Exception e)
            {
                VpsButton.CurrentInstance.SetState(VpsScanState.Off);

                if(e is IdException ex)
                {
                    string localizedError = SturfeeLocalizationProvider.Instance.GetString($"{ex.Id}", ex.Message);
                    TriggerLocalizationFailEvent(localizedError);
                    return;
                }
                TriggerLocalizationFailEvent(e.Message);
            }
        }

        private void OnScanStart()
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanStart");

            _requestNum++;
            TriggerLocalizationStartEvent();
        }

        private void OnScanCapture(LocalizationRequest localizationRequest)
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanCapture");

            SturfeeDebug.Log(JsonUtility.ToJson(localizationRequest));
        }

        private void OnScanLoading()
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanLoading");
            TriggerLocalizationLoadingedEvent();
        }

        private void OnScanStop()
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanStop");
            TriggerLocalizationStopEvent();
        }

        private void OnScanError(string error)
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanFail");
            TriggerLocalizationFailEvent(error);
        }
        
        private void OnScanComplete(LocalizationResponseMessage responseMessage)
        {
            SturfeeDebug.Log($"[SturfeeVPSLocalizationProvider] :: OnScanComplete");

            // success
            if (responseMessage.error == null)
            {
                YawOffset = responseMessage.response.yawOrientationCorrection;

                PitchOffset = responseMessage.response.pitchOrientationCorrection;

                EulerOffset = responseMessage.response.eulerOffset; ;
                SturfeeDebug.Log($" Euler offset : {EulerOffset}");                

                _vpsLocation = responseMessage.response.location;
                
                SturfeeDebug.Log($"Localization completed using {_scanner.ScanType} Scanner. Applying {_scanner.OffsetType} offsets");

                SturfeeDebug.Log(JsonUtility.ToJson(responseMessage));

                _providerStatus = ProviderStatus.Ready;

                TriggerLocalizationSuccessfulEvent();
            }
            // fail            
            else
            {
                SturfeeDebug.LogError($"[SturfeeVPSLoclizationProvider] :: Localization failed => {responseMessage.error.message}");
                _providerStatus= ProviderStatus.Stopped;

                string localizedError = SturfeeLocalizationProvider.Instance.GetString($"{responseMessage.error.code}", responseMessage.error.message);
                TriggerLocalizationFailEvent(localizedError);                
            }
            
        }

        #endregion
    }
}
