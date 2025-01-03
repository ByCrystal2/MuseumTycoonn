using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    public static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("CoroutineHelper");
                _instance = obj.AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(obj); // Sahne deðiþiminde yok edilmesin.
            }
            return _instance;
        }
    }

    public Coroutine RunCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
}

