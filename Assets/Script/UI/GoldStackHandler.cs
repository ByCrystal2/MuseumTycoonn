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

    private bool isCounting = false;

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
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentTempMoney = Mathf.RoundToInt(Mathf.Lerp(0, amount, elapsed / duration));
            TempGoldText.text = "+" + currentTempMoney.ToString();
            yield return null;
        }

        TempGoldText.text = amount.ToString();

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentTempMoney = Mathf.RoundToInt(Mathf.Lerp(amount, 0, elapsed / duration));
            TempGoldText.text = "+" + currentTempMoney.ToString();
            realMoney += Mathf.RoundToInt((Time.deltaTime / duration) * amount);
            MainGoldText.text = "" + realMoney;
            yield return null;
        }

        TempGoldText.text = "";
        MainGoldText.text = Mathf.FloorToInt(MuseumManager.instance.GetCurrentGold()).ToString("");

        CloseCoroutine = StartCoroutine(ClosePanel());
    }

    public void AddTempGold(float amount)
    {
        if (!TempGoldText.gameObject.activeSelf && !MainGoldText.gameObject.activeSelf) return;
        moneyQueue.Enqueue(amount);

        if (!isCounting)
        {
            if (OpenCoroutine != null)
                StopCoroutine(OpenCoroutine);
            if (CloseCoroutine != null)
                StopCoroutine(CloseCoroutine);
            Panel.gameObject.SetActive(true);
            OpenCoroutine = StartCoroutine(OpenPanel());
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
