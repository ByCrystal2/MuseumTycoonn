using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public int ID;
    public string IAP_ID;
    public float RequiredMoney;    
    public RoomType CurrentRoomType = RoomType.Normal;
    public ShoppingType CurrentShoppingType;
    public List<DoorDirection> Directions = new List<DoorDirection>();
    public bool isLock = true;
    public bool isActive = false;
    
    [SerializeField] public RoomCell availableRoomCell = new RoomCell();

    public List<GameObject> Doors = new List<GameObject>();
    private GameObject RoomBlok;
    private GameObject RoofLock;
    private void Start()
    {
        if (CurrentShoppingType == ShoppingType.RealMoney)
            IAP_ID = Constant.instance.IAPIDCompany + Constant.instance.IAPIDGame + CurrentRoomType.ToString().ToLower() + "x" + 1 + "_" + CurrentShoppingType.ToString().ToLower() + "_" + RequiredMoney.ToString(); //com_kosippysudio_museumtycoon_gold5000x_realmoney_10
        RoomBlok = gameObject.GetComponentInChildren<RoomBlokClickHandler>().gameObject;
        RoofLock = gameObject.GetComponentInChildren<PanelClickHandler>().gameObject.GetComponentInParent<Canvas>().gameObject;

        if (isLock)
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
        else
        {
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            RoomBlok.SetActive(false);
            RoofLock.SetActive(false);
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
}

[System.Serializable]
public struct RoomCell
{
    public CellLetter CellLetter;
    public int CellNumber;
}
