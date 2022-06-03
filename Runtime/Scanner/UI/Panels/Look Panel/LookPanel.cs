using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;
using UnityEngine.UI;

public class LookPanel : MonoBehaviour
{
    public int MinAngle ;
    public int MaxAngle;
    private int _targetOffset;

    [SerializeField]
    private RectTransform _target;
    [SerializeField]
    private GameObject _up;
    [SerializeField]
    private GameObject _down;        
    [SerializeField]
    private Button _scanButton;

    private int _viewAngle;
    private Camera _xrCamera;

    private void Awake()
    {
        MinAngle = ScanProperties.PitchMin + 5;
        MaxAngle = ScanProperties.PitchMax - 5;
    }

    private void Start()
    {
        _viewAngle = 90 + MaxAngle;
        _xrCamera = XRCamera.Camera;        

        // Distance to end of Up arrow from center
        _targetOffset = (int)(_up.GetComponent<RectTransform>().rect.max.y);
    }

    private void Update()
    {                
        SetTargetPosition();

        float pitch = _xrCamera.transform.eulerAngles.x;
        // set to [-180, 180] range
        pitch = pitch > 180 ? pitch - 360 : pitch;

        bool inRange = pitch < MaxAngle && pitch > MinAngle;

        _scanButton.interactable = inRange;
        _target.gameObject.SetActive(!inRange);
        _up.SetActive(pitch > MaxAngle);
        _down.SetActive(pitch < MinAngle);
    }
    
    private void SetTargetPosition()
    {
        var radialPosition = (180.0f - Vector3.Angle(_xrCamera.transform.forward, Vector3.down) - _viewAngle);
        var positionY = (Screen.height * Mathf.Sin(radialPosition * Mathf.Deg2Rad));

        positionY = Mathf.Clamp(positionY, -Screen.height, Screen.height);

        _target.anchoredPosition = new Vector2(0, positionY + _targetOffset); ;
    }

    private bool IsTargetVisibleOnScreen()
    {
        Vector3[] v = new Vector3[4];
        _target.GetWorldCorners(v);

        float maxY = Mathf.Max(v[0].y, v[1].y, v[2].y, v[3].y);
        float minY = Mathf.Min(v[0].y, v[1].y, v[2].y, v[3].y);
        //No need to check horizontal visibility: there is only a vertical scroll rect
        //float maxX = Mathf.Max (v [0].x, v [1].x, v [2].x, v [3].x);
        //float minX = Mathf.Min (v [0].x, v [1].x, v [2].x, v [3].x);

        return maxY > 0 && minY < Screen.height;
    }


}
