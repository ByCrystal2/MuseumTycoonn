using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class EditObjData
{
    public int ID;
    public string Name;
    public float Price;
    public bool IsPurchased;
    public bool IsLocked;
    public int FocusedLevel;
    public Sprite ImageSprite;
    public EditObjType EditType;
    public List<Bonus> Bonusses = new List<Bonus>();
    public RoomCell _currentRoomCell;
    public int myStatueIndex;

    public bool isClickable;

    public EditObjData()
    {
        
    }
    public EditObjData(int _id, string _name,float _price,Texture2D _resourcesTexture , EditObjType _objType, List<Bonus> _Bonusses,int _focusedLevel, int _myStatueIndex = -1, bool _isLocked = true)
    {
        ID = _id;
        Name = _name;
        Debug.Log(_name + " => _resourcesTexture.name => " + _resourcesTexture.name);
        Price = _price;
        ImageSprite = CatchTheColors.instance.TextureToSprite(_resourcesTexture);
        EditType = _objType;
        if (_Bonusses.Count > 0)
        {
            Bonusses.Clear();
            int length = _Bonusses.Count;
            for (int i = 0; i < length; i++)
            {
                Debug.Log("_Bonusses[i].BonussesType + _Bonusses[i].Value => " + _Bonusses[i].BonusType.ToString() +" "+ _Bonusses[i].Value.ToString());
                Bonusses.Add(_Bonusses[i]);
            }            
        }
        FocusedLevel = _focusedLevel;
        IsLocked = _isLocked;
        myStatueIndex = _myStatueIndex;
    }
    public void UnLock()
    {
        IsLocked = false;
    }
    public EditObjData(EditObjData _newData)
    {
        ID = _newData.ID;
        Name = _newData.Name;
        Price = _newData.Price;
        ImageSprite = _newData.ImageSprite;
        EditType = _newData.EditType;
        Bonusses.Clear();
        int length = _newData.Bonusses.Count;
        for (int i = 0; i < length; i++)
        {
            Debug.Log("_Bonusses[i].BonussesType + _Bonusses[i].Value => " + _newData.Bonusses[i].BonusType.ToString() + " " + _newData.Bonusses[i].Value.ToString());
            Bonusses.Add(_newData.Bonusses[i]);
        }
        IsPurchased = _newData.IsPurchased;
        IsLocked = _newData.IsLocked;
        FocusedLevel = _newData.FocusedLevel;
    }
    public void SetIsPurchased()
    {
        if (EditType == EditObjType.None) return;
        IsPurchased = true;
        if (EditType == EditObjType.Statue)
        {
            RoomManager.instance.GetRoomWithRoomCell(_currentRoomCell).isHasStatue = true;
            int length = Bonusses.Count;
            for (int i = 0; i < length; i++)
            {
                Debug.Log(Bonusses[i].BonusType.ToString());
            }
            RoomManager.instance.statuesHandler.AddAndBonusCalculator(this);
        }
        else if (EditType == EditObjType.Decoration)
        {
            RoomManager.instance.statuesHandler.AddAndBonusCalculator(this);
        }
    }
    public bool GetIsPurchased()
    {
        return IsPurchased;
    }
    

}
[System.Serializable]
public class Bonus
{
    public EditObjBonusType BonusType;
    public int Value;
    public Bonus(EditObjBonusType _bonusType, int _value)
    {
        BonusType = _bonusType;
        Value = _value;
    }
}
public enum EditObjBonusType
{
    None,
    IncreaseNPCHappiness, // Npc mutluluk orani azaltma
    ReduceNpcFightRate, // Npc kavga orani azaltma
    ReduceNpcUrinationRate, // Npc pisleme orani azaltma
    NPCsMustToPay, // Npcler ödeme yapmali
    DancingNPCsEmerge, // Dans eden NPC'ler ortaya cikiyor
    CowGivingMilkToNPCs, // Npclere sut veren inek
    IfArtistPaintingInRoom // Odada tablosu olan sanatci varsa
}
