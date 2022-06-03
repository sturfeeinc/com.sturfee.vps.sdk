using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

public class CaptureUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _cursor;
    [SerializeField]
    private GameObject _line;
    [SerializeField]
    private GameObject _gazeTarget;    

    private int _startYaw;
    private float _factor;
    private float _padding;
    private int _scanCursorSize;
    private int _gazeTargetSize = 75;
    private GameObject[] _gazetargets;

    private void Start()
    {
        SturfeeEventManager.Instance.OnFrameCaptured += OnFrameCaptured;
    }

    private void OnDestroy()
    {
        SturfeeEventManager.Instance.OnFrameCaptured -= OnFrameCaptured;
    }


    public async void StartScan()
    {
        _scanCursorSize = (int)(_gazeTargetSize * 0.75f);
        _padding = GetComponent<RectTransform>().rect.width * 0.2f;

        float width = GetComponent<RectTransform>().rect.width;
        _factor = (width - (_padding * 2)) / (ScanProperties.TargetCount - 1);

        //Set Yaw to start from where our current Yaw is 
        _startYaw = (int)XRCamera.Pose.Rotation.eulerAngles.y;

        //Create Scan UI 
        await CreateScanCaptureUI();
    }

    public void StopScan()
    {

    }

    private void OnFrameCaptured(int frameNum, LocalizationRequest localizationRequest, byte[] image)
    {
        Destroy(_gazetargets[frameNum - 1]);
    }

    private async Task CreateScanCaptureUI()
    {
        int width = (int)GetComponent<RectTransform>().rect.width;
        float height = 0;   // Screen.height * 0.05f;
                            //Create Scan-Line
        GameObject scanLine = _line;
        scanLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
        scanLine.GetComponent<RectTransform>().sizeDelta = new Vector2(width - _padding * 2, 1);
        scanLine.transform.localScale = new Vector3(1, (float)width / Screen.width, 1);


        //Create Gaze targets
        if (_gazetargets!= null && _gazetargets.Any())
        {
            foreach (var target in _gazetargets)
            {
                Destroy(target);
            }
        }
        _gazetargets = new GameObject[ScanProperties.TargetCount];
        float multiplier = (width - _padding * 2) / (ScanProperties.TargetCount - 1);

        for (int i = 1; i < _gazetargets.Length; i++)
        {
            _gazetargets[i] = Instantiate(_gazeTarget);
            _gazetargets[i].transform.SetParent(transform);
            _gazetargets[i].GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            _gazetargets[i].GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            _gazetargets[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * multiplier + _padding, height);
            _gazetargets[i].GetComponent<RectTransform>().sizeDelta = new Vector2(_gazeTargetSize, _gazeTargetSize);
        }

       

        //Create Scan Cursor
        GameObject scanCursor = _cursor;
        scanCursor.GetComponent<RectTransform>().sizeDelta = new Vector2(_scanCursorSize, _scanCursorSize);
        scanCursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(_padding, height);

        while (!gameObject.activeSelf)
        {
            await Task.Yield();
        }

        StartCoroutine(MoveCursor(scanCursor.GetComponent<RectTransform>()));
    }

    private IEnumerator MoveCursor(RectTransform scanCursorRT)
    {
        float cursorStart = _padding;
        var xrsession = XRSessionManager.GetSession();
        yield return new WaitUntil(() => xrsession?.Status == XRSessionStatus.Scanning);
        while (_gazetargets?.Length > 0 && xrsession?.Status == XRSessionStatus.Scanning)
        {
            float cursorPos = GetCursorPosition();
            if (cursorPos < cursorStart)
            {
                //Don't move cursor to left
            }
            else
            {
                scanCursorRT.anchoredPosition = new Vector2(cursorPos, 0 /*Screen.height * 0.05f*/);
            }

            yield return null;
        }
    }

    private float GetCursorPosition()
    {
        int yaw = (int)XRCamera.Pose.Rotation.eulerAngles.y;

        int yawDiff = yaw - _startYaw;
        int absYawDiff = Mathf.Abs(yawDiff);

        if (absYawDiff > 180)
        {
            yawDiff = yawDiff > 0 ? -(360 - absYawDiff) : 360 - absYawDiff;
        }

        //If our capture range goes above 180
        float captureRange = (ScanProperties.TargetCount - 1) * ScanProperties.YawAngle;
        if (yawDiff < 0 && captureRange > 180)
        {
            if (yawDiff > -180 && yawDiff <= captureRange - 360 + 5)    // + 5 is added for sanity just in case we want cursorPos beyond last gaze target
            {
                yawDiff += 360;
            }
        }

        float multiplier = _factor / ScanProperties.YawAngle;

        return (yawDiff * multiplier) + _padding;
    }
}
