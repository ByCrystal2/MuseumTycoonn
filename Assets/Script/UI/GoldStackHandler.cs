using SpriteShadersUltimate.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldStackHandler : MonoBehaviour
{
    public Text MainGoldText;
    public Text TempGoldText;
    public Image Panel;

    private float targetStackTimer; 
    private float targetCollectTimer;

    private float currentTempGold;
    private float targetTempGold;
    private float changeAmountTemp;
    private float goldBeforeEarn;

    private void Update()
    {
        if (targetStackTimer > 0)
        {
            if (targetTempGold > 0)
            {
                targetStackTimer -= Time.deltaTime;
                float change = Time.deltaTime * changeAmountTemp * 5;
                if (change > targetTempGold)
                {
                    change = targetTempGold;
                    targetTempGold = 0;
                    targetStackTimer = 0;
                    targetCollectTimer = 2;
                }
                else
                    targetTempGold -= change;

                currentTempGold += change;
                TempGoldText.text = "+" + currentTempGold.ToString("F0");
                return;
            }
        }

        if (targetTempGold > 0)
        {
            targetStackTimer = 2;
            return;
        }

        if (targetCollectTimer > 0)
        {
            if (currentTempGold > 0)
            {
                targetCollectTimer -= Time.deltaTime * 5;
                float change = Time.deltaTime * changeAmountTemp * 5;
                if (change > currentTempGold)
                {
                    change = currentTempGold;
                    currentTempGold = 0;
                    targetCollectTimer = 0;
                    StartCoroutine(ClosePanel());
                }
                else
                    currentTempGold -= change;

                TempGoldText.text = currentTempGold.ToString("F0");
                MainGoldText.text = (goldBeforeEarn + change).ToString("F0");
                goldBeforeEarn += change;
                return;
            }
        }

        if (currentTempGold > 0)
        {
            targetCollectTimer = 2;
            return;
        }

        TempGoldText.text = "";
    }

    public void AddTempGold(float _earnedGold)
    {
        if (!Panel.gameObject.activeSelf)
        {
            StopAllCoroutines();
            goldBeforeEarn = MuseumManager.instance.GetCurrentGold();
            Panel.gameObject.SetActive(true);
            StartCoroutine(OpenPanel());
        }
       
        targetStackTimer = 2;
        targetTempGold += _earnedGold;

        changeAmountTemp = ((targetTempGold - currentTempGold) / 2);
    }

    IEnumerator OpenPanel()
    {
        while (Panel.color.a < 1)
        {
            Color cc = Panel.color;
            cc.a += Time.deltaTime * 2;
            if (cc.a > 1)
                cc.a = 1;

            Panel.color = cc;
            yield return new WaitForSeconds(0.01f);
        }

        Color ccc = Panel.color;
        ccc.a = 1;
        Panel.color = ccc;
    }

    IEnumerator ClosePanel()
    {
        while (Panel.color.a < 1)
        {
            Color cc = Panel.color;
            cc.a -= Time.deltaTime * 2;
            if (cc.a < 0)
                cc.a = 0;

            Panel.color = cc;
            yield return new WaitForSeconds(0.01f);
        }

        Color ccc = Panel.color;
        ccc.a = 0;
        Panel.color = ccc;

        Panel.gameObject.SetActive(false);
    }
}
