using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillAbilityAmountController : MonoBehaviour
{
    public TextMeshProUGUI abilityText;
    private void Awake()
    {
        abilityText = GetComponent<TextMeshProUGUI>();
    }
    public void IncreasingAbilityAmount()
    {
        int currentSkillAbilityAmount = int.Parse(abilityText.text);
        currentSkillAbilityAmount++;
        abilityText.text = currentSkillAbilityAmount.ToString();
    }
   
}
