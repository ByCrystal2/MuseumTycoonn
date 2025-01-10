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
    public int SkillCurrentLevel = 0;
    public int SkillMaxLevel;    
    public bool IsLocked = true;
    public bool IsPurchased = false;

    public List<eStat> buffs = new List<eStat>();
    public List<int> Amounts = new List<int>();
    public string defaultEffectString;
    public SkillNode(int _id, string _skillName, string _skillDescription, string _skillEffect, float _skillRquiredPoint, float skillRequiredMoney, int _skillMaxLevel, List<eStat> _buffs, List<int> _amounts)
    {
        this.ID = _id;
        this.SkillName = _skillName;
        this.SkillDescription = _skillDescription;
        this.SkillEffect = _skillEffect;
        defaultEffectString = _skillEffect;
        this.SkillRequiredPoint = _skillRquiredPoint;
        this.SkillRequiredMoney = skillRequiredMoney;
        this.SkillMaxLevel = _skillMaxLevel;
        buffs.Clear();
        Amounts.Clear();
        int length = _buffs.Count;
        for (int i = 0; i < length; i++)
        {
            buffs.Add(_buffs[i]);
        }
        int length2 = _amounts.Count;
        for (int i = 0; i < length2; i++)
        {
            Amounts.Add(_amounts[i]);
        }
    }

    public SkillNode(SkillNode skillNode)
    {
        this.ID = skillNode.ID;
        this.SkillName = skillNode.SkillName;
        this.SkillDescription = skillNode.SkillDescription;
        this.SkillEffect = skillNode.SkillEffect;
        defaultEffectString = skillNode.SkillEffect;
        this.SkillRequiredPoint = skillNode.SkillRequiredPoint;
        this.SkillRequiredMoney= skillNode.SkillRequiredMoney;
        this.SkillCurrentLevel = skillNode.SkillCurrentLevel;
        this.SkillMaxLevel = skillNode.SkillMaxLevel;
        this.IsLocked = skillNode.IsLocked;
        this.IsPurchased = skillNode.IsPurchased;

        buffs.Clear();
        Amounts.Clear();
        int length = skillNode.buffs.Count;
        for (int i = 0; i < length; i++)
        {
            buffs.Add(skillNode.buffs[i]);
        }
        int length2 = skillNode.Amounts.Count;
        for (int i = 0; i < length2; i++)
        {
            Amounts.Add(skillNode.Amounts[i]);
        }
    }
    public SkillNode() { }
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
        if (_isPurchased)
        {
            SkillCurrentLevel++;
            if (SkillCurrentLevel < SkillMaxLevel && Amounts.Count >= SkillCurrentLevel)
                this.SkillEffect = $"+{this.Amounts[SkillCurrentLevel]} {defaultEffectString}";
            else
                Debug.LogError($"Mevcut {SkillCurrentLevel} levelli skill'in, {Amounts.Count} adet buff asamasý vardir!");
        }
    }
    
}
