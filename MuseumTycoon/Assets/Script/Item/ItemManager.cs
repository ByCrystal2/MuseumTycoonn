using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemManager : MonoBehaviour
{
    
    public List<ItemData> ShopItemDatas = new List<ItemData>();
    public List<ItemData> IAPItems = new List<ItemData>();
    public List<ItemData> RItems = new List<ItemData>(); //Random items
    public static ItemManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }
    public void AddItems()
    {
        // ADD Items
        List<Texture2D> TableTextures = MuseumManager.instance.GetPicturesTexture();
        if (ShopItemDatas == null || ShopItemDatas.Count == 0)
        {
            #region Magaza Kismi
            ItemData item1 = new ItemData(1000, "Gold", "", 10000, 5, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item2 = new ItemData(1001, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gold, 5, 5);
            ItemData item3 = new ItemData(1002, "Vincent Van Gogh", "1784'de resmedilen ünlü eser.", 1, 150, null, ItemType.Table, ShoppingType.Gem, 4, 1);
            ItemData item4 = new ItemData(1003, "Gem", "", 100, 15.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item5 = new ItemData(1004, "Gem", "", 50, 75000, Resources.Load<Sprite>("ItemPictures/ItemGem500x").texture, ItemType.Gem, ShoppingType.Gold, 0);
            ItemData item6 = new ItemData(1005, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gem, 5, 4);
            ItemData item7 = new ItemData(1006, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gem, 5, 3);
            ItemData item8 = new ItemData(1007, "Vincent Van Gogh", "Etkileyici Tablo", 1, 7.99f, null, ItemType.Table, ShoppingType.RealMoney, 5, 3);
            //ItemData item5 = new ItemData(1004, "Gem", "Þok Fiyata!", 200, 29.99f, "", ItemType.Gem, ShoppingType.RealMoney);
            //ItemData item6 = new ItemData(1005, "Gem", "Al-Ver", 5, 25000, "", ItemType.Gem, ShoppingType.Gold);

            //Add ItemDatas
            ShopItemDatas.Add(item1);
            ShopItemDatas.Add(item2);
            ShopItemDatas.Add(item3);
            ShopItemDatas.Add(item4);
            ShopItemDatas.Add(item5);
            ShopItemDatas.Add(item6);
            ShopItemDatas.Add(item7);
            ShopItemDatas.Add(item8);
            #endregion

            #region Randomize Itemler

            ItemData rItem1 = new ItemData(2000, "Fatmagul Burak", "Super Tablo", 1, 150, null, ItemType.Table, ShoppingType.Gem, 5, 4);
            ItemData rItem2 = new ItemData(2001, "Ahmet Burak", "Tablo", 1, 2000, null, ItemType.Table, ShoppingType.Gold, 4, 2);
            ItemData rItem3 = new ItemData(2002, "Mehmet Gok", "Tablo1", 1, 13.99f, null, ItemType.Table, ShoppingType.RealMoney, 3, 3);
            ItemData rItem4 = new ItemData(2003, "Koray Erdun", "Tablo2", 1, 1000, null, ItemType.Table, ShoppingType.Gold, 2, 1);
            ItemData rItem5 = new ItemData(2004, "Mertcan Gok", "Tablo3", 1, 31, null, ItemType.Table, ShoppingType.Gem, 1, 5);

            RItems.Add(rItem1);
            RItems.Add(rItem2);
            RItems.Add(rItem3);
            RItems.Add(rItem4);
            RItems.Add(rItem5);

            #endregion
        }
        else
        {
            List<ItemData> purchControlItems = new List<ItemData>();
            foreach (var item in ShopItemDatas)
            {
                if (!item.IsPurchased)
                {
                    purchControlItems.Add(item);
                }
            }
            foreach (var item in purchControlItems)
            {
                ShopItemDatas.Add(item);
            }
        }

        List<ItemData> _iapItems = new List<ItemData>();
        _iapItems = GetShoppingTypeItems(ShoppingType.RealMoney);
        foreach (ItemData item in _iapItems)
        {
            IAPItems.Add(item);
        }
    }

    public void AddItemInShop(ItemData _item)
    {
        GetAllItemDatas().Add(_item);
    }
    public List<ItemData> GetShoppingTypeItems(ShoppingType _shoppingType)
    {
        return ShopItemDatas.Where(x=> x.CurrentShoppingType == _shoppingType).ToList(); //coppyItems;

        /*
         * ItemDatas[0] => item1 ==> id, name, description .... ShopppingType.Gem List<ItemData> copyItems;  copyItems.Add(item1);
         * ItemDatas[1] => item2 ==> id, name, description .... ShopppingType.Table
         * ItemDatas[2] => item3 ==> id, name, description .... ShopppingType.Gem copyItems.Add(item3);
         * ItemDatas[3] => item4 ==> id, name, description .... ShopppingType.Gold
         
         */
    }

    public List<ItemData> GetItemTypeItems(ItemType _itemType)
    {
        Debug.Log(_itemType + "Onaylandý ve bu türde olanlar listelendi.");
        return ShopItemDatas.Where(x => x.CurrentItemType == _itemType).ToList(); //coppyItems;

        /*
         * ItemDatas[0] => item1 ==> id, name, description .... ShopppingType.Gem List<ItemData> copyItems;  copyItems.Add(item1);
         * ItemDatas[1] => item2 ==> id, name, description .... ShopppingType.Table
         * ItemDatas[2] => item3 ==> id, name, description .... ShopppingType.Gem copyItems.Add(item3);
         * ItemDatas[3] => item4 ==> id, name, description .... ShopppingType.Gold
         
         */
    }
    public List<ItemData> GetAllItemDatas()
    {
        return ShopItemDatas;
    }


    public List<ItemData> GetAllIAPItemDatas()
    {
        return IAPItems;
    }

}
public enum ItemType
{
    None,
    Gem,
    Gold,
    Table,
    All
}

public enum ShoppingType
{
    None,
    Gem,
    Gold,
    RealMoney
}
