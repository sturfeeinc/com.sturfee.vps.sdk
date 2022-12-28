using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteCircleLoader : MonoBehaviour
{
    [SerializeField]
    private float _time = 1;
    [SerializeField]
    private Image[] Parts;

    Sequence sequence;

    private List<Tween> tweens = new List<Tween>();

    private void Start()
    {
        Setup();
    }

    private void OnEnable()
    {
        Setup();
    }

    private void OnDisable()
    {
        foreach (var tween in tweens)
        {
            tween.Kill();
        }

        tweens.Clear();
    }

    void Setup()
    {
        if (tweens.Count != 0)
        {
            return;
        }

        float delay = (_time / Parts.Length);

        Debug.Log($"Showing Loader Animation ({Parts.Length}, {delay})...");

        for (var i = 0; i < Parts.Length; i++)
        {
            var tween1 = Parts[i].GetComponent<RectTransform>().DOScale(Vector3.one, _time).From(Vector3.zero).SetLoops(-1).SetDelay((float)(i * delay)).SetEase(Ease.InOutExpo);
            var tween2 = Parts[i].DOFade(0f, _time).From(1.0f).SetLoops(-1).SetDelay((float)(i * delay)).SetEase(Ease.InOutExpo);
        
            tweens.Add(tween1);
            tweens.Add(tween2);
        }
    }
}
