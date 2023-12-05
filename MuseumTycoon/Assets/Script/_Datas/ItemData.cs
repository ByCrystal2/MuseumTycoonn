using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemData
{
    public int ID;
    public string Name;
    public string Description;
    public float Amount;
    public float RequiredMoney;
    public Sprite ImageSprite;
    public ItemType CurrentItemType;
    public ShoppingType CurrentShoppingType;
    public byte StarCount;

    public ItemData(int _id, string _name, string _description, float _amount, float _requiredMoney, Texture2D _imageSprite, ItemType _itemType, ShoppingType _shoppingType, byte _starCount)
    {
        ID = _id;
        Name = _name;
        Description = _description;
        RequiredMoney = _requiredMoney;
        Amount = _amount;
        ImageSprite = CatchTheColors.instance.TextureToSprite(_imageSprite);
        CurrentItemType = _itemType;
        CurrentShoppingType = _shoppingType;
        StarCount = _starCount;
    }

    public ItemData(ItemData _item)
    {
        ID = _item.ID;
        Name = _item.Name;
        Description = _item.Description;
        RequiredMoney = _item.RequiredMoney;
        Amount = _item.Amount;
        ImageSprite = _item.ImageSprite;
        CurrentItemType = _item.CurrentItemType;
        CurrentShoppingType = _item.CurrentShoppingType;
        StarCount = _item.StarCount;
    }
   
}
