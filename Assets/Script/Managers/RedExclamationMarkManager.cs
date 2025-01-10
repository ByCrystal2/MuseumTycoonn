using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedExclamationMarkManager : MonoBehaviour
{
    [SerializeField] GameObject redMarkPrefab;

    private Dictionary<Transform, RedExclamationMarkHandler> markDictionary = new();
    public static RedExclamationMarkManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public void CreateMark(Transform _target)
    {
        // Eðer mark zaten varsa, iþlemi sonlandýr
        if (markDictionary.ContainsKey(_target)) return;

        // Yeni mark oluþtur
        GameObject newMark = Instantiate(redMarkPrefab, _target);
        RedExclamationMarkHandler markHandler = newMark.GetComponentInChildren<RedExclamationMarkHandler>();
        Debug.Log("Mark Created: " +  markHandler.gameObject.name, markHandler.transform);
        markDictionary[_target] = markHandler;
    }

    public void RemoveMark(Transform _target)
    {
        if (markDictionary.TryGetValue(_target, out var markHandler))
        {
            Destroy(markHandler.gameObject); // Mark'ý yok et
            markDictionary.Remove(_target); // Dictionary'den çýkar
        }
    }
}
