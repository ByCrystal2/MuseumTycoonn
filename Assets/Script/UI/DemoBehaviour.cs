using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DemoBehaviour : MonoBehaviour
{
    [SerializeField] RectTransform InformationPanel;
    private async void Awake()
    {
        await WaitForGameManager();

        if (GameManager.instance.IsDemo)
        {
            DontDestroyOnLoad(gameObject);
            InformationPanel.gameObject.SetActive(true);
        }
        else
            Destroy(gameObject, 0.5f);
    }
    private async Task WaitForGameManager()
    {
        while (GameManager.instance == null)
        {
            await Task.Delay(100);
        }
    }
}
