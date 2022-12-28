using DG.Tweening;
using SturfeeVPS.SDK;
using System;
using UnityEngine;

public class AlertDialogManager : SimpleSingleton<AlertDialogManager>
{
    public AlertDialog Ui;

    private Action<bool> _callback = null;

    private void Start()
    {
        Ui = FindObjectOfType<AlertDialog>(true);
        Ui.gameObject.SetActive(true);
        Ui.Hide(true);
        Ui.Overlay.alpha = 0.0f;
        Ui.Overlay.gameObject.SetActive(false);

        Ui.SuccessButton.onClick.AddListener(OnSuccess);
        Ui.CancelButton.onClick.AddListener(OnCancel);
    }

    private void OnDestroy()
    {
        if (Ui != null && Ui.SuccessButton != null)
        {
            Ui.SuccessButton.onClick.RemoveAllListeners();
        }
        if (Ui != null && Ui.CancelButton != null)
        {
            Ui.CancelButton.onClick.RemoveAllListeners();
        }
    }

    public void ShowAlert(string title, string message, Action<bool> callback, string buttonText = "Ok", bool showCancel = false)
    {
        _callback = callback;

        Ui.SetData(title, message, buttonText, showCancel);
        Ui.Show();

        Ui.Overlay.gameObject.SetActive(true);
        Ui.Overlay.DOFade(1, 0.3f);
    }

    public void HideAlert(bool immediate = false)
    {
        Ui.Hide();
        Ui.Overlay.DOFade(0, 0.3f).OnComplete(() => { Ui.Overlay.gameObject.SetActive(false); });
    }

    private void OnSuccess()
    {
        HideAlert();
        _callback?.Invoke(true);
        _callback = null;
    }

    private void OnCancel()
    {
        HideAlert();
        _callback?.Invoke(false);
        _callback = null;
    }
}
