using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsHandler : MonoBehaviour
{
    [SerializeField] Credit CreditPrefab;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] Transform creditsContent;
    [SerializeField] Button exitButton;
    [SerializeField] List<CreditData> creditDatas = new List<CreditData>();

    public static CreditsHandler instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        exitButton.onClick.AddListener(CreditsPanelSetActivation);
    }
    private void Start()
    {
        int length = creditDatas.Count;
        for (int i = 0; i < length; i++)
        {
            CreditData data = creditDatas[i];
            Credit currentCredit = Instantiate(CreditPrefab, creditsContent);
            currentCredit.SetData(data);
            currentCredit.gameObject.SetActive(true);
        }
    }
    public void CreditsPanelSetActivation()
    {
        bool active = creditsPanel.activeSelf ? false : true;
        creditsPanel.SetActive(active);
    }
    public void OpenUrl(CreditData url)
    {
        Application.OpenURL(url.Description);
    }

}
