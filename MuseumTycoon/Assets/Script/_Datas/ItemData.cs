using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ItemData
{
    public int ID;
    public string IAP_ID;
    public string Name;
    public string Description;
    public float Amount;
    public float RequiredMoney;
    public Sprite ImageSprite;
    public ItemType CurrentItemType;
    public ShoppingType CurrentShoppingType;
    public byte StarCount;
    public int textureID;
    public bool IsPurchased;
    //Ýndirim eklenebilir.

    // Orn: ItemType Gold,5000 adet, ShoppingType RealMoney, RequiredMoney 10TL
    public ItemData(int _id, string _name, string _description, float _amount, float _requiredMoney, Texture2D _imageSprite, ItemType _itemType, ShoppingType _shoppingType, byte _starCount, int _textureID = 0, string _iAPId = "IAP Urunu Degil.", bool _isPurchased = false)
    {
        ID = _id;
        Name = _name;
        IAP_ID = _iAPId;
        if (_shoppingType == ShoppingType.RealMoney)
            IAP_ID = Constant.instance.IAPIDCompany + Constant.instance.IAPIDGame + _itemType.ToString().ToLower() + "x" + _amount.ToString() + "_" + _shoppingType.ToString().ToLower() + "_" + ((int)_requiredMoney).ToString(); //com_kosippysudio_museumtycoon_gold5000x_realmoney_10
        Description = _description;
        RequiredMoney = _requiredMoney;
        Amount = _amount;
        Debug.Log("texture id for item creating: " + _textureID);
        if (_itemType == ItemType.Table)
            ImageSprite = CatchTheColors.instance.TextureToSprite(MuseumManager.instance.GetPictureElementData(_textureID).texture);
        else
            ImageSprite = CatchTheColors.instance.TextureToSprite(_imageSprite);
        CurrentItemType = _itemType;
        CurrentShoppingType = _shoppingType;
        StarCount = _starCount;
        textureID = _textureID;
        IsPurchased = _isPurchased;

    }

    public ItemData(ItemData _item)
    {
        ID = _item.ID;
        Name = _item.Name;
        IAP_ID = _item.IAP_ID;
        Description = _item.Description;
        RequiredMoney = _item.RequiredMoney;
        Amount = _item.Amount;
        ImageSprite = _item.ImageSprite;
        CurrentItemType = _item.CurrentItemType;
        CurrentShoppingType = _item.CurrentShoppingType;
        StarCount = _item.StarCount;
        textureID = _item.textureID;
        IsPurchased = _item.IsPurchased;
    }
   
}
