using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Text txtTitle;
    [SerializeField] Text txtURL;
    CreditData currentData;
    public void SetData(CreditData data)
    {
        currentData = data;
        txtTitle.text = data.Title;
        txtURL.text = data.Description;
        Button myButton = GetComponent<Button>();
        myButton.onClick.AddListener(() => CreditsHandler.instance.OpenUrl(currentData));
    }
    public string GetURL()
    {
        return currentData.Description;
    }
}
[System.Serializable]
public class CreditData
{
    public string Title;
    public string Description;
}
