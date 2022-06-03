using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SturfeeVPS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SturfeeVPS.SDK.Localization
{
    public enum VpsScanState
    {
        Off,
        Initializing,
        ReadyToScan,
        Scanning,
        Loading,
        ScanComplete
    }

    [Serializable]
    public class VpsButtonState
    {
        public VpsScanState State;
        public Sprite Icon;
        public Color color;
        public string Message;
        public bool ShowLoader;
        public UnityEvent Event;
    }

    public class ScanController : MonoBehaviour
    {
        public delegate void VpsButtonStateDelegate(VpsScanState state);
        public event VpsButtonStateDelegate OnVpsButtonScanStateChanged;

        public ScanType ScanType;

        [Header("VPS Button")]
        [SerializeField]
        private Button _button;
        [SerializeField]
        private Image _buttonIcon;
        [SerializeField]
        private RectTransform _message;
        [SerializeField]
        private TextMeshProUGUI _messageText;
        [SerializeField]
        private GameObject _loader;
        [SerializeField]
        private List<VpsButtonState> _states = new List<VpsButtonState>
        {
            // TODO : Set Default states            
            new VpsButtonState()
            {
                State = VpsScanState.Off,
                color = Color.white
            },
            new VpsButtonState()
            {
                State = VpsScanState.Initializing,
                Message = "Initializing...",
                color = new Color(0.9098039f, 0.3647059f, 0.4588235f)
            },
            new VpsButtonState()
            {
                State = VpsScanState.ReadyToScan,
                Message = "Ready to Scan",
                color = new Color(0.9098039f, 0.3647059f, 0.4588235f)
            },
            new VpsButtonState()
            {
                State = VpsScanState.Scanning,
                Message = "Scanning...",
                color = new Color(0.9098039f, 0.3647059f, 0.4588235f)
            },
            new VpsButtonState()
            {
                State = VpsScanState.ScanComplete,
                color = Color.white
            }
        };

        private VpsScanState _currentState;

        protected virtual void Start()
        {
            _button.onClick.AddListener(HandleClick);
            SetState(VpsScanState.Off);
            SturfeeEventManager.Instance.OnReadyForScan += OnReadyToscan;
            SturfeeEventManager.Instance.OnLocalizationLoading += OnLocalizationLoading;
            SturfeeEventManager.Instance.OnLocalizationSuccessful += OnLocalizationSuccessful;
            SturfeeEventManager.Instance.OnLocalizationFail += OnLocalizationFail;
        }

        protected virtual void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public virtual void OnScanOff()
        {
            SetState(VpsScanState.Off);
        }

        public virtual void OnScanInitializing()
        {
            SetState(VpsScanState.Initializing);
        }

        public virtual void OnReadyToscan()
        {
            SetState(VpsScanState.ReadyToScan);            
        }

        public virtual void StartScan()
        {
            XRSessionManager.GetSession().PerformLocalization(ScanType);
            SetState(VpsScanState.Scanning);
        }

        public virtual void OnLocalizationLoading()
        {
            SetState(VpsScanState.Loading);
        }

        public virtual void OnLocalizationSuccessful()
        {
            SetState(VpsScanState.ScanComplete);
        }

        public virtual void OnLocalizationFail(string error, string id)
        {
            SetState(VpsScanState.Off);   
        }


        private void HandleClick()
        {
            switch (_currentState)
            {
                case VpsScanState.Off:
                    XRSessionManager.GetSession().EnableVPS();
                    OnScanInitializing();
                    break;
                case VpsScanState.Initializing:                    
                    XRSessionManager.GetSession().CancelVPS();
                    OnScanOff();
                    break;
                case VpsScanState.ReadyToScan:
                    OnScanOff();
                    break;
                case VpsScanState.Scanning:
                    XRSessionManager.GetSession().CancelVPS();
                    OnReadyToscan();
                    break;
                case VpsScanState.Loading:
                    XRSessionManager.GetSession().CancelVPS();
                    OnReadyToscan();
                    break;
                case VpsScanState.ScanComplete:
                    XRSessionManager.GetSession().DisableVPS();
                    OnScanOff();
                    break;
            }           
        }

        private void SetState(VpsScanState state)
        {
            _currentState = state;

            var setupInfo = GetStateInfo(state);
            _buttonIcon.sprite = setupInfo.Icon;
            _buttonIcon.color = setupInfo.color;
            _messageText.SetText(setupInfo.Message);
            setupInfo.Event?.Invoke();

            if (!string.IsNullOrEmpty(setupInfo.Message))
            {
                // show the message box UI
                _message.DOAnchorPosX(40, 1);//.From(_message.rect.width);
            }
            else
            {
                // close the message box UI
                _message.DOAnchorPosX(_message.rect.width + 200, 1);//.From(40);
            }

            _loader.SetActive(setupInfo.ShowLoader);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_message);
            LayoutRebuilder.MarkLayoutForRebuild(_message);

            OnVpsButtonScanStateChanged?.Invoke(_currentState);
        }

        private VpsButtonState GetStateInfo(VpsScanState state)
        {
            var foundStateInfo = _states.FirstOrDefault(x => x.State == state);
            if (foundStateInfo != null)
            {
                return foundStateInfo;
            }
            else
            {
                Debug.LogError($"Missing State Setup in Inspector for {state}");
                throw new ArgumentException($"Missing State Info in Inspector", $"state");
            }
        }
    }
}