using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillAbilityMaxAmountController : MonoBehaviour
{
    TextMeshProUGUI myText;
    private void Awake()
    {
        
    }
    public void SetSkillMaxLevelUI(int _currentLevel)
    {
        if (myText == null)        
            myText = GetComponent<TextMeshProUGUI>();
        myText.text = _currentLevel.ToString();
    }
}
