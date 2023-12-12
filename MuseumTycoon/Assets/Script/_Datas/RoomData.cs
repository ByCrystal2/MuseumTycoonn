using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public int ID;
    public float RequiredMoney;    
    public RoomType CurrentRoomType = RoomType.Normal;
    public ShoppingType CurrentShoppingType;
    public List<DoorDirection> Directions = new List<DoorDirection>();
    public bool isLock = true;

    
    public bool isActive = false;
    
    [SerializeField] public RoomCell availableRoomCell = new RoomCell();

    public List<GameObject> Doors = new List<GameObject>();
    private GameObject RoomBlok;
    private void Awake()
    {
        
        RoomBlok = gameObject.GetComponentInChildren<RoomBlokClickHandler>().gameObject;
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
        }
        else
        {
            foreach (var door in Doors)
            {
                door.SetActive(false);
            }
            RoomBlok.SetActive(true);
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
public struct RoomCell
{
    public CellLetter CellLetter;
    public int CellNumber;
}
