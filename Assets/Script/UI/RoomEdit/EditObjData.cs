using System.Collections;
using System.Collections.Generic;
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
    public List<(EditObjBonusType _bonusses, int _value)> BonusEnums = new List<(EditObjBonusType _bonusses, int _value)>();
    public RoomData _currentRoom;
    public int myStatueIndex;

    public bool isClickable;

    public EditObjData()
    {
        
    }
    public EditObjData(int _id, string _name,float _price,Texture2D _resourcesTexture , EditObjType _objType, List<(EditObjBonusType _bonusses, int _value)> Bonusses_Values,int _focusedLevel, int _myStatueIndex = -1, bool _isLocked = true)
    {
        ID = _id;
        Name = _name;
        Debug.Log(_name + " => _resourcesTexture.name => " + _resourcesTexture.name);
        Price = _price;
        ImageSprite = CatchTheColors.instance.TextureToSprite(_resourcesTexture);
        EditType = _objType;
        if (Bonusses_Values.Count > 0)
        {
            BonusEnums.Clear();
            int length = Bonusses_Values.Count;
            for (int i = 0; i < length; i++)
            {
                Debug.Log("Bonusses_Values[i]._bonusses.ToString() + Bonusses_Values[i]._bonusses.ToString() => " + Bonusses_Values[i]._bonusses.ToString() +" "+ Bonusses_Values[i]._value.ToString());
                BonusEnums.Add(Bonusses_Values[i]);
            }
            int length1 = BonusEnums.Count;
            for (int i = 0; i < length1; i++)
            {
                Debug.Log("BonusEnums[i]._bonusses.ToString() + BonusEnums[i]._value.ToString() => " + BonusEnums[i]._bonusses.ToString() +" "+ BonusEnums[i]._value.ToString());
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
        BonusEnums.Clear();
        int length = _newData.BonusEnums.Count;
        for (int i = 0; i < length; i++)
        {
            Debug.Log("Bonusses_Values[i]._bonusses.ToString() + Bonusses_Values[i]._bonusses.ToString() => " + _newData.BonusEnums[i]._bonusses.ToString() + " " + _newData.BonusEnums[i]._value.ToString());
            BonusEnums.Add(_newData.BonusEnums[i]);
        }
        int length1 = BonusEnums.Count;
        for (int i = 0; i < length1; i++)
        {
            Debug.Log("BonusEnums[i]._bonusses.ToString() + BonusEnums[i]._value.ToString() => " + BonusEnums[i]._bonusses.ToString() + " " + BonusEnums[i]._value.ToString());
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
            _currentRoom.isHasStatue = true;
            int length = BonusEnums.Count;
            for (int i = 0; i < length; i++)
            {
                Debug.Log(BonusEnums[i]._bonusses.ToString());
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
public class Bonus<T>
{
    public T Value { get; set; } // Bonus deðeri
    public Bonus(T value)
    {
        Value = value;
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
