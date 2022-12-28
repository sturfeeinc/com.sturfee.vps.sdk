using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public class CaptureUI : MonoBehaviour
    {
        [SerializeField]
        private ScanTarget _scanTarget;
        [SerializeField]
        private RectTransform _cursor;
        [SerializeField]
        private HorizontalLayoutGroup _targetsLayout;

        private int _startYaw;
        private float _yawAngle;
        private int _targetCount;
        private Vector2 _cursorStartPos;
        private float _distanceBetweenTwoTargets;
        private int _index = 0;
        private bool _active;

        private ScanTarget[] _targets;

        private void Start()
        {
            _cursorStartPos = _cursor.GetComponent<RectTransform>().anchoredPosition;
        }

        private void Update()
        {
            if (_active)
            {
                float cursorPos = GetCursorPosition();
                if (cursorPos < _cursorStartPos.x)
                {
                    // Don't move cursor to left
                }
                else
                {
                    _cursor.anchoredPosition = new Vector2(cursorPos, 0);
                }
            }
        }

        public async void StartScan(float yawAngle, int targetCount)
        {
            //if (!gameObject.activeSelf)
            //{
            //    gameObject.SetActive(true);
            //    await Task.Yield();
            //}

            _yawAngle = yawAngle;
            _targetCount = targetCount;

            _targets = new ScanTarget[targetCount];

            ClearAllTargets();

            for (int i = 0; i < targetCount; i++)
            {
                _targets[i] = Instantiate(_scanTarget, _targetsLayout.transform);
            }

            _index = 0;
            _cursor.GetComponent<RectTransform>().anchoredPosition = _cursorStartPos;
            _startYaw = (int)Camera.transform.eulerAngles.y;

            // distance between 2 targets in screen-space
            float width = _scanTarget.GetComponent<RectTransform>().rect.width;
            _distanceBetweenTwoTargets = width + _targetsLayout.spacing;

            _active = true;
        }

        public void Capture()
        {
            _targets[_index].SetActive(false);
            _index++;
        }

        public void StopScan()
        {
            ClearAllTargets();
            _active = false;
        }

        private float GetCursorPosition()
        {
            int yaw = (int)Camera.transform.eulerAngles.y;

            int yawDiff = yaw - _startYaw;
            int absYawDiff = Mathf.Abs(yawDiff);

            if (absYawDiff > 180)
            {
                yawDiff = yawDiff > 0 ? -(360 - absYawDiff) : 360 - absYawDiff;
            }

            //If our capture range goes above 180
            float captureRange = (_targetCount - 1) *_yawAngle;
            if (yawDiff < 0 && captureRange > 180)
            {
                if (yawDiff > -180 && yawDiff <= captureRange - 360 + 5)    // + 5 is added for sanity just in case we want cursorPos beyond last gaze target
                {
                    yawDiff += 360;
                }
            }

            float multiplier = _distanceBetweenTwoTargets / _yawAngle;

            return (yawDiff * multiplier) + _cursorStartPos.x;
        }

        private void ClearAllTargets()
        {
            foreach(var target in _targetsLayout.GetComponentsInChildren<ScanTarget>())
            {
                DestroyImmediate(target.gameObject);
            }
        }

        private Camera Camera
        {
            get
            {
                if (XrCamera.Camera != null && XrSessionManager.GetSession() != null)
                {
                    return XrCamera.Camera;
                }
                return Camera.main;
            }
        }
    }
}
