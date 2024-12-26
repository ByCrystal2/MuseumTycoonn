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

    private Coroutine OpenCoroutine;
    private Coroutine CloseCoroutine;
    private Coroutine AddCoroutine;

    private float realMoney = 0;   // The player's real money
    private Queue<float> moneyQueue = new Queue<float>(); // Queue to handle stacking of money

    public bool isCounting = false;

    private IEnumerator AnimateMoney()
    {
        isCounting = true;

        while (moneyQueue.Count > 0)
        {
            float amount = moneyQueue.Dequeue();
            yield return StartCoroutine(AnimateMoneyRoutine(amount));
        }

        isCounting = false;
    }

    private IEnumerator AnimateMoneyRoutine(float amount)
    {
        float startingGold = MuseumManager.instance.GetCurrentGold();

        float duration = 0.3f; // Smooth animation duration
        float elapsed = 0f;

        // Temporary gold addition animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentTempMoney = Mathf.RoundToInt(Mathf.Lerp(0, amount, elapsed / duration));
            TempGoldText.text = "+" + currentTempMoney.ToString();
            yield return null;
        }

        TempGoldText.text = "+" + amount.ToString();

        // Smoothly add temporary gold to main gold
        elapsed = 0f;
        float targetGold = startingGold + amount;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentGold = Mathf.Lerp(startingGold, targetGold, elapsed / duration);
            MainGoldText.text = Mathf.FloorToInt(currentGold).ToString();
            yield return null;
        }

        // Ensure final values match exactly
        MainGoldText.text = Mathf.FloorToInt(targetGold).ToString();
        TempGoldText.text = "";

        // Close the panel after completion
        //CloseCoroutine = StartCoroutine(ClosePanel());
    }

    public void AddTempGold(float amount)
    {
        //if (!TempGoldText.gameObject.activeSelf && !MainGoldText.gameObject.activeSelf) return;
        moneyQueue.Enqueue(amount);

        if (!isCounting)
        {
            //if (OpenCoroutine != null)
            //    StopCoroutine(OpenCoroutine);
            //if (CloseCoroutine != null)
            //    StopCoroutine(CloseCoroutine);
            //Panel.gameObject.SetActive(true);
            //OpenCoroutine = StartCoroutine(OpenPanel());
            StartCoroutine(AnimateMoney());
        }
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
