using System;
using UnityEngine;

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateSingleton);

    public static T Instance => LazyInstance.Value;

    private static T CreateSingleton()
    {
        var instance = new T();
        return instance;
    }

}
