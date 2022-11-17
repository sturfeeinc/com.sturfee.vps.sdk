using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class JoystickController : MonoBehaviour
    {
        [SerializeField]
        protected FixedJoystick _moveJoystick;
        [SerializeField]
        protected FixedJoystick _lookJoystick;

        [Header("Config")]
        [SerializeField]
        protected int _moveSpeed = 20;
        [SerializeField]
        protected float pitchClampMin = -45f;
        [SerializeField]
        protected float pitchClampMax = 45f;
        [SerializeField]
        protected bool _invertPitch = true;
        [SerializeField]
        protected bool _invertYaw = false;

        protected virtual void Update()
        { 
            Move();
            Rotate();
        }

        protected virtual void Move()
        {
            transform.Translate(_moveJoystick.Horizontal * _moveSpeed * Time.deltaTime, 0, _moveJoystick.Vertical * _moveSpeed * Time.deltaTime);
        }

        protected virtual void Rotate()
        {
            Vector3 euler = transform.localEulerAngles;
            var pitch = _invertPitch ? -_lookJoystick.Vertical : _lookJoystick.Vertical;
            euler.x += pitch;
            euler.x = euler.x > 180 ? euler.x - 360 : euler.x;
            euler.x = Mathf.Clamp(euler.x, pitchClampMin, pitchClampMax);
            euler.y += _invertYaw ? -_lookJoystick.Horizontal : _lookJoystick.Horizontal;
            euler.z = 0;

            transform.localEulerAngles = euler;
        }
    }
}
