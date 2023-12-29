using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSkillOptions : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] public int SkillID;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(SkillID);
        UIController.instance.ShowSkillInfo(SkillID);
    }
}
