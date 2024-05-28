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

    public ShopUIType currentShopUIType;
    public static ShopController instance { get; set; }
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void GetCurrentShoppingTypeItems()
    {
        switch (currentShopUIType)
        {
            case ShopUIType.None:
                Debug.Log("Shop UI Turu Bulunamadi.");
                break;
            case ShopUIType.All:
                GetAllItemsUpdate();
                break;
            case ShopUIType.Gem:
                GetGemItems();
                break;
            case ShopUIType.Gold:
                GetGoldItems();
                break;
            case ShopUIType.Table:
                GetTableItems();
                break;
            default:
                break;
        }
    }
    public bool isAfterShopUIType = false;
    public void GetAllItemsUpdate()
    {
        if (currentShopUIType == ShopUIType.All)
            isAfterShopUIType = true;
        else
            isAfterShopUIType = false;

        currentShopUIType = ShopUIType.All;
        itemContent = GameObject.FindWithTag("ShopPanelItems").transform;
        
        ClearAllItemsInShop();
        List<ItemData> items = new List<ItemData>();
        items=ItemManager.instance.GetAllItemDatas();
        foreach (ItemData item in items)
        {
            
                GameObject newItem;
                if (item.CurrentItemType == ItemType.Gold || item.CurrentItemType == ItemType.Gem || item.CurrentItemType == ItemType.Ads)
                {
                    newItem = Instantiate(itemPrefabv1, itemContent);
                }
                else if (item.CurrentItemType == ItemType.Table)
                {
                    newItem = Instantiate(itemPrefabv2, itemContent);
                }
                else
                {
                    Debug.Log("Mevcut itemin turu yoktur.!");
                    return;
                }
                switch (item.CurrentItemType)
                {
                    case ItemType.None: // Tür yoksa.
                        Debug.Log("Itemin turu bulunmamaktadir.");
                        break;
                    case ItemType.Gem:
                        newItem.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(item));
                        SetNewItemProparties(newItem, item, ItemType.Gem);
                        break;
                    case ItemType.Gold:
                        newItem.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(item));
                        SetNewItemProparties(newItem, item, ItemType.Gold);
                        break;
                    case ItemType.Table:
                        newItem.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(item));
                        SetNewItemProparties(newItem, item, ItemType.Table);
                        break;
                    case ItemType.Ads:
                        newItem.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => BuyItem(item));
                        SetNewItemProparties(newItem, item, ItemType.Ads);
                    break;
                    default:
                        break;
                }
        }
    }
    public void ClearAllItemsInShop()
    {
        foreach (Transform child in itemContent)
        {
            // Her bir çocuk nesneyi sil
            Destroy(child.gameObject);
        }
        Debug.Log("Magaza Temizlendi.");
    }
    
    public void GetGemItems()//Gem button click e baðlýdýr.
    {
        if (currentShopUIType == ShopUIType.Gem)
            isAfterShopUIType = true;
        else
            isAfterShopUIType = false;

        currentShopUIType = ShopUIType.Gem;
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
    }
    public void GetGoldItems()//Gold button click e baðlýdýr.
    {
        if (currentShopUIType == ShopUIType.Gold)
            isAfterShopUIType = true;
        else
            isAfterShopUIType = false;

        currentShopUIType = ShopUIType.Gold;
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
    }

    public void GetTableItems()//Table button click e baðlýdýr.
    {
        if (currentShopUIType == ShopUIType.Table)
            isAfterShopUIType = true;
        else
            isAfterShopUIType = false;

        currentShopUIType = ShopUIType.Table;
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
    }
    public void SetButtonsAlphaValue()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Debug.Log("mevcut button namesi " + clickedButton.name);
        if (clickedButton.transform.GetChild(0).TryGetComponent(out CanvasGroup currentButtonCGroup))
        {
            currentButtonCGroup.alpha = ClikedShopTabAlpha;
        }
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
            AssignmentToItemChildren(_newItem, _currentItem, _currentItem.Amount.ToString());
        }
        else if (_itemType == ItemType.Table)
        {
            AssignmentToItemChildren(_newItem, _currentItem, _currentItem.Description);

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
        else if (_itemType == ItemType.Ads)
        {
            AssignmentToItemChildren(_newItem, _currentItem, _currentItem.Description);
        }
    }
    public void AssignmentToItemChildren(GameObject _newItem, ItemData _currentItem, string childFourValue)
    {
        _newItem.transform.GetChild(3).GetComponent<Text>().text = _currentItem.Name;
        _newItem.transform.GetChild(4).GetComponent<Text>().text = childFourValue;
        
        if(_currentItem.CurrentShoppingType != ShoppingType.RealMoney)
        {
            _newItem.transform.GetChild(5).transform.GetChild(2).GetComponent<Text>().text = _currentItem.RequiredMoney.ToString();
            _newItem.transform.GetChild(5).transform.GetChild(1).GetComponent<Image>().sprite = SetAndControlItemIcon(_currentItem.CurrentShoppingType);
        }
        else
        {
            _newItem.transform.GetChild(5).transform.GetChild(2).GetComponent<Text>().text = BuyingConsumables.instance.GetProductLocalizedPriceString(_currentItem.IAP_ID);
            Debug.Log("_currentItem.IAP_ID => " + _currentItem.IAP_ID);
            _newItem.transform.GetChild(5).transform.GetChild(1).gameObject.SetActive(false);

        }
        _newItem.transform.GetChild(0).GetComponent<Image>().sprite = _currentItem.ImageSprite;
        _newItem.transform.GetChild(2).GetComponent<Image>().sprite = _currentItem.ImageSprite;
    }
    public void BuyItem(ItemData _item)
    {
        Debug.Log(_item.CurrentItemType + " Item tipinde ki " + _item.Amount + " miktarda  ürün " + _item.RequiredMoney + " " + _item.CurrentShoppingType + " karþýlýðýnda satýlmak istendi.");
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
                    newInventoryItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount;
                    newInventoryItem.painterData = new PainterData(_item.ID, _item.Description, _item.Name, _item.StarCount);
                    MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
                    ItemsBuyingUpdate(_item);
                }
            }
            else
            {
                UIController.instance.InsufficientGemEffect();
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
                    newInventoryItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount;
                    newInventoryItem.painterData = new PainterData(_item.ID,_item.Description,_item.Name, _item.StarCount);
                    MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
                    ItemsBuyingUpdate(_item);
                }
            }
            else
            {
                UIController.instance.InsufficientGoldEffect();
                Debug.Log("Mevcut olan " + MuseumManager.instance.GetCurrentGold() + " miktarýnýz, gerekli olan " + _item.RequiredMoney + " miktarýndan daha az.");
            }

        }
        else if (_item.CurrentShoppingType == ShoppingType.RealMoney)
        {
            _item.IsPurchased = true;
            //BuyingConsumables.instance.BuyItemFromStore(_item);
            
            BuyingConsumables.instance.BuyItemFromStore(_item);
            GameManager.instance.Save();
        }

    }
    public void ItemsBuyingUpdate(ItemData _item)
    {
        ItemManager.instance.ShopItemDatas.Remove(_item);
        MuseumManager.instance.PurchasedItems.Add(_item);
        //ItemManager.instance.AddItemInShop(ItemManager.instance.RItems[Random.Range(0, ItemManager.instance.RItems.Count)]);
        GameManager.instance.Save();
        GetCurrentShoppingTypeItems();
    }
    public Sprite SetAndControlItemIcon(ShoppingType _shoppingType)
    {
        Sprite _currentIcon = _shoppingType switch
        {
            ShoppingType.Gem => GemSprite,
            ShoppingType.Gold => GoldSprite,
            ShoppingType.RealMoney => null,
            _ => GoldSprite
        };

        return _currentIcon;
    }
}
public enum ShopUIType
{
    None,
    All,
    Gem,
    Gold,
    Table
}
