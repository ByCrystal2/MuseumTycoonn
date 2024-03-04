using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemManager : MonoBehaviour
{
    
    public List<ItemData> ShopItemDatas = new List<ItemData>();
    public List<ItemData> IAPItems = new List<ItemData>();
    public List<ItemData> RItems = new List<ItemData>(); //Random items
    public List<ItemData> DailyRewardItems = new List<ItemData>(); //Daily Reward items
    public List<ItemData> CurrentDailyRewardItems = new List<ItemData>(); //Daily Reward items
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
            Texture2D gemTexture = Resources.Load<Texture2D>("ItemPictures/ItemGem");
            Texture2D goldTexture = Resources.Load<Texture2D>("ItemPictures/ItemGold");
            #region Magaza Kismi
            ItemData item1 = new ItemData(1000, "Gold", "", 10000, 5, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item2 = new ItemData(1001, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gold, 5, 5);
            ItemData item3 = new ItemData(1002, "Vincent Van Gogh", "1784'de resmedilen ünlü eser.", 1, 150, null, ItemType.Table, ShoppingType.Gem, 4, 1);
            ItemData item4 = new ItemData(1003, "Gem", "", 100, 15.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item5 = new ItemData(1004, "Gem", "", 50, 75000, Resources.Load<Sprite>("ItemPictures/ItemGem500x").texture, ItemType.Gem, ShoppingType.Gold, 0);
            ItemData item6 = new ItemData(1005, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gem, 5, 4);
            ItemData item7 = new ItemData(1006, "Leonardo Da Vinci", "Þaþalý bir tablo", 1, 50, null, ItemType.Table, ShoppingType.Gem, 5, 3);
            ItemData item8 = new ItemData(1007, "Vincent Van Gogh", "Etkileyici Tablo", 1, 7.99f, null, ItemType.Table, ShoppingType.RealMoney, 5, 3);

            //DailyItems
            
            //Gem
            ItemData itemDaily1 = new ItemData(2000,"Gem","",15,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0,0,"IAP Urunu Degil",false,5);
            ItemData itemDaily2 = new ItemData(2001,"Gem","",20,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily3 = new ItemData(2002,"Gem","",25,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily4 = new ItemData(2003,"Gem","",30,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily5 = new ItemData(2004,"Gem","",35,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily6 = new ItemData(2005,"Gem","",50,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily7 = new ItemData(2006,"Gem","",60,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily8 = new ItemData(2007,"Gem","",70,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily9 = new ItemData(2008,"Gem","",80,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily10 = new ItemData(2009,"Gem","",90,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);

            //Gold
            ItemData itemDaily11 = new ItemData(2010,"Gold","",1000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily12 = new ItemData(2011,"Gold","",2000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily13 = new ItemData(2012,"Gold","",3000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily14 = new ItemData(2013,"Gold","",4000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily15 = new ItemData(2014,"Gold","",5000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 5);
            ItemData itemDaily16 = new ItemData(2015,"Gold","",10000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily17 = new ItemData(2016,"Gold","",20000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily18 = new ItemData(2017,"Gold","",30000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily19 = new ItemData(2018,"Gold","",40000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);
            ItemData itemDaily20 = new ItemData(2019,"Gold","",50000,0,gemTexture,ItemType.Gold,ShoppingType.DailyReward,0, 0, "IAP Urunu Degil", false, 10);

            //Table
            ItemData itemDaily21 = new ItemData(2020,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,1, "IAP Urunu Degil", false, 5);
            ItemData itemDaily22 = new ItemData(2021,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,2, "IAP Urunu Degil", false, 5);
            ItemData itemDaily23 = new ItemData(2022,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,3, "IAP Urunu Degil", false, 5);
            ItemData itemDaily24 = new ItemData(2023,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,4, "IAP Urunu Degil", false, 5);
            ItemData itemDaily25 = new ItemData(2024,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,3,5, "IAP Urunu Degil", false, 5);

            ItemData itemDaily26 = new ItemData(2025,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,1, "IAP Urunu Degil", false, 10);
            ItemData itemDaily27 = new ItemData(2026,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,2, "IAP Urunu Degil", false, 10);
            ItemData itemDaily28 = new ItemData(2027,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,3, "IAP Urunu Degil", false, 10);
            ItemData itemDaily29 = new ItemData(2028,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,3,4, "IAP Urunu Degil", false, 10);
            ItemData itemDaily30 = new ItemData(2029,"Leonardo Da Vinci","",1,0,null,ItemType.Table,ShoppingType.DailyReward,4,5, "IAP Urunu Degil", false, 10);

            DailyRewardItems.Add(itemDaily1);
            DailyRewardItems.Add(itemDaily2);
            DailyRewardItems.Add(itemDaily3);
            DailyRewardItems.Add(itemDaily4);
            DailyRewardItems.Add(itemDaily5);
            DailyRewardItems.Add(itemDaily6);
            DailyRewardItems.Add(itemDaily7);
            DailyRewardItems.Add(itemDaily8);
            DailyRewardItems.Add(itemDaily9);
            DailyRewardItems.Add(itemDaily10);

            DailyRewardItems.Add(itemDaily11);
            DailyRewardItems.Add(itemDaily12);
            DailyRewardItems.Add(itemDaily13);
            DailyRewardItems.Add(itemDaily14);
            DailyRewardItems.Add(itemDaily15);
            DailyRewardItems.Add(itemDaily16);
            DailyRewardItems.Add(itemDaily17);
            DailyRewardItems.Add(itemDaily18);
            DailyRewardItems.Add(itemDaily19);
            DailyRewardItems.Add(itemDaily20);

            DailyRewardItems.Add(itemDaily21);
            DailyRewardItems.Add(itemDaily22);
            DailyRewardItems.Add(itemDaily23);
            DailyRewardItems.Add(itemDaily24);
            DailyRewardItems.Add(itemDaily25);
            DailyRewardItems.Add(itemDaily26);
            DailyRewardItems.Add(itemDaily27);
            DailyRewardItems.Add(itemDaily28);
            DailyRewardItems.Add(itemDaily29);
            DailyRewardItems.Add(itemDaily30);

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
    public (List<ItemData> GoldItems, List<ItemData> GemItems, List<ItemData> TableItems) CalculateDailyItems()
    {
        List<ItemData> randomItems = new List<ItemData>();
        int length = DailyRewardItems.Count;
        for (int i = 0; i < length; i++)
        {
            ItemData _newItem = new ItemData();
            _newItem = GetDailyRandomItem();
            randomItems.Add(_newItem);
        }
        byte maxGoldItemCount = (byte)Random.Range(3,5), maxGemItemCount = (byte)(6 - maxGoldItemCount), maxTabletemCount = 1, currentGoldItemCount = 0, currentGemItemCount=0, currentTableItemCount = 0;
        if (maxGoldItemCount+maxGemItemCount+maxTabletemCount != 7)
        {
            Debug.Log($"MaxItemCounts degeri:({maxGoldItemCount + maxGemItemCount + maxTabletemCount}) => 7'ye esit olmali.");
            return (new List<ItemData>(), new List<ItemData>(), new List<ItemData>());
        }
        List<ItemData> currentItems = new List<ItemData>();
        foreach (var item in randomItems)
        {
            if (item.CurrentItemType == ItemType.Gold && currentGoldItemCount < maxGoldItemCount )
            {
                currentItems.Add(item);
                Debug.Log("Before" + item.ID);
                currentGoldItemCount++;
            }
            else if (item.CurrentItemType == ItemType.Gem && currentGemItemCount < maxGemItemCount)
            {
                currentItems.Add(item);
                Debug.Log("Before" + item.ID);
                currentGemItemCount++;
            }
            else if (item.CurrentItemType == ItemType.Table && currentTableItemCount < maxTabletemCount)
            {
                currentItems.Add(item);
                Debug.Log("Before" + item.ID);
                currentTableItemCount++;
            }

            if (currentItems.Count == 7)
                break;
        }
        var goldItems = currentItems.Where(x => x.CurrentItemType == ItemType.Gold).OrderBy(x => x.Amount).ToList();
        var gemItems = currentItems.Where(x => x.CurrentItemType == ItemType.Gem).OrderBy(x => x.Amount).ToList();
        var tableItems = currentItems.Where(x => x.CurrentItemType == ItemType.Table).ToList();

        return (goldItems, gemItems, tableItems);
    }
    public ItemData GetDailyRandomItem()
    {
        List<ItemData> currentCultureLevelItems = new List<ItemData>();
        byte LevelControl = (MuseumManager.instance.GetCurrentCultureLevel()) switch
        {
            <=5 => 5,
            <= 10 => 10,
            <= 15 => 15,
            _ => 15,
        };
        currentCultureLevelItems = DailyRewardItems.Where(x=> x.FocusedLevel == LevelControl).ToList();
        return currentCultureLevelItems[Random.Range(0, currentCultureLevelItems.Count)];
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
    public void SetCalculatedDailyRewardItems()
    {
        CurrentDailyRewardItems.Clear();
        (List<ItemData> Golds, List<ItemData> Gems, List<ItemData> Tables) Items = CalculateDailyItems();
        int maxItemCount = System.Math.Max(Items.Golds.Count, Items.Gems.Count);

        for (int i = 0; i < maxItemCount; i++)
        {
            if (i < Items.Golds.Count)
            {
                var gold = Items.Golds[i];
                CurrentDailyRewardItems.Add(gold);
                Debug.Log($"{gold.ID} ID'li - {gold.CurrentItemType} item Turlu - {gold.CurrentShoppingType} item Shop turlu - {gold.Amount} - {gold.Name} Adli item eklendi!");
            }

            if (i < Items.Gems.Count)
            {
                var gem = Items.Gems[i];
                CurrentDailyRewardItems.Add(gem);
                Debug.Log($"{gem.ID} ID'li - {gem.CurrentItemType} item Turlu - {gem.CurrentShoppingType} item Shop turlu - {gem.Amount} - {gem.Name} Adli item eklendi!");
            }
        }
        foreach (var item in Items.Tables)
        {
            CurrentDailyRewardItems.Add(item);
            Debug.Log($"{item.ID} ID'li - {item.CurrentItemType} item Turlu - {item.CurrentShoppingType} item Shop turlu - {item.Amount} - {item.Name} Adli item eklendi!");
        }
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
    RealMoney,
    DailyReward
}
