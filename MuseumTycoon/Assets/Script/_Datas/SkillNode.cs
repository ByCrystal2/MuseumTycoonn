using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNode
{
    public int ID;
    public string SkillName;
    public string SkillDescription;
    public string SkillEffect;
    public float SkillRequiredLevel;
    public float SkillRequiredMoney;
    public bool IsLocked = true;

    public SkillNode(int _id, string _skillName, string _skillDescription, string _skillEffect, float _skillRquiredLevel, float skillRequiredMoney)
    {
        this.ID = _id;
        this.SkillName = _skillName;
        this.SkillDescription = _skillDescription;
        this.SkillEffect = _skillEffect;
        this.SkillRequiredLevel = _skillRquiredLevel;
        this.SkillRequiredMoney = skillRequiredMoney;
    }

    public SkillNode(SkillNode skillNode)
    {
        this.ID = skillNode.ID;
        this.SkillName = skillNode.SkillName;
        this.SkillDescription = skillNode.SkillDescription;
        this.SkillEffect = skillNode.SkillEffect;
        this.SkillRequiredLevel = skillNode.SkillRequiredLevel;
        this.SkillRequiredMoney= skillNode.SkillRequiredMoney;
        this.IsLocked = skillNode.IsLocked;
    }

    public bool IsMoneyAndLevelEnough(float _money, float _level)
    {
        if (_money >= SkillRequiredMoney && _level >= SkillRequiredLevel)  
            return true;        
        else 
            return false; 
        
    }

    public void Lock(bool _isLock)
    {
        IsLocked = _isLock;
    }
}
