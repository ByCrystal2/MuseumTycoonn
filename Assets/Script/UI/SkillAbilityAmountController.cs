using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillAbilityAmountController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI abilityText;
    public void IncreasingAbilityAmount()
    {
        int currentSkillAbilityAmount = int.Parse(abilityText.text);        
        currentSkillAbilityAmount++;
        abilityText.text = currentSkillAbilityAmount.ToString();        
    }
    public void SetSkillCurrentLevelUI(int _currentLevel)
    {        
        abilityText.text = _currentLevel.ToString();
    }
}