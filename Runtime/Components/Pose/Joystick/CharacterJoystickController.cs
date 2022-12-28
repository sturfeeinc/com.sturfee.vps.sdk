using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterJoystickController : JoystickController
    {
        [SerializeField]
        private float _jumpSpeed = 5;

        private float _verticalSpeed = 0;
        private readonly float GRAVITY = 9.8f;
        private CharacterController _characterController;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        protected override void Move()
        {

            // Multiply the normalized direction by the speed
            Vector2 movementDirection = _moveJoystick.Direction.normalized * _moveSpeed * Time.deltaTime;
            // Move relatively to the local transform's direction
            var vel = transform.right * movementDirection.x + transform.forward * movementDirection.y;

            // handle jumping?
            if (_characterController.isGrounded)
            {
                _verticalSpeed = 0; // grounded character has vSpeed = 0...
                if (Input.GetKeyDown(KeyCode.Space))
                { // unless it jumps:
                    _verticalSpeed = _jumpSpeed;
                }
            }

            // apply gravity acceleration to vertical speed:
            _verticalSpeed -= GRAVITY * Time.deltaTime;
            vel.y = _verticalSpeed; // include vertical speed in vel

            _characterController.Move(vel);
        }
    }
}
