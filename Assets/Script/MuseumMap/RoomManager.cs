using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class RoomManager : MonoBehaviour
{
    //Statues
    public StatuesHandler statuesHandler;
    //Statues

    //public float activeRoomsRequiredMoney = 1000;
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
    public RoomData CurrentEditedRoom;
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
        statuesHandler = new StatuesHandler(); // ctor ile heykeller olusturuldu.
        statuesHandler.AddEditObjs();
        
    }
    public void BuyTheRoom(RoomData currentRoom)
    {
        Debug.Log("Kapý kilidine týklandý/dokunuldu.");      
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
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
        {
            PlayerManager.instance.UnLockPlayer();
        }
    }

    public void AcceptButton()
    {
        PnlBuyRoom.SetActive(false);
        List<RoomData> roomDatas = GameObject.FindObjectsOfType<RoomData>().ToList();
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
        {
            PlayerManager.instance.UnLockPlayer();
        }
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

                GoogleAdsManager.instance.ShowInterstitialAd();
            }
            else
            {
                UIController.instance.InsufficientGemEffect();
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
                GoogleAdsManager.instance.ShowInterstitialAd();
            }
            else
            {
                UIController.instance.InsufficientGoldEffect();
                Debug.Log(purchasedRoom.availableRoomCell.CellLetter +""+ purchasedRoom.availableRoomCell.CellNumber + "  Numaralý Odayý Satýn Almaya Paran Yetmedi.");
            }
        }
        else if (purchasedRoom.CurrentShoppingType == ShoppingType.RealMoney)
        {
            // Gercek Parayla satin alinan oda islemleri...
            //BuyingConsumables.instance.BuyItemFromStore(purchasedRoom);
            GoogleAdsManager.instance.ShowInterstitialAd();
        }
    }

    public void ActivateRoomLocations(RoomData purchasedRoom)
    {
        Transform locationDatasParent = purchasedRoom.LocationHolder;
        int length = locationDatasParent.childCount;
        for (int i = 0; i < length; i++)
            locationDatasParent.GetChild(i).GetComponent<LocationData>().SetVisittible(true,false);
    }

    public void RoomsActivationAndPurchasedControl(RoomData purchasedRoom, List<RoomData> roomDatas)
    {
        List<RoomData> forDatabaseRoomDatas = new List<RoomData>();
        ActivateRoomLocations(purchasedRoom);
        purchasedRoom.isLock = false;
        purchasedRoom.isActive = true;
        purchasedRoom.IsPurchased(true);
        GPGamesManager.instance.achievementController.IncreasePurchasedRoomCount();
        RoomUIHandler _purchasedHandler= UIController.instance.roomUISPanelController.GetRoomUI(purchasedRoom.availableRoomCell);
        _purchasedHandler.UpdateMyUI();
        int purchasedRoomCellNumber = purchasedRoom.availableRoomCell.CellNumber;
        int purchasedRoomCellLetter = ((int)purchasedRoom.availableRoomCell.CellLetter);
        // B4
        List<RoomData> _CellCodeRooms = roomDatas.Where(x => x.availableRoomCell.CellLetter == purchasedRoom.availableRoomCell.CellLetter || ((int)x.availableRoomCell.CellLetter) == ((int)purchasedRoom.availableRoomCell.CellLetter) + 1 || ((int)x.availableRoomCell.CellLetter) == ((int)purchasedRoom.availableRoomCell.CellLetter) - 1).ToList();

        
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
                    //currentRoom.GetComponentInChildren<RoomCloudActivation>().CloudActivationChange(false);
                    RoomUIHandler _neighborHandler = UIController.instance.roomUISPanelController.GetRoomUI(currentRoom.availableRoomCell);
                    _neighborHandler.UpdateMyUI();
                    if (purchasedRoom.CurrentShoppingType != ShoppingType.RealMoney) // Satin alinan oda, Gercek para ile satin alinmamissa islemleri uygula.
                    {
                        if (currentRoom.CurrentShoppingType == ShoppingType.RealMoney)
                        {
                            //currentRoom shopppingtype relamoney ise yapilacak islemler
                        }
                        else
                        {
                            //currentRoom shopppingtype relamoney degilse yapilacak islemler
                            Debug.Log("CurrentRoom => " + currentRoom.availableRoomCell.CellLetter.ToString() + currentRoom.availableRoomCell.CellNumber.ToString());                            
                        }
                    }
                    forDatabaseRoomDatas.Add(currentRoom);
                }
                // B3 - B5 - A4 - C4 => 2500                
            }
        }
        if (purchasedRoom.CurrentShoppingType != ShoppingType.RealMoney)
        {
            List<RoomData> activeRoomDatas = roomDatas.Where(x => x.isActive && x.isLock).ToList();
        GameManager.instance.ActiveRoomsRequiredMoney = purchasedRoom.RequiredMoney + (activeRoomDatas.Count * 500);
            foreach (var _activeRoom in activeRoomDatas)
            {
                if (!(_activeRoom.CurrentShoppingType == ShoppingType.RealMoney))
                {
                    _activeRoom.RequiredMoney = GameManager.instance.ActiveRoomsRequiredMoney;
                    _activeRoom.SetMyRequiredTexts(GameManager.instance.ActiveRoomsRequiredMoney);
                }

            }
            Debug.Log("aktif odalarin fiyatlari guncellendi => " + GameManager.instance.ActiveRoomsRequiredMoney);
        }
        forDatabaseRoomDatas.Add(purchasedRoom);

        FirestoreManager.instance.roomDatasHandler.AddRoomsWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, forDatabaseRoomDatas);

        GPGamesManager.instance.achievementController.PurchasedRoomControl();
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

    public List<RoomData> GetRoomWithIDs(List<int> _id)
    {
        List<RoomData> Rooms = new List<RoomData>();

        foreach (var item in RoomDatas)
            foreach (var ids in _id)
                if (item.ID == ids)
                    Rooms.Add(item);

        return Rooms;
    }
    public RoomData GetRoomWithRoomCell(RoomCell _cell)
    {
        return RoomDatas.Where(x=> x.availableRoomCell.CellLetter.ToString() + x.availableRoomCell.CellNumber.ToString()  == _cell.CellLetter.ToString() + _cell.CellNumber.ToString()).SingleOrDefault();
    }
}
public partial class RoomManager // Room Bonus Controller
{
    
    public void RemoveNpcInTheRoom(RoomData _currentRoom, NPCBehaviour _removeNpc)
    {
        if (_currentRoom.GetNpcsInTheMyRoom().Contains(_removeNpc))
        {
            if(_currentRoom.isHasStatue)
                statuesHandler.RemoveBonusExitedInTheRoom(_currentRoom, _removeNpc);
            _currentRoom.GetNpcsInTheMyRoom().Remove(_removeNpc);
        }
    }
    public void AddNpcInTheRoom(RoomData _currentRoom, NPCBehaviour _newNpc)
    {
        if (!_currentRoom.GetNpcsInTheMyRoom().Contains(_newNpc))
        {
            Debug.Log($"An NPC Entered {_currentRoom.availableRoomCell.CellLetter + " " + _currentRoom.availableRoomCell.CellNumber} Cell Room. Current NPC => " + _newNpc.name);
            if(_currentRoom.isHasStatue)
                statuesHandler.AddBonusEnteredInTheRoom(_currentRoom, _newNpc);
            _currentRoom.GetNpcsInTheMyRoom().Add(_newNpc);
        }
    }
    public void AddSavedStatues(EditObjData _statue)
    {
        StatueSlotHandler currentStateContent = FindObjectsOfType<StatueSlotHandler>().Where(x => x.MyRoomCode == (_statue._currentRoomCell.CellLetter.ToString() + _statue._currentRoomCell.CellNumber.ToString())).SingleOrDefault();

        Vector3 anaScale = currentStateContent.transform.localScale;

        GameObject Statue = Instantiate(statuesHandler.Statues[_statue.myStatueIndex], currentStateContent.transform);

        Vector3 prefabScale = new Vector3(Statue.transform.localScale.x / anaScale.x, Statue.transform.localScale.y / anaScale.y, Statue.transform.localScale.z / anaScale.z);
        Statue.transform.localScale = prefabScale;

        EditObjBehaviour currentStatueData = Statue.AddComponent<EditObjBehaviour>();
        currentStatueData.data = new EditObjData(_statue);
        currentStateContent.MyStatue = currentStatueData.data;
        statuesHandler.currentEditObjs.Remove(statuesHandler.editObjs.Where(x => x.ID == _statue.ID).SingleOrDefault());
    }
    public List<RoomData> GetDesiredNeighborRooms(RoomData _currentRoom)
    {
        List<RoomData> desiredRooms = new List<RoomData>();
        foreach (var room in RoomDatas)
        {
            CellLetter n_letter = room.availableRoomCell.CellLetter;
            int n_number = room.availableRoomCell.CellNumber;

            CellLetter c_letter = _currentRoom.availableRoomCell.CellLetter;
            int c_number = _currentRoom.availableRoomCell.CellNumber;
            // A5
            // B
            if ((n_letter == c_letter && n_number == c_number - 1) /* Mevcut Oda B3 ise */ || (n_letter == c_letter && n_number == c_number + 1) /* Mevcut Oda B5 ise */  || (n_letter == c_letter - 1 && n_number == c_number) /* Mevcut Oda A4 ise */  || (n_letter == c_letter + 1 && n_number == c_number) /* Mevcut Oda C4 ise */)
            {
                desiredRooms.Add(room);
                //Debug.Log("Desired Neighbor Room Code => " + n_letter.ToString() + n_number + " Current Room Code => " + c_letter.ToString()+c_number.ToString());
            }
        }
        if (desiredRooms.Count > 0)
        {
            return desiredRooms;
        }
        else
        {
            Debug.Log("Komsu oda bulunamadi.");
            return null;
        }
    }
    public List<RoomData> GetDesiredNeighborRooms(RoomData _currentRoom, int _howMuchRoom)
    {
        List<RoomData> desiredRooms = new List<RoomData>();
        foreach (var room in RoomDatas)
        {
            CellLetter n_letter = room.availableRoomCell.CellLetter;
            int n_number = room.availableRoomCell.CellNumber;

            CellLetter c_letter = _currentRoom.availableRoomCell.CellLetter;
            int c_number = _currentRoom.availableRoomCell.CellNumber;
            // A5
            // B
            if ((n_letter == c_letter && n_number == c_number - 1) /* Mevcut Oda B3 ise */ || (n_letter == c_letter && n_number == c_number + 1) /* Mevcut Oda B5 ise */  || (n_letter == c_letter - 1 && n_number == c_number) /* Mevcut Oda A4 ise */  || (n_letter == c_letter + 1 && n_number == c_number) /* Mevcut Oda C4 ise */)
            {
                desiredRooms.Add(room);
                Debug.Log("Desired Neighbor Room Code => " + n_letter.ToString() + n_number + " Current Room Code => " + c_letter.ToString() + c_number.ToString());
            }
            if (desiredRooms.Count >= _howMuchRoom)
            {
                break;
            }
        }
        if (desiredRooms.Count > 0)
        {
            return desiredRooms;
        }
        else
        {
            Debug.Log("Komsu oda bulunamadi.");
            return null;
        }
    }
    public IEnumerator UpdateAndActivatePictureElement(RoomData _room, PictureElement pe, bool activateRoom)
    {
        pe.UpdateVisual(true);

        // Coroutine'in tamamlanmasýný bekleyin
        yield return StartCoroutine(pe.IEUpdateVisual(true));

        if (!_room.isLock && pe._pictureData.TextureID > 0)
        {
            pe.SetImage(true);
            if (activateRoom)
            {
                ActivateRoomLocations(_room);
            }
        }
    }
}
