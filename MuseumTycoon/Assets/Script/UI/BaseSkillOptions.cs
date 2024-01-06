using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSkillOptions : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] public int SkillID;
    [SerializeField] private Transform[] MyPoints;
    [SerializeField] public SkillPoint MyPoint; 
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(SkillID);
        UIController.instance.ShowSkillInfo(SkillID);
    }
    public Transform GetMyCurrentPointTransform()
    {
        return MyPoints[(int)MyPoint];
    }

    public enum SkillPoint
    {
        Point1 = 0,
        Point2 = 1,
        Point3 = 2,
        Point4 = 3,
    }
}
