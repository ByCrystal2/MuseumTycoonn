using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialEndPanelController : MonoBehaviour
{
    [SerializeField] UIFade imgBackground;
    [SerializeField] Text journeyBeginsText;
    [SerializeField] Transform pnlTopContent;
    [SerializeField] Transform pnlBottomContent;

    public static TutorialEndPanelController Instance { get; private set; }
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        int myChild = transform.childCount;
        for (int i = 0; i < myChild; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }
    public IEnumerator IEStartEndPanelProgress()
    {
        int myChild = transform.childCount;
        for (int i = 0; i < myChild; i++)
            transform.GetChild(i).gameObject.SetActive(true);
        imgBackground.FadeIn(0.9f, 0.8f);
        yield return new WaitForSeconds(0.9f);
        int length = pnlTopContent.childCount;
        for (int i = 0; i < length; i++)
            if (pnlTopContent.GetChild(i).TryGetComponent(out UIFade uIFade))
            {
                uIFade.FadeIn(1, 0.5f);
                yield return new WaitForSeconds(0.25f);
            }

        //yield return new WaitForSeconds(length * 0.5f);

        int length1 = pnlBottomContent.childCount;
        for (int i = 0; i < length1; i++)
            if (pnlBottomContent.GetChild(i).TryGetComponent(out UIFade uIFade))
            {
                uIFade.FadeIn(1, 0.5f);
                yield return new WaitForSeconds(0.25f);
            }
        journeyBeginsText.GetComponent<CanvasGroup>().alpha = 1;
        yield return StartCoroutine(textWaiting(journeyBeginsText.text, journeyBeginsText));
        yield return new WaitForSeconds(2f);
    }
    private IEnumerator textWaiting(string sentence, Text text)
    {
        text.text = string.Empty;
        foreach (char letter in sentence.ToCharArray())
        {
            text.text += letter;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();        
    }
}
