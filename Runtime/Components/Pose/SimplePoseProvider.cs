using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class SimplePoseProvider : BasePoseProvider
    {
        public int MoveSpeed = 5;
        public int RotateSpeed = 60;
        public float HeightFromGround = 1.5f;

        private void Update()
        {
            transform.Translate(0, 0, Input.GetAxis("Vertical") * MoveSpeed * Time.deltaTime);
            transform.Rotate(0, Input.GetAxis("Horizontal") * RotateSpeed * Time.deltaTime, 0);
        }

        public override float GetHeightFromGround()
        {
            return HeightFromGround;
        }

        public override Vector3 GetPosition(out bool includesElevation)
        {
            includesElevation = false;
            return Converters.UnityToWorldPosition(transform.position);
        }

        public override ProviderStatus GetProviderStatus()
        {
            return ProviderStatus.Ready;
        }

        public override Quaternion GetRotation()
        {
            return Converters.UnityToWorldRotation(transform.rotation);
        }
    }
}
