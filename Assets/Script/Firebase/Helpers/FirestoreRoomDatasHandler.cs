using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreRoomDatasHandler : MonoBehaviour
{
    FirebaseFirestore db;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    public async System.Threading.Tasks.Task AddRoomsWithUserId(string userId, List<RoomData> _roomDatas)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        foreach (var _roomData in _roomDatas)
        {
            await IERoomDataProcces(userId, _roomData);
        }
    }

    public async System.Threading.Tasks.Task IERoomDataProcces(string userId, RoomData _roomData)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        Query query = db.Collection("Rooms").WhereEqualTo("userID", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count() == 0)
        {
            // Belge yoksa yeni belge oluþtur ve alt koleksiyon ekle
            Dictionary<string, object> newDocument = new Dictionary<string, object>
        {
            { "userID", userId }
        };

            DocumentReference documentReference = await db.Collection("Rooms").AddAsync(newDocument);

            if (documentReference == null)
            {
                Debug.LogError("Failed to add new document.");
                return;
            }

            Debug.Log($"User document created for user {userId}");
            await IECheckAndAddRoomData(documentReference, _roomData, userId);
        }
        else
        {
            DocumentReference documentReference = snapshot.Documents.FirstOrDefault().Reference;
            await IECheckAndAddRoomData(documentReference, _roomData, userId);
        }
    }


    private async System.Threading.Tasks.Task IECheckAndAddRoomData(DocumentReference documentReference, RoomData _roomData, string _userId)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        // Alt koleksiyon olan RoomDatas'ý sorgula
        CollectionReference roomDatasRef = documentReference.Collection("RoomDatas");
        Query query = roomDatasRef.WhereEqualTo("ID", _roomData.ID);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count() > 0)
        {
            Debug.Log($"Room with ID {_roomData.ID} already exists for user.");
            await IEUpdateRoomData(_userId, _roomData.ID);
        }
        else
        {
            // Belge yoksa yeni belge ekle
            Dictionary<string, object> roomData = new Dictionary<string, object>
        {
            { "RoomCell", _roomData.availableRoomCell.CellLetter.ToString() + _roomData.availableRoomCell.CellNumber },
            { "IsActive", _roomData.isActive },
            { "IsLock", _roomData.isLock },
            { "ID", _roomData.ID },
            { "IsHasStatue", _roomData.isHasStatue },
            { "StatueID", _roomData.GetMyStatueInTheMyRoom()?.ID },
            { "RequiredMoney", _roomData.RequiredMoney },
            { "RoomWorkersIDs", _roomData.MyRoomWorkersIDs },
            { "Timestamp", FieldValue.ServerTimestamp }
        };

            DocumentReference addTask = await roomDatasRef.AddAsync(roomData);

            if (addTask == null)
            {
                Debug.LogError("Failed to add room data.");
            }
            else
            {
                Debug.Log($"Room data with ID {_roomData.ID} successfully added.");
            }
        }
    }

    private async System.Threading.Tasks.Task IEUpdateRoomData(string userId, int roomDataId)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        RoomData currentRoom = RoomManager.instance.RoomDatas.SingleOrDefault(x => x.ID == roomDataId);
        if (currentRoom == null)
        {
            Debug.LogError($"No room found with ID: {roomDataId}");
            return;
        }

        Debug.Log("currentRoom.ID is => " + currentRoom.ID);
        Query query = db.Collection("Rooms").WhereEqualTo("userID", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count() == 0)
        {
            Debug.LogError($"No document found for user ID: {userId}");
            return;
        }

        DocumentReference documentReference = snapshot.Documents.FirstOrDefault().Reference;

        // Belge varsa, alt koleksiyon olan RoomDatas'ta tabloyu bul ve güncelle
        CollectionReference roomDatasRef = documentReference.Collection("RoomDatas");
        Query roomDataQuery = roomDatasRef.WhereEqualTo("ID", roomDataId);
        QuerySnapshot roomDataSnapshot = await roomDataQuery.GetSnapshotAsync();

        Debug.Log("roomDataSnapshot.Documents.Count() => " + roomDataSnapshot.Documents.Count());
        if (roomDataSnapshot.Documents.Count() == 0)
        {
            Debug.LogError($"No room data found for ID: {roomDataId}");
            return;
        }

        DocumentReference roomDataRef = roomDataSnapshot.Documents.FirstOrDefault().Reference;

        if (currentRoom.GetMyStatueInTheMyRoom() != null)
        {
            Debug.Log("currentRoom.GetMyStatueInTheMyRoom().ID => " + currentRoom.GetMyStatueInTheMyRoom().ID);
        }
        else
        {
            Debug.Log("currentRoom.GetMyStatueInTheMyRoom() is null.");
        }

        Dictionary<string, object> updates = new Dictionary<string, object>
    {
        { "IsActive", currentRoom.isActive },
        { "IsHasStatue", currentRoom.isHasStatue },
        { "IsLock", currentRoom.isLock },
        { "RequiredMoney", currentRoom.RequiredMoney },
        { "RoomWorkersIDs", currentRoom.MyRoomWorkersIDs },
        { "StatueID", currentRoom.GetMyStatueInTheMyRoom()?.ID },
        { "Timestamp", FieldValue.ServerTimestamp }
    };

        try
        {
            await roomDataRef.UpdateAsync(updates);
            Debug.Log($"Room data successfully updated for user {userId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update room data: {ex}");
        }
    }

    public async Task<List<RoomData>> GetRoomsInDatabase(string userId, List<int> roomIds)
    {
        List<RoomData> foundRooms = new List<RoomData>();
        Debug.Log("Rooms test 1 complated.");
        try
        {
            //Kullanýcýya ait belgeleri sorgula
            Query query = db.Collection("Rooms").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot =  await query.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Debug.Log("Rooms test 2 complated.");
                    // Belgenin RoomDatas alt koleksiyonuna eriþ
                    List<EditObjData> eobs = await FirestoreManager.instance.statueDatasHandler.GetStatuesInDatabase(userId, RoomManager.instance.statuesHandler.GetStatueDatas());
                    Debug.Log("Statues test complated.");
                    CollectionReference roomDatasRef = documentSnapshot.Reference.Collection("RoomDatas");
                    foreach (int _id in roomIds)
                    {
                        Debug.Log("Rooms test 3 complated.");
                        Query roomQuery = roomDatasRef.WhereEqualTo("ID", _id);
                        QuerySnapshot roomQuerySnapshot = await roomQuery.GetSnapshotAsync();
                        RoomData foundRoom;
                        EditObjData foundEob;
                        foreach (DocumentSnapshot roomDocumentSnapshot in roomQuerySnapshot.Documents)
                        {
                            if (roomDocumentSnapshot.Exists)
                            {
                                // Eþleþen tabloyu bulun
                                var roomData = roomDocumentSnapshot.ToDictionary();
                                int statueId = roomData.ContainsKey("StatueID") ? Convert.ToInt32(roomData["StatueID"]) : 0;
                                foundEob = eobs.Where((x) => x.ID == statueId).SingleOrDefault();

                                int id = roomData.ContainsKey("ID") ? Convert.ToInt32(roomData["ID"]) : 0;
                                RoomData helperRoomData = RoomManager.instance.RoomDatas.Where(x => x.ID == id).SingleOrDefault();
                                if (helperRoomData != null)
                                {
                                    foundRoom = helperRoomData;
                                    foundRoom.isActive = roomData.ContainsKey("IsActive") && Convert.ToBoolean(roomData["IsActive"]);
                                    foundRoom.isHasStatue = roomData.ContainsKey("IsHasStatue") && Convert.ToBoolean(roomData["IsHasStatue"]);
                                    foundRoom.isLock = roomData.ContainsKey("IsLock") && Convert.ToBoolean(roomData["IsLock"]);
                                    foundRoom.MyRoomWorkersIDs = roomData.ContainsKey("RoomWorkersIDs")
                            ? ((List<object>)roomData["RoomWorkersIDs"]).Select(x => Convert.ToInt32(x)).ToList()
                            : new List<int>();
                                    if (foundEob != null)
                                    {
                                        Debug.Log("Database Statue ID is " + foundEob.ID);
                                        foundRoom.SetMyStatue(foundEob);
                                    }
                                    foundRooms.Add(foundRoom);
                                    break;
                                }
                            }
                        }
                    }
                    if (foundRooms != null)
                    {
                        Debug.Log("Oda aktarimi sonlandi. Veri tabani oda sayisi => " + foundRooms.Count);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting room data: {ex.Message}");
        }
        Debug.Log("Rooms test 4 complated.");
        return foundRooms;
    }

}
