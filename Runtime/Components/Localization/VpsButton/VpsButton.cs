using DG.Tweening;
using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
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
    }

    public delegate void VpsButtonStateDelegate(VpsScanState state);

    public class VpsButton : SceneSingleton<VpsButton>
    {
        public event VpsButtonStateDelegate OnVpsButtonScanStateChanged;
        public SturfeeVPSLocalizationProvider LocalizationProvider;

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
        [SerializeField][ReadOnly]
        protected VpsScanState _currentState = VpsScanState.Off;
        [SerializeField]
        private List<VpsButtonState> _states = new List<VpsButtonState>();

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);

            LocalizationProvider.OnLocalizationStart += OnLocalizationStart;
            LocalizationProvider.OnLocalizationLoading += OnLocalizationLoading;
            LocalizationProvider.OnLocalizationFail += OnLocalizationFail;
            LocalizationProvider.OnLocalizationSuccessful += OnLocalizationSuccessful;
        }

        private void Start()
        {
            SetState(VpsScanState.Off);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);

            LocalizationProvider.OnLocalizationStart -= OnLocalizationStart;
            LocalizationProvider.OnLocalizationLoading -= OnLocalizationLoading;
            LocalizationProvider.OnLocalizationFail -= OnLocalizationFail;
            LocalizationProvider.OnLocalizationSuccessful -= OnLocalizationSuccessful;
        }

        public void SetState(VpsScanState state)
        {
            _currentState = state;

            var setupInfo = GetStateInfo(state);
            _buttonIcon.sprite = setupInfo.Icon;
            _buttonIcon.color = setupInfo.color;
            _messageText.SetText(setupInfo.Message);
            //setupInfo.Event?.Invoke();

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

        private void HandleClick()
        {
            switch (_currentState)
            {
                case VpsScanState.Off:
                    EnableVPS();
                    SetState(VpsScanState.Initializing);
                    break;
                case VpsScanState.Initializing:
                    StopVPS();
                    SetState(VpsScanState.Off);
                    break;
                case VpsScanState.ReadyToScan:
                    StopVPS();
                    SetState(VpsScanState.Off);
                    break;
                case VpsScanState.Scanning:
                    StopVPS();
                    SetState(VpsScanState.ReadyToScan);
                    break;
                case VpsScanState.Loading:
                    StopVPS();
                    SetState(VpsScanState.ReadyToScan);
                    break;
                case VpsScanState.ScanComplete:
                    DisableVPS();
                    SetState(VpsScanState.Off);
                    break;
            }
        }

        private void EnableVPS()
        {
            LocalizationProvider.EnableLocalization();
        }

        private void StopVPS()
        {
            LocalizationProvider.StopLocalization();              
        }

        private void DisableVPS()
        {
            LocalizationProvider.DisableLocalization();
        }

        private void OnLocalizationStart()
        {
            SetState(VpsScanState.Scanning);
        }

        private void OnLocalizationLoading()
        {
            SetState(VpsScanState.Loading);
        }

        private void OnLocalizationFail(string error)
        {
            SetState(VpsScanState.Off);
        }

        private void OnLocalizationSuccessful()
        {
            SetState(VpsScanState.ScanComplete);
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
