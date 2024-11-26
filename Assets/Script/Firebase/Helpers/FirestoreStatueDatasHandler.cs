using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreStatueDatasHandler : MonoBehaviour
{
    FirebaseFirestore db;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    public void AddOrUpdateStatueWithUserId(string userId, EditObjData _statue)
    {
        // Kullan�c� ID'si ile belgeyi sorgula
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        Query query = db.Collection("Statues").WhereEqualTo("userID", userId);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                DocumentReference documentReference;

                if (snapshot.Documents.Count() > 0)
                {
                    DocumentSnapshot documentSnapshot = snapshot.Documents.FirstOrDefault();
                    documentReference = documentSnapshot.Reference;

                    CheckAndAddStatueData(documentReference, _statue);
                }
                else
                {

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId }
                    };

                    db.Collection("Statues").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddStatueData(documentReference, _statue);
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add new document: {addTask.Exception}");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError($"Error querying documents: {task.Exception}");
            }
        });
    }

    private void CheckAndAddStatueData(DocumentReference documentReference, EditObjData _statue)
    {
        // Alt koleksiyon olan PictureDatas'� sorgula
        CollectionReference statueDatasRef = documentReference.Collection("StatueDatas");
        Query query = statueDatasRef.WhereEqualTo("ID", _statue.ID);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Skill with ID {_statue.ID} already exists for user.");
                    // Mevcut belgeyi g�ncelle
                    DocumentSnapshot existingStatue = snapshot.Documents.First();
                    Dictionary<string, object> updatedStatueData = new Dictionary<string, object>
                {
                    { "IsPurchased", _statue.IsPurchased },
                    { "IsLocked", _statue.IsLocked },
                    { "OnSlot", _statue.OnSlot },
                    { "BonusIDs", _statue.Bonusses.Select(b => b.ID).ToList() },
                    { "StatueIndex", _statue.myStatueIndex },
                    { "TargetRoomCell", _statue._currentRoomCell.CellLetter.ToString() + _statue._currentRoomCell.CellNumber },
                    { "Timestamp", FieldValue.ServerTimestamp }
                };

                    existingStatue.Reference.UpdateAsync(updatedStatueData).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"Statue data with ID {_statue.ID} successfully updated.");
                        }
                        else if (updateTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to update statue data: {updateTask.Exception}");
                        }
                    });
                }
                else
                {
                    // Belge yoksa yeni belge ekle
                    List<int> bonusIds = new List<int>();
                    int length = _statue.Bonusses.Count;
                    for (int i = 0; i < length; i++)
                    {
                        bonusIds.Add(_statue.Bonusses[i].ID);
                    }
                    Dictionary<string, object> statueData = new Dictionary<string, object>
                    {
                        { "ID", _statue.ID },
                        { "IsPurchased", _statue.IsPurchased },
                        { "IsLocked", _statue.IsLocked },
                        { "OnSlot", _statue.OnSlot },
                        { "BonusIDs", bonusIds },
                        { "StatueIndex", _statue.myStatueIndex },
                        { "TargetRoomCell", _statue._currentRoomCell.CellLetter.ToString() + _statue._currentRoomCell.CellNumber  },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                    statueDatasRef.AddAsync(statueData).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            Debug.Log($"Statue data with ID {_statue.ID} successfully added.");
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add statue data: {addTask.Exception}");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError($"Error querying skill data: {task.Exception}");
            }
        });
    }
    public async Task<List<EditObjData>> GetStatuesInDatabase(string userId, List<int> _statueIds)
    {
        List<EditObjData> foundStatues = new List<EditObjData>();
        Debug.Log("Statues test 1 complated.");
        try
        {
            // Kullan�c�ya ait belgeleri sorgula
            Query query = db.Collection("Statues").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Debug.Log("Statues test 2 complated.");
                    CollectionReference statueIdsRef = documentSnapshot.Reference.Collection("StatueDatas");

                    foreach (int id in _statueIds)
                    {
                        Query statueQuery = statueIdsRef.WhereEqualTo("ID", id);
                        QuerySnapshot statueQuerySnapshot = await statueQuery.GetSnapshotAsync();
                        EditObjData foundStatue;
                        foreach (DocumentSnapshot statueDocumentSnapshot in statueQuerySnapshot.Documents)
                        {
                            if (statueDocumentSnapshot.Exists)
                            {
                                // E�le�en tabloyu bulun
                                var statueData = statueDocumentSnapshot.ToDictionary();
                                EditObjData helperStatueData = RoomManager.instance.statuesHandler.GetStatueWithID(id);
                                if (helperStatueData != null)
                                {
                                    foundStatue = helperStatueData;
                                    foundStatue.Bonusses.Clear();
                                    string cellString = statueData.ContainsKey("TargetRoomCell") ? statueData["TargetRoomCell"].ToString() : "null";
                                    if (cellString != "null")
                                    {
                                        if (Enum.TryParse(cellString[0].ToString(), true, out CellLetter targetRoomCell))
                                        {
                                            foundStatue._currentRoomCell = new RoomCell(targetRoomCell, int.Parse(cellString[1].ToString()));
                                        }
                                    }
                                    foundStatue.IsPurchased = statueData.ContainsKey("IsPurchased") && Convert.ToBoolean(statueData["IsPurchased"]);
                                    foundStatue.IsLocked = statueData.ContainsKey("IsLocked") && Convert.ToBoolean(statueData["IsLocked"]);
                                    foundStatue.OnSlot = statueData.ContainsKey("OnSlot") && Convert.ToBoolean(statueData["OnSlot"]);
                                    List<int> statueBonusIds = statueData.ContainsKey("BonusIDs") ? ((List<object>)statueData["BonusIDs"]).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                                    foundStatue.myStatueIndex = statueData.ContainsKey("StatueIndex") ? Convert.ToInt32(statueData["StatueIndex"]) : -1;

                                    foreach (int _id in statueBonusIds)
                                    {
                                        Bonus databaseBonus = RoomManager.instance.statuesHandler.GetStatueBonusInAllBonusWithId(_id);
                                        foundStatue.Bonusses.Add(databaseBonus);
                                    }
                                    foundStatues.Add(foundStatue);
                                    break;
                                }
                            }
                        }
                    }
                    

                    if (foundStatues != null)
                    {
                        Debug.Log("Statue aktarimi sonlandi. Veri tabani statue sayisi => " + foundStatues.Count);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting statue data: {ex.Message}");
        }
        Debug.Log("Statues test 3 complated.");
        return foundStatues;
    }
    public async Task DeleteStatueData(string userId, int _statueId)
    {
        Debug.Log("Delete statues process started.");
        try
        {
            // Kullan�c�ya ait belgeleri sorgula
            Query query = db.Collection("Statues").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Debug.Log("Matching user statue found.");
                    CollectionReference statueIdsRef = documentSnapshot.Reference.Collection("StatueDatas");

                    
                    Query statueQuery = statueIdsRef.WhereEqualTo("ID", _statueId);
                    QuerySnapshot statueQuerySnapshot = await statueQuery.GetSnapshotAsync();

                    foreach (DocumentSnapshot statueDocumentSnapshot in statueQuerySnapshot.Documents)
                    {
                        if (statueDocumentSnapshot.Exists)
                        {
                            Debug.Log($"Deleting statue with ID: {_statueId}");
                            // Belgeyi sil
                            await statueDocumentSnapshot.Reference.DeleteAsync();
                        }
                    }
                    
                    Debug.Log("Statue deletion process completed for current user.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting statue data: {ex.Message}");
        }
        Debug.Log("Delete statues process ended.");
    }


}
