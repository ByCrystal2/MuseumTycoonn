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
    [SerializeField] private GameObject[] DirectionWalls; // West-North-East-South Walls
    [SerializeField] public GameObject[] DirectionPictures; // West-North-East-South Pictures

    public List<int> MyRoomWorkersIDs = new List<int>();

    [SerializeField] public RoomCell availableRoomCell = new RoomCell();
    //UI
    public List<GameObject> Doors = new List<GameObject>();
    private GameObject RoomBlok;
    private GameObject RoofLock;
    public List<TextMeshProUGUI> MyRequiredMoneyTexts;
    private void Start()
    {
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
            foreach(var directionWall in DirectionWalls)
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
            RoofLock.SetActive(true);
            if (CurrentShoppingType != ShoppingType.RealMoney && isActive && RoomManager.instance.activeRoomsRequiredMoney > 0)
            {
                SetMyRequiredTexts(RoomManager.instance.activeRoomsRequiredMoney);
                Debug.Log("Oda Aktif Ve activeRoomsRequiredMoney 0'dan buyuk" + RoomManager.instance.activeRoomsRequiredMoney);
            }
            else
            {
                SetMyRequiredTexts(RequiredMoney);
                Debug.Log("Oda Aktif deðil Ve activeRoomsRequiredMoney 0'dan kucuk => " + RoomManager.instance.activeRoomsRequiredMoney + "|| My Required Money => " + RequiredMoney);
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
            RoomManager.instance.ActivateRoomLocations(this);
            RoomBlok.SetActive(false);
            RoofLock.SetActive(false);
        }
        
        Debug.Log("Oda start tamamlandi.");
    }
    public void SetMyRequiredTexts(float _RequiredMoney)
    {
        foreach (var requiredText in MyRequiredMoneyTexts)
        {
            requiredText.text = _RequiredMoney.ToString();
        }
    }
    public void IsPurchased(bool _isPurchased)
    {
        if (_isPurchased)
        {
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            RoomBlok.SetActive(false);
            RoofLock.SetActive(false);
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
    public List<int> MyRoomWorkersIDs = new List<int>();
}

[System.Serializable]
public struct RoomCell
{
    public CellLetter CellLetter;
    public int CellNumber;
}
