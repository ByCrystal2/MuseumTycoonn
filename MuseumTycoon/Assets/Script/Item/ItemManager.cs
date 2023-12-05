using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemManager : MonoBehaviour
{
    
    public List<ItemData> ItemDatas = new List<ItemData>();
    public List<ItemData> CurrentItemDatas = new List<ItemData>();
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
        ItemData item1 = new ItemData(1000, "Gold", "", 10000, 5, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem,0);
        ItemData item2 = new ItemData(1001, "Leonardo Da Vinci", "Þaþalý bir tablo", 0, 50, TableTextures[Random.Range(0, TableTextures.Count)], ItemType.Table, ShoppingType.RealMoney, 5);
        ItemData item3 = new ItemData(1002, "Vincent Van Gogh", "1784'de resmedilen ünlü eser.", 0, 150, TableTextures[Random.Range(0, TableTextures.Count)], ItemType.Table, ShoppingType.Gem,4);
        ItemData item4 = new ItemData(1003, "Gem", "", 100, 15.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney,0);
        ItemData item5 = new ItemData(1004, "Gem", "", 75, 75000, Resources.Load<Sprite>("ItemPictures/ItemGold50000x").texture, ItemType.Gem, ShoppingType.Gold,0);
        //ItemData item5 = new ItemData(1004, "Gem", "Þok Fiyata!", 200, 29.99f, "", ItemType.Gem, ShoppingType.RealMoney);
        //ItemData item6 = new ItemData(1005, "Gem", "Al-Ver", 5, 25000, "", ItemType.Gem, ShoppingType.Gold);

        //Add ItemDatas
        ItemDatas.Add(item1);
        ItemDatas.Add(item2);
        ItemDatas.Add(item3);
        ItemDatas.Add(item4);
        ItemDatas.Add(item5);
        //ItemDatas.Add(item5);
        //ItemDatas.Add(item6);
    }
    public List<ItemData> GetShoppingTypeItems(ShoppingType _shoppingType)
    {
        Debug.Log(_shoppingType + "Onaylandý ve bu türde olanlar listelendi.");
        return ItemDatas.Where(x=> x.CurrentShoppingType == _shoppingType).ToList(); //coppyItems;

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
        return ItemDatas.Where(x => x.CurrentItemType == _itemType).ToList(); //coppyItems;

        /*
         * ItemDatas[0] => item1 ==> id, name, description .... ShopppingType.Gem List<ItemData> copyItems;  copyItems.Add(item1);
         * ItemDatas[1] => item2 ==> id, name, description .... ShopppingType.Table
         * ItemDatas[2] => item3 ==> id, name, description .... ShopppingType.Gem copyItems.Add(item3);
         * ItemDatas[3] => item4 ==> id, name, description .... ShopppingType.Gold
         
         */
    }
    public List<ItemData> GetAllItemDatas()
    {
        return ItemDatas;
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
