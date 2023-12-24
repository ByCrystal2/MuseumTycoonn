using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    // Buy Room Panel Elements
    
    public GameObject PnlBuyRoom;
    public TextMeshProUGUI txtBuyQuestion;
    public TextMeshProUGUI txtRequiredMoney;
    public Image Money;
    public Sprite GemSprite;
    public Sprite GoldSprite;
    public Sprite RealMoneySprite;
    int currentRoomID;

    public List<RoomData> RoomDatas;
    public static RoomManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    public void AddRooms()
    {
        RoomDatas = GameObject.FindObjectsOfType<RoomData>().ToList();
    }
    public void BuyTheRoom(RoomData currentRoom)
    {
        Debug.Log("Kapý kilidine týklandý/dokunuldu.");
        MuseumManager.instance.AddGold(10000); // KRÝTÝK KOD TEST EDÝLDÝKTEN SONRA KALDIRILMALI!!        
        currentRoomID = currentRoom.ID;

        PnlBuyRoom.SetActive(false);
        txtRequiredMoney.text = currentRoom.RequiredMoney.ToString();
        Money.sprite = SetAndControlItemIcon(currentRoom.CurrentShoppingType);
        PnlBuyRoom.SetActive(true);
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
    public void CancelButton()
    {
        PnlBuyRoom.SetActive(false);

    }

    public void AcceptButton()
    {
        PnlBuyRoom.SetActive(false);
        List<RoomData> roomDatas = GameObject.FindObjectsOfType<RoomData>().ToList();

        //RoomData purchasedRoom = roomDatas.Where(x=> x.ID == currentRoomID).SingleOrDefault();
        RoomData purchasedRoom;
        string exMessage;
        if (TryGetRoomData(currentRoomID, out purchasedRoom, roomDatas, out exMessage))
            Debug.Log(exMessage);
        else
        {
            Debug.Log(exMessage);
            return;
        }
        if (purchasedRoom.CurrentShoppingType == ShoppingType.Gem)
        {
            if (purchasedRoom.RequiredMoney <= MuseumManager.instance.GetCurrentGem())
            {
                RoomsActivationAndPurchasedControl(purchasedRoom, roomDatas);
                MuseumManager.instance.SpendingGem(purchasedRoom.RequiredMoney);
                GameManager.instance.Save();
            }
            else
            {
                Debug.Log(purchasedRoom.availableRoomCell.CellLetter + purchasedRoom.availableRoomCell.CellNumber + "  Numaralý Odayý Satýn Almaya Paran Yetmedi.");
            }
        }
        else if (purchasedRoom.CurrentShoppingType == ShoppingType.Gold)
        {
            if (purchasedRoom.RequiredMoney <= MuseumManager.instance.GetCurrentGold())
            {
                RoomsActivationAndPurchasedControl(purchasedRoom, roomDatas);
                MuseumManager.instance.SpendingGold(purchasedRoom.RequiredMoney);
                GameManager.instance.Save();
            }
            else
            {
                Debug.Log(purchasedRoom.availableRoomCell.CellLetter + purchasedRoom.availableRoomCell.CellNumber + "  Numaralý Odayý Satýn Almaya Paran Yetmedi.");
            }
        }
        else if (purchasedRoom.CurrentShoppingType == ShoppingType.RealMoney)
        {
            // Gercek Parayla satin alinan oda islemleri...
            BuyingConsumables.instance.BuyItemFromStore(purchasedRoom);
        }
    }

    public void RoomsActivationAndPurchasedControl(RoomData purchasedRoom, List<RoomData> roomDatas)
    {
        purchasedRoom.isLock = false;
        purchasedRoom.isActive = true;
        purchasedRoom.IsPurchased(true);
        int purchasedRoomCellNumber = purchasedRoom.availableRoomCell.CellNumber;
        int purchasedRoomCellLetter = ((int)purchasedRoom.availableRoomCell.CellLetter);
        // B4
        List<RoomData> _CellCodeRooms = roomDatas.Where(x => x.availableRoomCell.CellLetter == purchasedRoom.availableRoomCell.CellLetter || ((int)x.availableRoomCell.CellLetter) == ((int)purchasedRoom.availableRoomCell.CellLetter) + 1 || ((int)x.availableRoomCell.CellLetter) == ((int)purchasedRoom.availableRoomCell.CellLetter) - 1).ToList();

        float activeRoomRequiredMoney = 0;
        // A odalarý B Odalarý ve C Odalarý
        foreach (var currentRoom in _CellCodeRooms) //A1
        {
            int currentRoomCellNumber = currentRoom.availableRoomCell.CellNumber;
            int currentRoomCellLetter = ((int)currentRoom.availableRoomCell.CellLetter);
            
            //Mevcut odamýz B3 diye düþünelim.
            if (!currentRoom.isActive && currentRoom.isLock)
            {
                if ((currentRoomCellLetter == purchasedRoomCellLetter && currentRoomCellNumber == purchasedRoomCellNumber - 1) /* Mevcut Oda B3 ise */ || (currentRoomCellLetter == purchasedRoomCellLetter && currentRoomCellNumber == purchasedRoomCellNumber + 1) /* Mevcut Oda B5 ise */  || (currentRoomCellLetter == purchasedRoomCellLetter - 1 && currentRoomCellNumber == purchasedRoomCellNumber) /* Mevcut Oda A4 ise */  || (currentRoomCellLetter == purchasedRoomCellLetter + 1 && currentRoomCellNumber == purchasedRoomCellNumber) /* Mevcut Oda C4 ise */)
                {
                    currentRoom.isActive = true;
                    Debug.Log(currentRoom.availableRoomCell.CellLetter + " " + currentRoom.availableRoomCell.CellNumber + " Kodlu Oda Aktif Edildi.");                    
                    currentRoom.GetComponentInChildren<RoomCloudActivation>().CloudActivationChange(false);

                    if (currentRoom.CurrentShoppingType == ShoppingType.RealMoney)
                    {
                        //currentRoom shopppingtype relamoney ise yapilacak islemler
                    }
                    else
                    {
                        //currentRoom shopppingtype relamoney degilse yapilacak islemler
                        currentRoom.RequiredMoney = (purchasedRoom.RequiredMoney * 2) + 500;
                        activeRoomRequiredMoney = currentRoom.RequiredMoney;
                    }
                }
                // B3 - B5 - A4 - C4 => 2500
            }
        }
        List<RoomData> activeRoomDatas = roomDatas.Where(x => x.isActive && x.isLock).ToList();
        foreach (var _activeRoom in activeRoomDatas)
        {
            if (!(_activeRoom.CurrentShoppingType == ShoppingType.RealMoney))            
                _activeRoom.RequiredMoney = activeRoomRequiredMoney;            
        }
    }

    
    private bool TryGetRoomData(int roomId, out RoomData room, List<RoomData> _roomDatas, out string _exMessage)
    {
        room = _roomDatas.FirstOrDefault(x => x.ID == roomId);

        if (room != null)
        {
            // Belirtilen ID'ye sahip oda bulundu
            bool multipleRooms = _roomDatas.Count(x => x.ID == roomId) > 1;
            if (multipleRooms)
            {
                // Birden fazla oda var
                _exMessage =  room.ID + " Bu ID'den Birden fazla oda mevcut. Bu odalar þunlardýr: ";
                RoomData currentRoom = room;//A2
                List<RoomCell> roomCodes = _roomDatas.Where(x => x.ID == roomId).Select(x=> x.availableRoomCell).ToList(); // RoomCell listesi at.
                // Bütün odalarýmýzýn bulunduðu roomdatas listesinde, mevcut odamýzýn id'si ile eþleþen odalarýn RoomCell'ini listeledik ve RoomCell tipinde ki roomCodes adlý Listeye aktardýk.          
                
                
                foreach (RoomCell roomCode in roomCodes) // örn: 3 adet roomCode var.
                {
                    
                    _exMessage += (" " + roomCode.CellLetter + roomCode.CellNumber + " Kodlu Oda.").ToString(); //  room.ID + " Bu ID'den Birden fazla oda mevcut. Bu odalar þunlardýr: A1 Kodlu Oda. A2 Kodlu Oda. A3 Kodlu Oda."
                }
                
                return false;
            }
            else
            {
                // Tek oda bulundu
                _exMessage = room.ID + " ID olan " + room.availableRoomCell.CellLetter + room.availableRoomCell.CellNumber + " Kodlu Oda Bulundu.";
                return true;
            }
        }
        else
        {
            // Belirtilen ID'ye sahip oda bulunamadý
            _exMessage = "Belirtilen ID'ye sahip oda bulunamadý";
            return false;
        }
    }
}
