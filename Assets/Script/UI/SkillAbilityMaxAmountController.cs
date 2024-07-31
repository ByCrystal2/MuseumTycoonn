using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillAbilityMaxAmountController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI myText;
    public void SetSkillMaxLevelUI(int _currentLevel)
    {        
        myText.text = _currentLevel.ToString();
    }
}
