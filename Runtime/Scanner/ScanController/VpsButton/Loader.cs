using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    [SerializeField]
    private RectTransform _graphic;

    private Vector3 _rotateTo = new Vector3(0, 0, 360);
    private void Start()
    {
        _graphic.transform.DOLocalRotate(_rotateTo, 1, RotateMode.FastBeyond360).From(Vector3.zero).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
}
