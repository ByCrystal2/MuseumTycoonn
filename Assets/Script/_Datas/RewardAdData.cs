using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RewardAdData
{
    public int ID;
    public int Amount;
    public ItemType Type;
    public int FocusedLevel;
    public int AdDuration;
    public bool IsPurchased;
    public RewardAdData(int _id, int _amount, ItemType _type, int _focusedLevel)
    {
        ID = _id;
        Amount = _amount;
        Type = _type;
        AdDuration = Random.Range(8, 20);
        IsPurchased = false;
        FocusedLevel = _focusedLevel;

    }
    public RewardAdData()
    {
        
    }
}