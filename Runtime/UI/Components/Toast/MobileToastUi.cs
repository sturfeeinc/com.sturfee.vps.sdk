using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileToastUi : MonoBehaviour
{
    [Header("Config")]
    public float AnimationSpeed = 0.3f;

    [Header("Internal")]
    [SerializeField]
    private RectTransform _container;
    [SerializeField]
    private TextMeshProUGUI _message;
    [SerializeField]
    private TextMeshProUGUI _buttonText;    
    public Button Button => _button;
    [SerializeField]
    private Button _button;

    private void Start()
    {
        var mgr = MobileToastManager.Instance;
    }

    public void SetData(string message, bool showButton, string buttonText)
    {
        _message.SetText(message);

        _button.gameObject.SetActive(showButton);
        _buttonText.SetText(buttonText);
    }

    public void Show()
    {
        _container.DOAnchorPosY(0, AnimationSpeed);
    }

    public void Hide(bool immediate = false)
    {
        _message.SetText("");
        _container.DOAnchorPosY(-_container.rect.height, immediate ? 0 : AnimationSpeed);
    }
}
