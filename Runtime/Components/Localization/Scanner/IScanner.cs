using SturfeeVPS.Core.Proto;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public delegate void ScanStartEvent();
    public delegate void ScanErrorEvent(string error);
    public delegate void ScanCaptureEvent(LocalizationRequest localizationRequest);
    public delegate void ScanLoadingEvent();
    public delegate void ScanStopEvent();
    public delegate void ScanCompleteEvent(LocalizationResponseMessage localizationResponse);

    public interface IScanner
    {
        OffsetType OffsetType { get; }
        ScanType ScanType { get; }

        event ScanStartEvent OnScanStart;
        event ScanErrorEvent OnScanError;
        event ScanCaptureEvent OnScanCapture;
        event ScanLoadingEvent OnScanLoading;
        event ScanStopEvent OnScanStop;
        event ScanCompleteEvent OnScanComplete;
    }

    public class Scanner : MonoBehaviour, IScanner
    {
        public virtual OffsetType OffsetType => throw new System.NotImplementedException();

        public virtual ScanType ScanType => throw new System.NotImplementedException();

        public event ScanStartEvent OnScanStart;
        public event ScanCaptureEvent OnScanCapture;
        public event ScanLoadingEvent OnScanLoading;
        public event ScanStopEvent OnScanStop;
        public event ScanCompleteEvent OnScanComplete;
        public event ScanErrorEvent OnScanError;

        public virtual async Task Initialize(uint requestNum)
        {
            throw new System.NotImplementedException();
        }

        public virtual void StartScan()
        {
            throw new System.NotImplementedException();
        }

        public virtual void StopScan()
        {
            throw new System.NotImplementedException();
        }

        protected virtual void TriggerScanStartEvent()
        {
            OnScanStart?.Invoke();
        }

        protected virtual void TriggerScanErrorEvent(string error)
        {
            OnScanError?.Invoke(error);
        }

        protected virtual void TriggerScanStopEvent()
        {
            OnScanStop?.Invoke();
        }

        protected virtual void TriggerScanLoadingEvent()
        {
            OnScanLoading?.Invoke();
        }

        protected virtual void TriggerScanCaptureEvent(LocalizationRequest localizationRequest)
        {
            OnScanCapture?.Invoke(localizationRequest);
        }

        protected virtual void TriggerScanCompleteEvent(LocalizationResponseMessage localizationResponse)
        {
            OnScanComplete?.Invoke(localizationResponse);
        }
    }
}
