using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    private Transform itemContent;
    [SerializeField] GameObject itemPrefabv1;
    [SerializeField] GameObject itemPrefabv2;

    [SerializeField] Sprite GoldSprite;
    [SerializeField] Sprite GemSprite;
    [SerializeField] Sprite RealMoneySprite;

    //ShopTabOptions
    public float ClikedShopTabAlpha = 0.2f;
    public float DefaultShopTabAlpha = 1f;

    public static ShopController instance { get; set; }
    
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
    public void GetAllItems()
    {
        itemContent = GameObject.FindWithTag("ShopPanelItems").transform;
        
        ClearAllItemsInShop();
        List<ItemData> items = new List<ItemData>();
        items=ItemManager.instance.GetAllItemDatas();
        foreach (ItemData item in items)
        {
            if (item != null)
            {
                GameObject newItem;
                if (item.CurrentItemType == ItemType.Gold || item.CurrentItemType == ItemType.Gem)
                {
                    newItem = Instantiate(itemPrefabv1, itemContent);
                }
                else if (item.CurrentItemType == ItemType.Table)
                {
                    newItem = Instantiate(itemPrefabv2, itemContent);
                }
                else
                {
                    Debug.Log("Mevcut itemin türü yoktur.!");
                    return;
                }
                switch (item.CurrentItemType)
                {
                    case ItemType.None: // Tür yoksa.
                        Debug.Log("Itemin türü bulunmamaktadýr.");
                        break;
                    case ItemType.Gem:
                        SetNewItemProparties(newItem, item, ItemType.Gem);
                        break;
                    case ItemType.Gold:
                        SetNewItemProparties(newItem, item, ItemType.Gold);
                        break;
                    case ItemType.Table:
                        SetNewItemProparties(newItem, item, ItemType.Table);
                        break;                   
                    default:
                        break;
                }
            }
            ItemManager.instance.CurrentItemDatas = items;
        }
    }
    public void ClearAllItemsInShop()
    {
        foreach (Transform child in itemContent)
        {
            // Her bir çocuk nesneyi sil
            Destroy(child.gameObject);
        }
        //ItemManager.instance.CurrentItemDatas.Clear();
    }
    public void GetGemItems()//Gem button click e baðlýdýr.
    {
        SetButtonsAlphaValue();

        Debug.Log("Gem Butonuna tiklandi.");
        ClearAllItemsInShop();
        List<ItemData> gemItems = new List<ItemData>();
        gemItems = ItemManager.instance.GetItemTypeItems(ItemType.Gem);
        foreach (ItemData gemItem in gemItems) // ItemData gemItem = gemItems[0] ItemData gemItem = gemItems[1]
        {
            GameObject _newGem = Instantiate(itemPrefabv1, itemContent);
            _newGem.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(gemItem));
            SetNewItemProparties(_newGem, gemItem, ItemType.Gem);
        }
        ItemManager.instance.CurrentItemDatas = gemItems;
    }
    public void GetGoldItems()//Gold button click e baðlýdýr.
    {
        SetButtonsAlphaValue();

        Debug.Log("Gold Butonuna tiklandi.");
        ClearAllItemsInShop();
        List<ItemData> goldItems = new List<ItemData>();
        goldItems = ItemManager.instance.GetItemTypeItems(ItemType.Gold);
        foreach (ItemData goldItem in goldItems) // ItemData gemItem = gemItems[0] ItemData gemItem = gemItems[1]
        {
            GameObject _newGold = Instantiate(itemPrefabv1, itemContent);
            _newGold.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(goldItem));
            SetNewItemProparties(_newGold, goldItem, ItemType.Gold);
        }
        ItemManager.instance.CurrentItemDatas = goldItems;
    }

    public void GetTableItems()//Table button click e baðlýdýr.
    {

        SetButtonsAlphaValue();
        
        Debug.Log("Table Butonuna tiklandi.");
        ClearAllItemsInShop();
        List<ItemData> tableItems = new List<ItemData>();
        tableItems =ItemManager.instance.GetItemTypeItems(ItemType.Table);
        foreach (ItemData tableItem in tableItems) // ItemData gemItem = gemItems[0] ItemData gemItem = gemItems[1]
        {
            GameObject _newTable = Instantiate(itemPrefabv2, itemContent);
            _newTable.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(tableItem));
            SetNewItemProparties(_newTable, tableItem, ItemType.Table);
        }
        ItemManager.instance.CurrentItemDatas = tableItems;
    }
    public void SetButtonsAlphaValue()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Debug.Log("mevcut button namesi " + clickedButton.name);
        clickedButton.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = ClikedShopTabAlpha;
        //for (int i = 0; i < clickedButton.gameObject.transform.parent.childCount; i++)
        //{
        //    if (clickedButton.gameObject.transform.parent.GetChild(i).gameObject.name != clickedButton.gameObject.name)
        //    {
        //        clickedButton.gameObject.transform.parent.GetChild(i).transform.GetChild(0).GetComponent<CanvasGroup>().alpha = DefaultShopTabAlpha;
        //    }
        //}
    }
    public void SetNewItemProparties(GameObject _newItem, ItemData _currentItem, ItemType _itemType)
    {
        if (_itemType == ItemType.Gold || _itemType == ItemType.Gem)
        {
            _newItem.transform.GetChild(3).GetComponent<Text>().text = _currentItem.Name;
            _newItem.transform.GetChild(4).GetComponent<Text>().text = _currentItem.Amount.ToString();
            _newItem.transform.GetChild(5).transform.GetChild(1).GetComponent<Image>().sprite = SetAndControlItemIcon(_currentItem.CurrentShoppingType);
            _newItem.transform.GetChild(5).transform.GetChild(2).GetComponent<Text>().text = _currentItem.RequiredMoney.ToString();
            _newItem.transform.GetChild(0).GetComponent<Image>().sprite = _currentItem.ImageSprite;
            _newItem.transform.GetChild(2).GetComponent<Image>().sprite = _currentItem.ImageSprite;            
        }
        else if (_itemType == ItemType.Table)
        {
            _newItem.transform.GetChild(3).GetComponent<Text>().text = _currentItem.Name;
            _newItem.transform.GetChild(4).GetComponent<Text>().text = _currentItem.Description;
            _newItem.transform.GetChild(5).transform.GetChild(1).GetComponent<Image>().sprite = SetAndControlItemIcon(_currentItem.CurrentShoppingType);
            _newItem.transform.GetChild(5).transform.GetChild(2).GetComponent<Text>().text = _currentItem.RequiredMoney.ToString();
            _newItem.transform.GetChild(0).GetComponent<Image>().sprite = _currentItem.ImageSprite;
            _newItem.transform.GetChild(2).GetComponent<Image>().sprite = _currentItem.ImageSprite;
            List<GameObject> OpenStars = new List<GameObject>();
            List<GameObject> CloseStars = new List<GameObject>();
            Transform parentObject1 = _newItem.transform.GetChild(6).transform.GetChild(0);
            Transform parentObject2 = _newItem.transform.GetChild(6).transform.GetChild(1);
            // Tüm çocuklarý döngüyle al ve List'e ekle
            for (int i = 0; i < parentObject1.childCount; i++)
            {
                CloseStars.Add(parentObject1.GetChild(i).gameObject);
                CloseStars[i].gameObject.SetActive(true);
            }
            for (int i = 0; i < parentObject2.childCount; i++)
            {
                OpenStars.Add(parentObject2.GetChild(i).gameObject);
                OpenStars[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _currentItem.StarCount; i++)
            {
                OpenStars[i].gameObject.SetActive(true);
            }
        }
    }
    public void BuyItem(ItemData _item)
    {
        MuseumManager.instance.AddGold(5000); // KRÝTÝK KOD TESTTEN SONRA SÝLÝNMELÝ!
        MuseumManager.instance.AddGem(5000); // KRÝTÝK KOD TESTTEN SONRA SÝLÝNMELÝ!
        Debug.Log(_item.CurrentItemType + " Item tipinde ki " + _item.Amount + " miktarda " + _item.Name + " adlý ürün " + _item.RequiredMoney + " " + _item.CurrentShoppingType + " karþýlýðýnda satýlmak istendi.");
        if (_item.CurrentShoppingType == ShoppingType.Gem)
        {
            if (MuseumManager.instance.GetCurrentGem() >= _item.RequiredMoney)
            {
                MuseumManager.instance.SpendingGem(_item.RequiredMoney);
                if (_item.CurrentItemType == ItemType.Gold)
                {
                    MuseumManager.instance.AddGold(_item.Amount);
                }
                else if (_item.CurrentItemType == ItemType.Table)
                {
                    // Gemle satýn alýnan tablo
                    //Tablo satýlýnca marketten kaldýrýlmayacak fakat satýn alýndý yazýsý çýkacak.
                    PictureData newInventoryItem = new PictureData();
                    newInventoryItem.TextureID = _item.textureID;
                    newInventoryItem.RequiredGold = PicturesMenuController.instance.PictureChangeRequiredAmount;
                    newInventoryItem.painterData = new PainterData(_item.ID, _item.Description, _item.Name, _item.StarCount);
                    MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
                }
            }
            else
            {
                Debug.Log("Mevcut olan " + MuseumManager.instance.GetCurrentGem() + " miktarýnýz, gerekli olan " + _item.RequiredMoney + " miktarýndan daha az.");
            }
        }
        else if (_item.CurrentShoppingType == ShoppingType.Gold)
        {
            if (MuseumManager.instance.GetCurrentGold() >= _item.RequiredMoney)
            {
                MuseumManager.instance.SpendingGold(_item.RequiredMoney);
                if (_item.CurrentItemType == ItemType.Gem)
                {
                    MuseumManager.instance.AddGem(_item.Amount);
                }
                else if (_item.CurrentItemType == ItemType.Table)
                {
                    PictureData newInventoryItem = new PictureData();
                    newInventoryItem.TextureID = _item.textureID;
                    newInventoryItem.RequiredGold = PicturesMenuController.instance.PictureChangeRequiredAmount;
                    newInventoryItem.painterData = new PainterData(_item.ID,_item.Description,_item.Name, _item.StarCount);
                    MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
                }
            }
            else
            {
                Debug.Log("Mevcut olan " + MuseumManager.instance.GetCurrentGold() + " miktarýnýz, gerekli olan " + _item.RequiredMoney + " miktarýndan daha az.");
            }

        }
        else if (_item.CurrentShoppingType == ShoppingType.RealMoney)
        {
            // Gerçek para ile satýn alýnma durumu
        }

    }
    public Sprite SetAndControlItemIcon(ShoppingType _shoppingType)
    {
        Sprite _currentIcon = _shoppingType switch
        {
            ShoppingType.Gem => GemSprite,
            ShoppingType.Gold => GoldSprite,
            ShoppingType.RealMoney => RealMoneySprite,
            _ => GoldSprite
        };

        return _currentIcon;
    }
}
