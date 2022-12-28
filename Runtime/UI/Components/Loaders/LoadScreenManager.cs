using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SturfeeVPS.SDK;

public class LoadScreenManager : SimpleSingleton<LoadScreenManager>
{
    [SerializeField]
    private LoadingScreenFull _loadingScreen;

    private void Awake()
    {
        _loadingScreen = FindObjectOfType<LoadingScreenFull>(true);
        if (_loadingScreen == null) { Debug.LogError("Missing Loading Screen UI"); return; }
        _loadingScreen.gameObject.SetActive(false);
    }

    public LoadingScreenFull GetLoadingScreen()
    {
        return _loadingScreen;
    }

    public void SetMessage(string message)
    {
        _loadingScreen.SetMessage(message);
    }

    public void SetLoadingPercentage(float decimalPercent)
    {
        _loadingScreen.SetLoadingPercentage(decimalPercent);
    }

    public void ShowLoadingScreen(LoaderType type = LoaderType.Indeterminate, string message = "Loading...")
    {
        if (_loadingScreen == null) { return; }
        _loadingScreen.gameObject.SetActive(true);
        _loadingScreen.ShowLoader(type, message);
    }

    public void HideLoadingScreen()
    {
        if (_loadingScreen == null) { return; }
        _loadingScreen.gameObject.SetActive(false);
    }
}
