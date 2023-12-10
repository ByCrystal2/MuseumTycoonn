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
        public void BuyTheRoom(RoomData currentRoom)
    {
        Debug.Log("Kapý kilidine týklandý/dokunuldu.");
        MuseumManager.instance.AddGold(100000); // KRÝTÝK KOD TEST EDÝLDÝKTEN SONRA KALDIRILMALI!!        
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
        RoomData purchasedRoom = roomDatas.Where(x=> x.ID == currentRoomID).FirstOrDefault();
        Debug.Log(purchasedRoom.name + " Adlý, " + purchasedRoom.availableRoomCell.currentCellLetter + purchasedRoom.availableRoomCell.CellNumber + " Kodlu oda için " + "Satýn alma iþlevi sorgulanýyor...");
        if (purchasedRoom.CurrentShoppingType == ShoppingType.Gem)
        {
            if (purchasedRoom.RequiredMoney <= MuseumManager.instance.GetCurrentGem())
            {
                purchasedRoom.isLock = false;
                purchasedRoom.isActive = true;

                List<RoomData> _CellLetterRooms = roomDatas.Where(x=> x.availableRoomCell.currentCellLetter == purchasedRoom.availableRoomCell.currentCellLetter).ToList();
                foreach (var room in _CellLetterRooms)
                {
                    if ((room.availableRoomCell.CellNumber == purchasedRoom.availableRoomCell.CellNumber - 1 || room.availableRoomCell.CellNumber == purchasedRoom.availableRoomCell.CellNumber + 1) && room.isLock && room.isActive )
                    {
                        room.RequiredMoney += (purchasedRoom.RequiredMoney * 2) + 500;
                        break;
                    }
                }

            }
            else
            {
                Debug.Log(purchasedRoom.availableRoomCell.currentCellLetter + purchasedRoom.availableRoomCell.CellNumber + "  Numaralý Odayý Satýn Almaya Paran Yetmedi.");
            }
        }
        else if (purchasedRoom.CurrentShoppingType == ShoppingType.Gold)
        {
            if (purchasedRoom.RequiredMoney <= MuseumManager.instance.GetCurrentGold())
            {
                purchasedRoom.isLock = false;
                purchasedRoom.isActive = true;
                // B4
                List<RoomData> _CellCodeRooms = roomDatas.Where(x => x.availableRoomCell.currentCellLetter == purchasedRoom.availableRoomCell.currentCellLetter || ((int)x.availableRoomCell.currentCellLetter) == ((int)purchasedRoom.availableRoomCell.currentCellLetter) + 1 || ((int)x.availableRoomCell.currentCellLetter) == ((int)purchasedRoom.availableRoomCell.currentCellLetter) - 1).ToList();
                // A odalarý B Odalarý ve C Odalarý
                foreach (var currentRoom in _CellCodeRooms) //A1
                {
                    int currentRoomCellNumber = currentRoom.availableRoomCell.CellNumber;
                    int purchasedRoomCellNumber = purchasedRoom.availableRoomCell.CellNumber;

                    int currentRoomCellLetter = ((int)currentRoom.availableRoomCell.currentCellLetter);
                    int purchasedRoomCellLetter = ((int)purchasedRoom.availableRoomCell.currentCellLetter);
                    //Mevcut odamýz B3 diye düþünelim.
                    if (!currentRoom.isActive && currentRoom.isLock)
                    {
                        if ((currentRoomCellLetter == purchasedRoomCellLetter && currentRoomCellNumber == purchasedRoomCellNumber - 1) /* Mevcut Oda B3 ise */ || (currentRoomCellLetter == purchasedRoomCellLetter && currentRoomCellNumber == purchasedRoomCellNumber + 1) /* Mevcut Oda B5 ise */  || (currentRoomCellLetter == purchasedRoomCellLetter - 1 && currentRoomCellNumber == purchasedRoomCellNumber) /* Mevcut Oda A4 ise */  || (currentRoomCellLetter == purchasedRoomCellLetter + 1 && currentRoomCellNumber == purchasedRoomCellNumber) /* Mevcut Oda C4 ise */)
                        {
                            Debug.Log(currentRoom.name + "ODA ÝSTENEN KLASSMANA UYGUN.");
                            currentRoom.isActive = true;                            
                            currentRoom.RequiredMoney = (purchasedRoom.RequiredMoney * 2) + 500;

                            roomDatas.Where(x => x.isActive && x.isLock).Select(x=> x.RequiredMoney = currentRoom.RequiredMoney); // en son burda kaldýk. mevcut odanýn para gereksinimi tüm aktif ve kilitli odalara yansýmýyor.
                        }
                        // B3 - B5 - A4 - C4 => 2500                       
                    }
                }
            }
            else
            {
                Debug.Log(purchasedRoom.availableRoomCell.currentCellLetter + purchasedRoom.availableRoomCell.CellNumber + "  Numaralý Odayý Satýn Almaya Paran Yetmedi.");
            }
        }
        else if (purchasedRoom.CurrentShoppingType == ShoppingType.RealMoney)
        {

        }
    }
}
