using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public enum ScannerState
    {
        ReadyToScan,
        Scanning,
        Loading,
        Complete
    }

    public class ScannerUI : MonoBehaviour
    {        
        [SerializeField]
        protected CaptureUI _caprtureUI;
        [SerializeField]
        protected Button _scanButton;
        [SerializeField]
        protected UnityEvent _onReadyForScan;
        [SerializeField]
        protected UnityEvent _onScanStart;
        [SerializeField]
        protected UnityEvent _onScanStop;
        [SerializeField]
        protected UnityEvent _onScanLoading;
        [SerializeField]
        protected UnityEvent _onScanComplete;

        private bool _isScanning;

        public void ReadyForScan()
        {
            _onReadyForScan?.Invoke();
        }

        public void StartScan(ScanConfig scanConfig)
        {
            _caprtureUI.gameObject.SetActive(true);
            _caprtureUI.StartScan(scanConfig.YawAngle, scanConfig.TargetCount);
            _onScanStart?.Invoke();

            _isScanning = true;
        }

        public void Capture()
        {
            _caprtureUI.Capture();
        }

        public void StopScan()
        {
            if (_isScanning)
            {
                _onReadyForScan.Invoke();
            }
            else
            {
                _onScanStop.Invoke();
            }

            _caprtureUI.StopScan();
            _isScanning = false;
        }

        public void ScanLoading()
        {
            _onScanLoading?.Invoke();
            _isScanning = false;
        }

        public void ScanComplete()
        {
            _caprtureUI.StopScan();
            _onScanComplete?.Invoke();
            _isScanning = false;
            _caprtureUI.gameObject.SetActive(false);
        }
    }
}
