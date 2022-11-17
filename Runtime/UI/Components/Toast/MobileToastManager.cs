using SturfeeVPS.SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileToastManager : SimpleSingleton<MobileToastManager>
{
    public MobileToastUi Ui;

    private Action _callback = null;

    [SerializeField]
    private float _totalTimeout = 0;

    [SerializeField]
    private bool _isOpen = false;

    private void Start()
    {
        Ui = FindObjectOfType<MobileToastUi>();
        Ui.Button.onClick.AddListener(HandleButtonClick);

        Ui.Hide(true);

        StartCoroutine(StartTimerCo());
    }

    private void OnDestroy()
    {
        if (Ui != null && Ui.Button != null)
        {
            Ui.Button.onClick.RemoveListener(HandleButtonClick);
        }
    }

    public void ShowToastWithButton(string message, string buttonText = "OK")
    {
        ShowToast(message, -1, true, buttonText);
    }

    public void ShowToastWithAction(string message, string buttonText, Action onButtonClick)
    {
        ShowToast(message, -1, true, buttonText, onButtonClick);
    }

    public void ShowToast(string message, float timeoutSeconds = 5f, bool showButton = false, string buttonText = "Ok", Action onButtonClick = null)
    {
        _callback = onButtonClick;
        Ui.SetData(message, showButton, buttonText);

        if (timeoutSeconds > 0)
        {
            _totalTimeout += timeoutSeconds;
            _totalTimeout = Mathf.Clamp(_totalTimeout, 0, 10);
        }
        else
        {
            _totalTimeout = -1;
        }

        ShowToast(_totalTimeout);
        //StartCoroutine(ShowToastCo(_totalTimeout));
    }

    public void HideToast(bool immediate = false)
    {
        _totalTimeout = 0;
        Ui.Hide();
        _isOpen = false;
    }

    //private IEnumerator ShowToastCo(float timeout)
    //{
    //    if (!_isOpen)
    //    {
    //        _isOpen = true;
    //        Ui.Show();

    //        if (_totalTimeout >= 0)
    //        {
    //            MyLogger.Log($"MobileToastManager :: waiting {_totalTimeout} seconds...");
    //            //yield return new WaitForSeconds(_totalTimeout);
    //            yield return new WaitUntil(() => _totalTimeout == 0);

    //            MyLogger.Log($"MobileToastManager :: done waiting. closing toast...");
    //            HideToast();
    //        }
    //    }    
    //}

    private void ShowToast(float timeout)
    {
        if (!_isOpen)
        {
            _isOpen = true;
            Ui.Show();
        }
    }

    private void HandleButtonClick()
    {
        HideToast();

        _callback?.Invoke();
        _callback = null;
    }

    private IEnumerator StartTimerCo()
    {
        yield return new WaitForSeconds(1);        

        if (_totalTimeout > 0)
        {
            _totalTimeout--;
        }
        
        if (_totalTimeout == 0 && _isOpen)
        {
            HideToast();
        }

        StartCoroutine(StartTimerCo());
    }
}
