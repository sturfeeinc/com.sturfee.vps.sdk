using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public abstract class BaseLocalizationProvider : BaseProvider, ILocalizationProvider
    {
        public abstract OffsetType OffsetType { protected set; get; }
        public abstract Quaternion YawOffset { protected set; get; }
        public abstract Quaternion PitchOffset { protected set; get; }
        public abstract Quaternion RollOffset { protected set; get; }
        public abstract Vector3 EulerOffset { protected set; get; }

        public abstract Scanner Scanner { protected set; get; }

        public event LocalizationRequestedAction OnLocalizationRequested;
        public event LocalizationStartAction OnLocalizationStart;
        public event LocalizationStopAction OnLocalizationStop;
        public event LocalizationLoadingAction OnLocalizationLoading;
        public event LocalizationSuccessfulAction OnLocalizationSuccessful;
        public event LocalizationFailAction OnLocalizationFail;
        public event LocalizationDisabledAction OnLocalizationDisabled;

        public abstract GeoLocation GetVpsLocation(out bool includesElevation);
        public abstract void DisableLocalization();
        public abstract void StopLocalization();
        public abstract void EnableLocalization();
        //public abstract ProviderStatus GetProviderStatus();
        public override void OnRegister()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public override void OnUnregister()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void TriggerLocalizationRequestedEvent()
        {
            OnLocalizationRequested?.Invoke();
        }

        protected virtual void TriggerLocalizationStartEvent()
        {
            OnLocalizationStart?.Invoke();
        }
        protected virtual void TriggerLocalizationStopEvent()
        {
            OnLocalizationStop?.Invoke();
        }

        protected virtual void TriggerLocalizationLoadingedEvent()
        {
            OnLocalizationLoading?.Invoke();
        }

        protected virtual void TriggerLocalizationFailEvent(string error)
        {
            OnLocalizationFail?.Invoke(error);
        }

        protected virtual void TriggerLocalizationSuccessfulEvent()
        {
            OnLocalizationSuccessful?.Invoke();
        }

        protected virtual void TriggerLocalizationDisabledEvent()
        {
            OnLocalizationDisabled?.Invoke();
        }

        
    }
}
