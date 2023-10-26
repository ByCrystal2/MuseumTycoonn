using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillNode
{
    public int ID;
    public string SkillName;
    public string SkillDescription;
    public string SkillEffect;
    public float SkillRequiredPoint;
    public float SkillRequiredMoney;
    public float SkillCurrentLevel = 0;
    public float SkillMaxLevel;
    public bool IsLocked = true;
    public bool IsPurchased = false;

    public SkillNode(int _id, string _skillName, string _skillDescription, string _skillEffect, float _skillRquiredPoint, float skillRequiredMoney, float _skillMaxLevel)
    {
        this.ID = _id;
        this.SkillName = _skillName;
        this.SkillDescription = _skillDescription;
        this.SkillEffect = _skillEffect;
        this.SkillRequiredPoint = _skillRquiredPoint;
        this.SkillRequiredMoney = skillRequiredMoney;
        this.SkillMaxLevel = _skillMaxLevel;
    }

    public SkillNode(SkillNode skillNode)
    {
        this.ID = skillNode.ID;
        this.SkillName = skillNode.SkillName;
        this.SkillDescription = skillNode.SkillDescription;
        this.SkillEffect = skillNode.SkillEffect;
        this.SkillRequiredPoint = skillNode.SkillRequiredPoint;
        this.SkillRequiredMoney= skillNode.SkillRequiredMoney;
        this.SkillCurrentLevel = skillNode.SkillCurrentLevel;
        this.SkillMaxLevel= skillNode.SkillMaxLevel;
        this.IsLocked = skillNode.IsLocked;
        this.IsPurchased = skillNode.IsPurchased;
    }

    public bool IsMoneyAndLevelEnough(float _money, float _point)
    {
        if (_money >= SkillRequiredMoney && _point >= SkillRequiredPoint)  
            return true;        
        else 
            return false; 
        
    }

    public void Lock(bool _isLock)
    {
        this.IsLocked = _isLock;
    }

    public void Purchased(bool _isPurchased)
    {
        this.IsPurchased = _isPurchased;
    }
}
