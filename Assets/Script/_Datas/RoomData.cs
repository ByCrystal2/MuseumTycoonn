using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public int ID;
    public string IAP_ID;
    public float RequiredMoney;    
    public RoomType CurrentRoomType = RoomType.Normal;
    public ShoppingType CurrentShoppingType;
    public List<DoorDirection> Directions = new List<DoorDirection>(); // Max 4!
    public List<DoorDirection> pictureDirections = new List<DoorDirection>(); // Max 3!
    public bool isLock = true;
    public bool isActive = false;
    public bool isHasStatue;
    [SerializeField] private GameObject[] DirectionWalls; // West-North-East-South Walls
    [SerializeField] public GameObject[] DirectionPictures; // West-North-East-South Pictures

    public List<int> MyRoomWorkersIDs = new List<int>();

    [SerializeField] public RoomCell availableRoomCell = new RoomCell();
    [SerializeField] GameObject RoomEditingCamera; 
    //UI
    public List<GameObject> Doors = new List<GameObject>();
    private GameObject RoomBlok;
    [SerializeField] GameObject NotPurchasedBlok;
    private GameObject RoofLock;
    public List<TextMeshProUGUI> MyRequiredMoneyTexts;

    public Transform LocationHolder;

    List<NPCBehaviour> NPCsInTheRoom = new List<NPCBehaviour>();
    EditObjData MyStatue;
    public Transform CenterPoint;
    private void Start()
    {
        
        
    }
    public List<NPCBehaviour> GetNpcsInTheMyRoom()
    {
        return NPCsInTheRoom;
    }
    public EditObjData GetMyStatueInTheMyRoom()
    {
        return MyStatue;
    }
    public void SetMyStatue(EditObjData _newStatue)
    {
        MyStatue = _newStatue;
    }
    public async System.Threading.Tasks.Task LoadThisRoom()
    {
        await FirestoreManager.instance.roomDatasHandler.GetRoomInDatabase("ahmet123", this.ID).ContinueWithOnMainThread(async (getTask) =>
        {
            try
            {
                if (getTask.IsCompleted && !getTask.IsFaulted)
                {
                    RoomData databaseRoom = getTask.Result;
                    Debug.Log("Database room data retrieval completed.");

                    if (databaseRoom != null)
                    {
                        Debug.Log($"databaseRoom.ID => {databaseRoom.ID} databaseRoom.isActive => {databaseRoom.isActive} databaseRoom.StatueID => {databaseRoom.GetMyStatueInTheMyRoom()?.ID}");

                        RoomData myRoom = GetComponent<RoomData>();
                        myRoom = databaseRoom;
                        Debug.Log("myRoom.ID is " + myRoom.ID + " databaseRoom.ID is " + databaseRoom.ID);
                        if (myRoom.GetMyStatueInTheMyRoom() != null)
                        {
                            Debug.Log($"before myRoom.GetMyStatueInTheMyRoom().IsPurchased => {myRoom.GetMyStatueInTheMyRoom().IsPurchased}");
                            var myStatue = myRoom.GetMyStatueInTheMyRoom();
                            Debug.Log($"myStatue Infos: ID:{myStatue.ID} + Name:{myStatue.Name} + RoomCell:{myStatue._currentRoomCell.CellLetter.ToString() + myStatue._currentRoomCell.CellNumber} + Bonusses.Count:{myStatue.Bonusses.Count}");
                            myStatue.SetIsPurchased();
                            Debug.Log($"myStatue Infos: FocusedLevel:{myStatue.FocusedLevel}");
                            RoomManager.instance.AddSavedStatues(myStatue);
                            RoomManager.instance.statuesHandler.activeEditObjs.Add(myStatue);
                            Debug.Log($"after myRoom.GetMyStatueInTheMyRoom().IsPurchased => {myRoom.GetMyStatueInTheMyRoom().IsPurchased}");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error occurred: " + getTask.Exception + " Room code => " + this.availableRoomCell.CellLetter.ToString() + this.availableRoomCell.CellNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Room Database Loading Error:"+ex.Message + " Room code => " + this.availableRoomCell.CellLetter.ToString() + this.availableRoomCell.CellNumber.ToString());
            }
            
        });

        if (CurrentShoppingType == ShoppingType.RealMoney)
            IAP_ID = Constant.instance.IAPIDCompany + Constant.instance.IAPIDGame + CurrentRoomType.ToString().ToLower() + "x" + 1 + "_" + CurrentShoppingType.ToString().ToLower() + "_" + ((int)RequiredMoney).ToString(); //com_kosippysudio_museumtycoon_gold5000x_realmoney_10

        RoomBlok = gameObject.GetComponentInChildren<RoomBlokClickHandler>().gameObject;
        RoofLock = gameObject.GetComponentInChildren<PanelClickHandler>().gameObject.GetComponentInParent<Canvas>().gameObject;

        if (isLock)
        {
            int childCount = gameObject.transform.GetChild(4).childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject DoorObj = gameObject.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject;
                Doors.Add(DoorObj);
            }
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            foreach (var directionWall in DirectionWalls)
            {
                directionWall.SetActive(true);
            }
            for (int i = 0; i < Directions.Count; i++)
            {
                Doors[(int)Directions[i]].SetActive(true);
                DirectionWalls[(int)Directions[i]].SetActive(false);
            }
            foreach (DoorDirection direction in Enum.GetValues(typeof(DoorDirection)))
            {
                if (!Directions.Contains(direction)) //Directionlardan birini icermiyorsa bu islemi uygula.
                {
                    DirectionPictures[(int)direction].SetActive(true);
                    pictureDirections.Add(direction);
                }
                else
                    DirectionPictures[(int)direction].SetActive(false);
            }
            RoomBlok.SetActive(true);
            NotPurchasedBlok.SetActive(true);
            RoofLock.SetActive(true);
            if (CurrentShoppingType != ShoppingType.RealMoney && isActive && GameManager.instance.ActiveRoomsRequiredMoney> 0)
            {
                SetMyRequiredTexts(GameManager.instance.ActiveRoomsRequiredMoney);
                StartCoroutine(WaitForRoomUISHandler());

                Debug.Log("Oda Aktif Ve activeRoomsRequiredMoney 0'dan buyuk" + GameManager.instance.ActiveRoomsRequiredMoney);
            }
            else
            {
                SetMyRequiredTexts("Not Purchased");
                Debug.Log("Oda Aktif deðil Ve activeRoomsRequiredMoney 0'dan kucuk => " + GameManager.instance.ActiveRoomsRequiredMoney + "|| My Required Money => " + RequiredMoney);
            }
        }
        else
        {
            int childCount = gameObject.transform.GetChild(4).childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject DoorObj = gameObject.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject;
                Doors.Add(DoorObj);
            }
            foreach (var directionWall in DirectionWalls)
            {
                directionWall.SetActive(true);
            }

            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            for (int i = 0; i < Directions.Count; i++)
            {
                DirectionWalls[(int)Directions[i]].SetActive(false);
            }
            foreach (DoorDirection direction in Enum.GetValues(typeof(DoorDirection)))
            {
                if (!Directions.Contains(direction)) //Directionlardan birini icermiyorsa bu islemi uygula.
                {
                    DirectionPictures[(int)direction].SetActive(true);
                    pictureDirections.Add(direction);
                }
                else
                    DirectionPictures[(int)direction].SetActive(false);
            }
            //RoomBlok.SetActive(false);
            RoofLock.SetActive(false);
            NotPurchasedBlok.SetActive(false);
            StartCoroutine(WaitForRoomUISHandler());
            foreach (DoorDirection pictureDirection in pictureDirections)
            {
                //Debug.Log("DoorDirection ForEach Start!");
                int length3 = DirectionPictures[(int)pictureDirection].transform.childCount;
                for (int i = 0; i < length3; i++)
                {
                    //Debug.Log("room.DirectionPictures[(int)pictureDirection].transform name: " + DirectionPictures[(int)pictureDirection].transform.name);
                    if (DirectionPictures[(int)pictureDirection].transform.GetChild(i).TryGetComponent(out PictureElement pe))
                    {
                        //Debug.Log(availableRoomCell.CellLetter + availableRoomCell.CellNumber + " Hucreli odanin " + DirectionPictures[(int)pictureDirection].name + " Yonlu Resimler contentinin " + DirectionPictures[(int)pictureDirection].transform.GetChild(i).name + " Adli tablosu bulunmaktadir.");

                        //PictureData currentPictureData = GameManager.instance.CurrentSaveData.CurrentPictures.Where(x => x.id == pe._pictureData.id).SingleOrDefault();
                        Debug.Log("RoomCode => " + availableRoomCell.CellLetter.ToString() + availableRoomCell.CellNumber.ToString() + " pe.name => " + pe.name + " and pe._pictureData.id => " + pe._pictureData.id);
                        await FirestoreManager.instance.pictureDatasHandler.GetPictureInDatabase("ahmet123", pe._pictureData.id)
                        .ContinueWithOnMainThread(async (task) =>
                        {
                            if (task.IsCompleted && !task.IsFaulted)
                            {
                                PictureData databasePicture = task.Result;
                                if (databasePicture != null)
                                {
                                    Debug.Log("databaseRoom.painterData.ID => " + databasePicture.painterData.ID + " databaseRoom.isActive => " + databasePicture.isActive + " databaseRoom.TextureID => " + databasePicture.TextureID);

                                    if (databasePicture.isActive)
                                    {
                                        pe._pictureData = databasePicture;
                                        pe.UpdateVisual(true);
                                        if (!isLock)
                                        {
                                            if (pe._pictureData.TextureID > 0)
                                                pe.SetImage(true);
                                            RoomManager.instance.ActivateRoomLocations(this);
                                        }
                                    }
                                    else
                                    {
                                        //Eger mevcut PictureElement'in Picture ID'si ile Database PictureDatas dokumani TabloID eslesiyorsa ve bu eslesmeden gelen pictureData'nin isActivesi false ise buraya girer.

                                    }

                                }
                            }
                            else
                            {
                                Debug.LogError("Hata olustu: " + task.Exception);
                            }
                        });


                    }
                }
            }
        }

    }


    IEnumerator WaitForRoomUISHandler()
    {
        //while (UIController.instance.roomUISPanelController.GetRoomUIS().Count <= 0)
        //    yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        RoomUIHandler _targetHandler = GetComponentInChildren<RoomUIHandler>();
        _targetHandler.UpdateMyUI();
    }
    public void SetRoomBlockPanelActive(bool _isActive)
    {
        RoomBlok.GetComponent<BoxCollider>().enabled = _isActive;
    }
    public void SetMyRequiredTexts(float _RequiredMoney)
    {
        foreach (var requiredText in MyRequiredMoneyTexts)
        {
            requiredText.fontStyle = FontStyles.Normal;
            requiredText.fontSize = 1.1f;
            requiredText.text = _RequiredMoney.ToString();
        }
    }
    public void SetMyRequiredTexts(string _RequiredMoney)
    {
        foreach (var requiredText in MyRequiredMoneyTexts)
        {
            requiredText.fontStyle = FontStyles.Underline;
            requiredText.fontSize = 0.6f;
            requiredText.text = _RequiredMoney;
        }
    }
    public void SetActivationMyRoomEditingCamera(bool _active)
    {
        RoomEditingCamera.SetActive(_active);
    }
    public void IsPurchased(bool _isPurchased)
    {
        if (_isPurchased)
        {
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            //RoomBlok.SetActive(false);
            RoofLock.SetActive(false);
            NotPurchasedBlok.SetActive(false);
        }
        else
        {
            int childCount = gameObject.transform.GetChild(4).childCount;
            for (int i = 0; i < childCount; i++)
            {
                Doors.Add(gameObject.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject);
            }
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            for (int i = 0; i < Directions.Count; i++)
            {
                Doors[(int)Directions[i]].SetActive(true);
            }
            RoomBlok.SetActive(true);
            NotPurchasedBlok.SetActive(true);
            RoofLock.SetActive(true);
        }
    }
}
public enum RoomType
{
    None,
    Normal,
    Special
}

public enum DoorDirection
{
    West,
    North,
    East,
    South
}
public enum CellLetter
{
    None,
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,    
    Y,
    Z,
}

[System.Serializable]
public class RoomSaveData
{
    public string availableRoomCell;
    public bool isLock = true;
    public bool isActive = false;
    public float RequiredMoney;
    public bool IsHasStatue = false;
    public EditObjData MyStatue;
    public List<int> MyRoomWorkersIDs = new List<int>();
}

[System.Serializable]
public struct RoomCell
{
    public CellLetter CellLetter;
    public int CellNumber;

    public RoomCell(CellLetter _letter, int _number)
    {
        CellLetter = _letter;
        CellNumber = _number;
    }
}
