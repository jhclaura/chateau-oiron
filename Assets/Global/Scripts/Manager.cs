using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Manager<T> : MonoBehaviour
    where T : Manager<T>, new()
{
    private static T _Instance;
    public static T Instance
    {
        get
        {
            if (_Instance == null) _Instance = GameObject.FindObjectOfType<T>();
            return _Instance;
        }
    }

    private void OnEnable()
    {
        _Instance = this as T;
    }

}