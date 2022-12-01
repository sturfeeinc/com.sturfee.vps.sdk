using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SimpleLocalizationProvider : BaseLocalizationProvider
    {
        [SerializeField]
        private OffsetType _offsetType;
        [SerializeField]
        private Quaternion _yawOffset;
        [SerializeField]
        private Quaternion _pitchOffset;
        [SerializeField]
        private Quaternion _rollOfset;
        [SerializeField]
        private Vector3 _eulerOffset;
        [SerializeField]
        private GeoLocation _vpsLocation;
        [SerializeField]
        private ProviderStatus _providerStatus;


        public override OffsetType OffsetType { get => _offsetType; protected set => throw new System.NotImplementedException(); }
        public override Quaternion YawOffset { get => _yawOffset; protected set => throw new System.NotImplementedException(); }
        public override Quaternion PitchOffset { get => _pitchOffset; protected set => throw new System.NotImplementedException(); }
        public override Quaternion RollOffset { get => _rollOfset; protected set => throw new System.NotImplementedException(); }
        public override Vector3 EulerOffset { get => _eulerOffset; protected set => throw new System.NotImplementedException(); }
        public override Scanner Scanner { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EnableLocalization();
                _providerStatus = ProviderStatus.Ready;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                DisableLocalization();
            }


        }

        public override void DisableLocalization()
        {
            TriggerLocalizationDisabledEvent();
        }

        public override void EnableLocalization()
        {
            TriggerLocalizationRequestedEvent();
            TriggerLocalizationStartEvent();
            TriggerLocalizationSuccessfulEvent();
        }

        public override ProviderStatus GetProviderStatus()
        {
            return _providerStatus;
        }

        public override GeoLocation GetVpsLocation(out bool includesElevation)
        {
            includesElevation = true;
            return _vpsLocation;
        }

        public override void StopLocalization()
        {
            throw new System.NotImplementedException();
        }       
    }
}
