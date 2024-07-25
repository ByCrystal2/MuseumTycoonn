using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AutoRoomWallActiveCalculate : MonoBehaviour
{
    [MenuItem("Tools/Museum Of Excesses/Refresh The Rooms")]
    public static void RoomWallActivation()
    {
        List<RoomData> roomDatas = FindObjectsOfType<RoomData>().ToList();
        foreach (var room in roomDatas)
        {
            int childCount = room.gameObject.transform.GetChild(4).childCount;
            List<GameObject> doors = new List<GameObject>();
            for (int i = 0; i < childCount; i++)
            {
                GameObject DoorObj = room.gameObject.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject;
                doors.Add(DoorObj);
            }
            foreach (var door in doors)
            {
                door.SetActive(false);
            }
            foreach (var directionWall in room.DirectionWalls)
            {
                directionWall.SetActive(true);
            }
            for (int i = 0; i < room.Directions.Count; i++)
            {
                doors[(int)room.Directions[i]].SetActive(true);
                room.DirectionWalls[(int)room.Directions[i]].SetActive(false);
            }
            foreach (DoorDirection direction in Enum.GetValues(typeof(DoorDirection)))
            {
                if (!room.Directions.Contains(direction)) //Directionlardan birini icermiyorsa bu islemi uygula.
                {
                    room.DirectionPictures[(int)direction].SetActive(true);
                    //room.pictureDirections.Add(direction);
                }
                else
                    room.DirectionPictures[(int)direction].SetActive(false);
            }
        }
        Debug.Log("Rooms refreshed!");
    }


}
