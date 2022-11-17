using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertDialog : MonoBehaviour
{
    public CanvasGroup Overlay;

    [Header("Config")]
    public float AnimationSpeed = 0.3f;

    [Header("Internal")]
    [SerializeField]
    private RectTransform _container;
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _message;
    [SerializeField]
    private TextMeshProUGUI _buttonText;

    public Button SuccessButton => _successButton;
    [SerializeField]
    private Button _successButton;

    public Button CancelButton => _cancelButton;
    [SerializeField]
    private Button _cancelButton;

    public void SetData(string title, string message, string buttonText, bool showCancel)
    {
        _title.SetText(title);
        _message.SetText(message);
        _buttonText.SetText(buttonText);

        _cancelButton.gameObject.SetActive(showCancel);
    }

    public void Show()
    {
        _container.DOScale(1, AnimationSpeed).From(0);
    }

    public void Hide(bool immediate = false)
    {
        _container.DOScale(0, immediate ? 0 : AnimationSpeed);
    }
}
