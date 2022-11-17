using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class MobileJoystickPoseProvider : BasePoseProvider
    {
        [SerializeField]
        private Transform _joystick;
        [SerializeField]
        private CharacterController _characterController;

        [Space(5)]
        [SerializeField][ReadOnly]
        private float _elevation;
        
        private void OnEnable()
        {
            SturfeeEventManager.OnTilesLoaded += OnTilesLoaded;
        }

        private void OnDisable()
        {
            SturfeeEventManager.OnTilesLoaded -= OnTilesLoaded;
        }

        public override void OnRegister()
        {
            base.OnRegister();
            _joystick.position = XrCamera.Pose.Position;
            _joystick.rotation = XrCamera.Pose.Rotation;
        }

        public override float GetHeightFromGround()
        {
            return 1.5f;
        } 

        public override Vector3 GetPosition(out bool includesElevation)
        {
            includesElevation = _elevation != 0;
            return Converters.UnityToWorldPosition(_joystick.transform.position);
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }

        public override Quaternion GetRotation()
        {
            return Converters.UnityToWorldRotation(_joystick.transform.rotation);
        }

        private void OnTilesLoaded()
        {
            var location = Converters.UnityToGeoLocation(transform.position);
            var tilesProvider = XrSessionManager.GetSession().GetProvider<ITilesProvider>();
            _elevation = tilesProvider.GetElevation(location) + GetHeightFromGround();

            Vector3 pos = _joystick.transform.position;
            pos.y = _elevation + GetHeightFromGround();
            _joystick.transform.position = pos;

            _characterController.enabled = true;
        }
    }
}
