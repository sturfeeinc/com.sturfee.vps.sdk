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
        [SerializeField][ReadOnly]
        private Vector3 _lastPosition;
        [SerializeField][ReadOnly]
        private Quaternion _lastRotation;

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


            var distance = Vector3.Distance(XrCamera.Pose.Position, Vector3.zero);
            if (distance < 300)     // 300 => range of tiles. TODO: have this number coming from tileprovider
            {
                _joystick.position = XrCamera.Pose.Position;
                _joystick.rotation = XrCamera.Pose.Rotation;
            }
            else
            {
                _joystick.position = _lastPosition;
                _joystick.rotation = _lastRotation;
            }

            _joystick.position = new Vector3(_joystick.position.x, _elevation, _joystick.position.z);

            SturfeeDebug.Log($" Initial Joystick position : {_joystick.position}");

            // Enable Avatar Mesh
            // SturfeeEventManager.AvatarOn = true;
        }

        public override void OnUnregister()
        {            
            _lastPosition = _joystick.transform.position;
            _lastRotation = _joystick.transform.rotation;

            base.OnUnregister();    

            // Enable Avatar Mesh
            // SturfeeEventManager.AvatarOn = false;
        }

        public override float GetHeightFromGround()
        {
            return 1.5f;
        } 

        public override Vector3 GetPosition(out bool includesElevation)
        {
            includesElevation = _elevation != 0;
            // FOR DEBUG
            // SturfeeDebug.Log("MobileJoystickPoseProvider.cs is being called for position!!");

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
            
            // FOR DEBUG
            // Debug.Log($"[MobileJoystickPoseProvider.cs] Location: {Converters.GeoToUnityPosition(location)}");

            var tilesProvider = XrSessionManager.GetSession().GetProvider<ITilesProvider>();
            _elevation = tilesProvider.GetElevation(location) + GetHeightFromGround();

            // FOR DEBUG
            // Debug.Log($"[MobileJoystickPoseProvider.cs] Elevation: {_elevation}");

            Vector3 pos = _joystick.transform.position;
            pos.y = _elevation + GetHeightFromGround();
            _joystick.transform.position = pos;

            _characterController.enabled = true;

            // Turn on player avatar
            SturfeeEventManager.AvatarOn = true;
        }
    }
}
