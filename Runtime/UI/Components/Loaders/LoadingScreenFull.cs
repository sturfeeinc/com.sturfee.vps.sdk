using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LoaderType
{
    Indeterminate,
    Determinate
}

public class LoadingScreenFull : MonoBehaviour
{
    [SerializeField]
    private GameObject _infiniteLoader;

    [SerializeField]
    private GameObject _percentLoader;
    [SerializeField]
    private Slider _percentSlider;
    [SerializeField]
    public TextMeshProUGUI PercentText;

    [SerializeField]
    public TextMeshProUGUI Message;

    public void ShowLoader(LoaderType type = LoaderType.Indeterminate, string message = "Loading...")
    {
        Message.SetText(message);

        if (type == LoaderType.Determinate)
        {
            _infiniteLoader.SetActive(false);
            _percentLoader.SetActive(true);

            SetLoadingPercentage(0);
        }
        else
        {
            _infiniteLoader.SetActive(true);
            _percentLoader.SetActive(false);
        }
    }

    public void SetMessage(string message)
    {
        Message.SetText(message);
    }

    public void SetLoadingPercentage(float decimalPercent)
    {
        decimalPercent = Mathf.Clamp(decimalPercent, 0f, 1f);

        _percentSlider.value = decimalPercent;

        PercentText.SetText($"{Mathf.Round(100f * decimalPercent)}%");
    }
}

