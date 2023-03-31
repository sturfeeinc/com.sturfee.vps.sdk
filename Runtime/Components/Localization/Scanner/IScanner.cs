using SturfeeVPS.Core.Proto;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Event fired when scan is started.
    /// </summary>
    public delegate void ScanStartEvent();
    public delegate void ScanErrorEvent(string error);
    /// <summary>
    /// Event fired when a single scan is captured.
    /// </summary>
    /// <param name="localizationRequest">Localization request</param>
    public delegate void ScanCaptureEvent(LocalizationRequest localizationRequest);
    public delegate void ScanLoadingEvent();
    /// <summary>
    /// Event fired when scan is prematurely stopped.
    /// </summary>
    public delegate void ScanStopEvent();
    /// <summary>
    /// Event fired when scan is successfully completed.
    /// </summary>
    /// <param name="localizationResponse">Localization response message</param>
    public delegate void ScanCompleteEvent(LocalizationResponseMessage localizationResponse);

    /// <summary>
    /// Scanner interface
    /// </summary>
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

    /// <summary>
    /// Base scanner class
    /// </summary>
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

        /// <summary>
        /// Scanner initialization sequence
        /// </summary>
        public virtual async Task Initialize(uint requestNum)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Scanner start sequence
        /// </summary>
        public virtual void StartScan()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Scanner stop sequence
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
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
