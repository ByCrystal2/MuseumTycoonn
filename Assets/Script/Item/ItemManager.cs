using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemManager : MonoBehaviour
{
    
    public List<ItemData> ShopItemDatas = new List<ItemData>();
    public List<ItemData> IAPItems = new List<ItemData>();
    public List<ItemData> RItems = new List<ItemData>(); //Random items
    public List<ItemData> DailyRewardItems = new List<ItemData>(); // 30 adet gunluk odul ogesi iceren liste.
    public List<ItemData> CurrentDailyRewardItems = new List<ItemData>(); // mevcut gunluk odul ogesi listesi (max 7)
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
            //NoAds
            ItemData item0 = new ItemData(10000, "No Ads", "Reklamlarý Kaldýr", 1, 39.99f, Resources.Load<Texture2D>("ItemPictures/No_Ads"), ItemType.Ads, ShoppingType.RealMoney, 0);
            //NoAds
            ItemData item1 = new ItemData(1000, "Gold", "", 8000, 50, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item2 = new ItemData(1001, "Gold", "", 16000, 85, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item3 = new ItemData(1002, "Gold", "", 30000, 150, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item4 = new ItemData(1003, "Gold", "", 100000, 500, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            ItemData item5 = new ItemData(1004, "Gold", "", 250000, 1100, Resources.Load<Texture2D>("ItemPictures/ItemGold10000x"), ItemType.Gold, ShoppingType.Gem, 0);
            
            //Real Money
            ItemData item1000 = new ItemData(10001, "Gem", "", 55, 16.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1001 = new ItemData(10002, "Gem", "", 130, 35.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1002 = new ItemData(10003, "Gem", "", 255, 69.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1003 = new ItemData(10004, "Gem", "", 520, 134.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1004 = new ItemData(10005, "Gem", "", 1100, 229.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1005 = new ItemData(10006, "Gem", "", 2700, 709.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            ItemData item1006 = new ItemData(10007, "Gem", "", 5500, 1449.99f, Resources.Load<Sprite>("ItemPictures/ItemGem100x").texture, ItemType.Gem, ShoppingType.RealMoney, 0);
            //Real Money

            //Star Count 1
            ItemData itemTutorialGift = new ItemData(9999, "King of The Museum", "Kral'ýn hediyesi", 1, 0, null, ItemType.Table, ShoppingType.Gold, 2, 2);
            ItemData item6 = new ItemData(1005, "Frida Kahlo", "Yýpranmýþ bir tablo", 1, 500, null, ItemType.Table, ShoppingType.Gold, 1, 4);
            ItemData item7 = new ItemData(1006, "Käthe Kollwitz", "1985'te resmedilen bir tablo", 1, 1000, null, ItemType.Table, ShoppingType.Gold, 1, 3);
            ItemData item8 = new ItemData(1007, "Gabriele Münter", "Kýzýl nehir tablo", 1, 1500, null, ItemType.Table, ShoppingType.Gold, 1, 1);
            ItemData item9 = new ItemData(1008, "Michelangelo Buonarroti", "Eski bir tablo", 1, 2000, null, ItemType.Table, ShoppingType.Gold, 1, 2);
            ItemData item10 = new ItemData(1009, "Georges Seurat", "Yýpranmýþ bir tablo", 1, 2500, null, ItemType.Table, ShoppingType.Gold, 1, 3);
            //Star Count 2
            ItemData item11 = new ItemData(1010, "Edgar Degas", "Yýpranmýþ bir tablo", 1, 2000, null, ItemType.Table, ShoppingType.Gold, 2, 2);
            ItemData item12 = new ItemData(1011, "Sofonisba Anguissola", "1985'te resmedilen bir tablo", 1, 2500, null, ItemType.Table, ShoppingType.Gold, 2, 1);
            ItemData item13 = new ItemData(1012, "Rembrandt van Rijn", "Kýzýl nehir tablo", 1, 3000, null, ItemType.Table, ShoppingType.Gold, 2, 4);
            ItemData item14 = new ItemData(1013, "Michelangelo Buonarroti", "Eski bir tablo", 1, 3500, null, ItemType.Table, ShoppingType.Gold, 2, 2);
            ItemData item15 = new ItemData(1014, "Paula Modersohn-Becker", "Yýpranmýþ bir tablo", 1, 4000, null, ItemType.Table, ShoppingType.Gold, 2, 1);
            ItemData item16 = new ItemData(1015, "Georges Seurat", "Yýpranmýþ bir tablo", 1, 5000, null, ItemType.Table, ShoppingType.Gold, 2, 3);
            //Star Count 3
            ItemData item17 = new ItemData(1016, "Frida Kahlo", "Yýpranmýþ bir tablo", 1, 5000, null, ItemType.Table, ShoppingType.Gold, 3, 2);
            ItemData item18 = new ItemData(1017, "Käthe Kollwitz", "1985'te resmedilen bir tablo", 1, 7000, null, ItemType.Table, ShoppingType.Gold, 3, 1);
            ItemData item19 = new ItemData(1018, "Leonardo da Vinci", "Kýzýl nehir tablo", 1, 7500, null, ItemType.Table, ShoppingType.Gold, 3, 3);
            ItemData item20 = new ItemData(1019, "Michelangelo Buonarroti", "Eski bir tablo", 1, 8000, null, ItemType.Table, ShoppingType.Gold, 3, 4);
            ItemData item21 = new ItemData(1020, "Leonardo da Vinci", "Yýpranmýþ bir tablo", 1, 10000, null, ItemType.Table, ShoppingType.Gold, 3, 2);
            //Star Count 4
            ItemData item22 = new ItemData(1021, "Helen Frankenthaler", "Yýpranmýþ bir tablo", 1, 10000, null, ItemType.Table, ShoppingType.Gold, 4, 2);
            ItemData item23 = new ItemData(1022, "Leonardo da Vinci", "1985'te resmedilen bir tablo", 1, 15000, null, ItemType.Table, ShoppingType.Gold, 4, 3);
            ItemData item24 = new ItemData(1023, "Vincent van Gogh", "Kýzýl nehir tablo", 1, 20000, null, ItemType.Table, ShoppingType.Gold, 4, 1);
            ItemData item25 = new ItemData(1024, "Michelangelo Buonarroti", "Eski bir tablo", 1, 25000, null, ItemType.Table, ShoppingType.Gold, 4, 3);
            ItemData item26 = new ItemData(1025, "Vincent van Gogh", "Yýpranmýþ bir tablo", 1, 30000, null, ItemType.Table, ShoppingType.Gold, 4, 4);
            //Star Count 5
            ItemData item27 = new ItemData(1026, "Leonardo da Vinci", "Yýpranmýþ bir tablo", 1, 45000, null, ItemType.Table, ShoppingType.Gold, 5, 2);
            ItemData item28 = new ItemData(1027, "Artemisia Gentileschi", "1985'te resmedilen bir tablo", 1, 60000, null, ItemType.Table, ShoppingType.Gold, 5, 1);

            //DailyItems
            //Gem
            ItemData itemDaily1 = new ItemData(2000,"Gem","",15,0,gemTexture,ItemType.Gem,ShoppingType.DailyReward,0,0,"IAP Urunu Degil", false, 5);
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
            ItemData itemDaily21 = new ItemData(2020, "Leonardo da Vinci", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,1, "IAP Urunu Degil", false, 5);
            ItemData itemDaily22 = new ItemData(2021, "Henri Matisse", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,2, "IAP Urunu Degil", false, 5);
            ItemData itemDaily23 = new ItemData(2022, "Paul Cezanne", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,1,3, "IAP Urunu Degil", false, 5);
            ItemData itemDaily24 = new ItemData(2023, "Elisabeth Vigée Le Brun", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,4, "IAP Urunu Degil", false, 5);
            ItemData itemDaily25 = new ItemData(2024, "Hilma af Klint", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,3,5, "IAP Urunu Degil", false, 5);

            ItemData itemDaily26 = new ItemData(2025, "Leonardo da Vinci", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,1, "IAP Urunu Degil", false, 10);
            ItemData itemDaily27 = new ItemData(2026, "Albrecht Durer", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,2, "IAP Urunu Degil", false, 10);
            ItemData itemDaily28 = new ItemData(2027, "Vincent van Gogh", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,2,3, "IAP Urunu Degil", false, 10);
            ItemData itemDaily29 = new ItemData(2028, "Claude Monet", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,3,4, "IAP Urunu Degil", false, 10);
            ItemData itemDaily30 = new ItemData(2029, "Leonardo da Vinci", "",1,0,null,ItemType.Table,ShoppingType.DailyReward,4,5, "IAP Urunu Degil", false, 10);

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

            Debug.Log("DailyRewardItems.Count => " + DailyRewardItems.Count);

            //Add ItemDatas
            //NoAds
            ShopItemDatas.Add(item0);
            //NoAds

            //Gold
            ShopItemDatas.Add(item1);
            ShopItemDatas.Add(item2);
            ShopItemDatas.Add(item3);
            ShopItemDatas.Add(item4);
            ShopItemDatas.Add(item5);
            //Gold

            //RealMoneyShoppingItemsAdding
            ShopItemDatas.Add(item1000);
            ShopItemDatas.Add(item1001);
            ShopItemDatas.Add(item1002);
            ShopItemDatas.Add(item1003);
            ShopItemDatas.Add(item1004);
            ShopItemDatas.Add(item1005);
            ShopItemDatas.Add(item1006);
            //RealMoneyShoppingItemsAdding

            //Normal Items
            ShopItemDatas.Add(itemTutorialGift);
            ShopItemDatas.Add(item6);
            ShopItemDatas.Add(item7);
            ShopItemDatas.Add(item8);
            ShopItemDatas.Add(item9);
            ShopItemDatas.Add(item10);
            ShopItemDatas.Add(item11);
            ShopItemDatas.Add(item12);
            ShopItemDatas.Add(item13);
            ShopItemDatas.Add(item14);
            ShopItemDatas.Add(item15);
            ShopItemDatas.Add(item16);
            ShopItemDatas.Add(item17);
            ShopItemDatas.Add(item18);
            ShopItemDatas.Add(item19);
            ShopItemDatas.Add(item20);
            ShopItemDatas.Add(item21);
            ShopItemDatas.Add(item22);
            ShopItemDatas.Add(item23);
            ShopItemDatas.Add(item24);
            ShopItemDatas.Add(item25);
            ShopItemDatas.Add(item26);
            ShopItemDatas.Add(item27);
            ShopItemDatas.Add(item28);
            //Normal Items


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
            _newItem = GetDailyRandomItem(); // level 5 = 1k gold, level 10 = 15k Gold;
            randomItems.Add(_newItem);
        }// 1k gold 10 adet - 3gem 10 adet - 10 adet tablo
        byte maxGoldItemCount = (byte)Random.Range(3,5), maxGemItemCount = (byte)(6 - maxGoldItemCount), maxTabletemCount = 1, currentGoldItemCount = 0, currentGemItemCount=0, currentTableItemCount = 0;
        if (maxGoldItemCount+maxGemItemCount+maxTabletemCount != 7)
        {
            Debug.Log($"MaxItemCounts degeri:({maxGoldItemCount + maxGemItemCount + maxTabletemCount}) => 7'ye esit olmali.");
            return (new List<ItemData>(), new List<ItemData>(), new List<ItemData>());
        }
        List<ItemData> currentItems = new List<ItemData>();
        foreach (var item in randomItems)
        {
            if (item.CurrentItemType == ItemType.Gold && currentGoldItemCount < maxGoldItemCount  && !currentItems.Contains(item))
            {
                currentItems.Add(item);
                //Debug.Log("Before" + item.ID);
                currentGoldItemCount++;
            }
            else if (item.CurrentItemType == ItemType.Gem && currentGemItemCount < maxGemItemCount && !currentItems.Contains(item))
            {
                currentItems.Add(item);
                //Debug.Log("Before" + item.ID);
                currentGemItemCount++;
            }
            else if (item.CurrentItemType == ItemType.Table && currentTableItemCount < maxTabletemCount)
            {
                currentItems.Add(item);
                //Debug.Log("Before" + item.ID);
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
        }; // 1k gold | 2 gem 
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
    public List<ItemData> GetAllDailyRewardItemDatas()
    {
        return CurrentDailyRewardItems;
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
                //Debug.Log($"{gold.ID} ID'li - {gold.CurrentItemType} item Turlu - {gold.CurrentShoppingType} item Shop turlu - {gold.Amount} - {gold.Name} Adli item eklendi!");
            }

            if (i < Items.Gems.Count)
            {
                var gem = Items.Gems[i];
                CurrentDailyRewardItems.Add(gem);
                //Debug.Log($"{gem.ID} ID'li - {gem.CurrentItemType} item Turlu - {gem.CurrentShoppingType} item Shop turlu - {gem.Amount} - {gem.Name} Adli item eklendi!");
            }
        }
        foreach (var item in Items.Tables)
        {
            CurrentDailyRewardItems.Add(item);
            //Debug.Log($"{item.ID} ID'li - {item.CurrentItemType} item Turlu - {item.CurrentShoppingType} item Shop turlu - {item.Amount} - {item.Name} Adli item eklendi!");
        }
    }
}
public enum ItemType
{
    None,
    Gem,
    Gold,
    Table,
    All,
    Ads
}

public enum ShoppingType
{
    None,
    Gem,
    Gold,
    RealMoney,
    DailyReward
}
