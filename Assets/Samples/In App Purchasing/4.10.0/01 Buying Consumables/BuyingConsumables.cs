using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
public class BuyingConsumables : MonoBehaviour, IDetailedStoreListener
{
    IStoreController m_StoreController; // The Unity Purchasing system.
    public static BuyingConsumables instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);        
    }

    public void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var item in ItemManager.instance.GetAllIAPItemDatas())
        {
            if (item.CurrentItemType == ItemType.Gold || item.CurrentItemType == ItemType.Gem)
                builder.AddProduct(item.IAP_ID, ProductType.Consumable);
            else if (item.CurrentItemType == ItemType.Table)
                builder.AddProduct(item.IAP_ID, ProductType.NonConsumable);
        }
        List<RoomData> IAPRooms = RoomManager.instance.RoomDatas.Where(x => x.CurrentShoppingType == ShoppingType.RealMoney).ToList();
        if (IAPRooms != null && IAPRooms.Count > 0)
        {
            Debug.Log("IAP Rooms 0 index: " + IAPRooms[0].availableRoomCell.CellLetter + IAPRooms[0].availableRoomCell.CellNumber);
            foreach (var room in IAPRooms)
            {
                if (room.CurrentRoomType == RoomType.Normal) // item turu normal ise yapýlacak islemler...
                {
                    builder.AddProduct(room.IAP_ID, ProductType.NonConsumable);
                    Debug.Log("Oda Buildere Eklendi: " + room.ID + " " + room.IAP_ID + " " + room.availableRoomCell.CellLetter + room.availableRoomCell.CellNumber);
                }
                else if (room.CurrentRoomType == RoomType.Special) // item turu special ise yapýlacak islemler...
                {
                    builder.AddProduct(room.IAP_ID, ProductType.NonConsumable);
                    Debug.Log("Oda Buildere Eklendi: " + room.ID + " " + room.IAP_ID + " " + room.availableRoomCell.CellLetter + room.availableRoomCell.CellNumber);
                }

            }
        }
        else
        {
            Debug.Log("Para ile satilan oda bulunamadi.");
        }
        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyItemFromStore(ItemData buyItem)
    {
        Debug.Log("buyItem.IAP_ID => " + buyItem.IAP_ID);
        m_StoreController.InitiatePurchase(buyItem.IAP_ID);
    }
    public void BuyItemFromStore(RoomData buyItem)
    {
        m_StoreController.InitiatePurchase(buyItem.IAP_ID);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        m_StoreController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null)
        {
            errorMessage += $" More details: {message}";
        }

        Debug.Log(errorMessage);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        var product = args.purchasedProduct;

        ItemData currentItem = ItemManager.instance.GetAllIAPItemDatas().Where(x => x.IAP_ID == product.definition.id).SingleOrDefault();
        RoomData currentRoom = RoomManager.instance.RoomDatas.Where(x => x.IAP_ID == product.definition.id).SingleOrDefault();
        if (args.purchasedProduct != null && !currentItem.Equals(default(ItemData)))
        {
            //ItemData currentItem = ItemManager.instance.GetAllIAPItemDatas().Where(x => x.IAP_ID == product.definition.id).SingleOrDefault();
            if (!currentItem.Equals(default(ItemData)))
            {
                if (currentItem.CurrentItemType == ItemType.Gem)
                {
                    AddGem(currentItem.Amount);
                }
                else if (currentItem.CurrentItemType == ItemType.Gold)
                {
                    AddGold(currentItem.Amount);
                }
                else if (currentItem.CurrentItemType == ItemType.Table)
                {
                    AddTable(currentItem);
                }
                Debug.Log($"Purchase Complete - Product: {product.definition.id}");
                Debug.Log($"Purchase Complete - Item: {currentItem.IAP_ID}");

                //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
                return PurchaseProcessingResult.Complete;
            }
            else
            {
                Debug.Log(product.definition.id + " Bu IAPID'de bir item bulunamadi.");
                return PurchaseProcessingResult.Pending;
            }
        }
        else if (args.purchasedProduct != null && currentRoom != null)
        {
            if (currentRoom.isActive && currentRoom.isLock)
            {
                if (currentRoom.CurrentRoomType == RoomType.Normal)
                {
                    AddRoom(currentRoom);
                }

                Debug.Log($"Purchase Complete - Product: {product.definition.id}");
                Debug.Log($"Purchase Complete - Room: {currentRoom.IAP_ID}");

                //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
                return PurchaseProcessingResult.Complete;
            }
            else
            {
                Debug.Log("Islem tamamlanmadi! Satin alinmak istenen oda aktif degil ve/veya kilitli degil.");
                return PurchaseProcessingResult.Pending;
            }
        }
        else
        {
            Debug.Log("Boyle Bir Urun Bulunamadi.");
            return PurchaseProcessingResult.Pending;
        }

    }
    void AddGold(float _amount)
    {
        MuseumManager.instance.AddGold(_amount);
    }

    void AddGem(float _amount)
    {
        MuseumManager.instance.AddGem(_amount);
    }
    void AddTable(ItemData _table)
    {
        PictureData newInventoryItem = new PictureData();
        newInventoryItem.TextureID = _table.textureID;
        newInventoryItem.RequiredGold = PicturesMenuController.instance.PictureChangeRequiredAmount;
        newInventoryItem.painterData = new PainterData(_table.ID, _table.Description, _table.Name, _table.StarCount);
        MuseumManager.instance.AddNewItemToInventory(newInventoryItem);

        ItemManager.instance.GetAllItemDatas().Remove(_table);
        ItemManager.instance.GetAllIAPItemDatas().Remove(_table);
        ShopController.instance.GetCurrentShoppingTypeItems();
    }
    void AddRoom(RoomData _room)
    {
        RoomManager.instance.RoomsActivationAndPurchasedControl(_room, RoomManager.instance.RoomDatas);
        GameManager.instance.Save();
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
            $" Purchase failure reason: {failureDescription.reason}," +
            $" Purchase failure details: {failureDescription.message}");
    }
}
