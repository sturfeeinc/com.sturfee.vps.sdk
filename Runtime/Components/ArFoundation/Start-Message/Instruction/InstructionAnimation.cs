using DG.Tweening;
using UnityEngine;

namespace SturfeeVPS.SDK.Examples
{
    public class InstructionAnimation : MonoBehaviour
    {
        public RectTransform Foreground;
        public RectTransform Background;

        private void Start()
        {
            Foreground.DOAnchorPos(new Vector2(0, -80), 3).SetLoops(-1).SetEase(Ease.OutExpo);
            Background.DOAnchorPos(new Vector2(55, 120), 3).SetLoops(-1).SetEase(Ease.OutExpo);
        }
    } 
}
