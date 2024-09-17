using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreCustomizationDatasHandler : MonoBehaviour
{
    FirebaseFirestore db;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task AddCustomizationDataWithUserId(string userId, CharacterCustomizeData _customizeData)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        Query query = db.Collection("Customizations").WhereEqualTo("userID", userId);

        await query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                DocumentReference documentReference;

                if (snapshot.Documents.Count() > 0)
                {
                    DocumentSnapshot documentSnapshot = snapshot.Documents.FirstOrDefault();
                    documentReference = documentSnapshot.Reference;

                    Dictionary<string, object> updatedData = new Dictionary<string, object>
                    {
                        { "UnlockedCustomizeElementIDs", _customizeData.unlockedCustomizeElementIDs },
                        { "LastSelectedCustomizeCategory", _customizeData.LastSelectedCustomizeCategory },
                        { "LastSelectedCustomizeHeader", _customizeData.LastSelectedCustomizeHeader },
                        { "LastSelectedColorHeader", _customizeData.LastSelectedColorHeader }
                    };

                    documentReference.UpdateAsync(updatedData).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            CheckAndAddCustomizeData(documentReference, _customizeData);
                        }
                        else if (updateTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to update document: {updateTask.Exception}");
                        }
                    });                    
                }
                else
                {

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId },
                        { "UnlockedCustomizeElementIDs", _customizeData.unlockedCustomizeElementIDs },
                        { "LastSelectedCustomizeCategory", _customizeData.LastSelectedCustomizeCategory },
                        { "LastSelectedCustomizeHeader", _customizeData.LastSelectedCustomizeHeader },
                        { "LastSelectedColorHeader", _customizeData.LastSelectedColorHeader },
                    };

                    db.Collection("Customizations").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddCustomizeData(documentReference, _customizeData);
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

    private void CheckAndAddCustomizeData(DocumentReference documentReference, CharacterCustomizeData _customizeData)
    {
        // Alt koleksiyon olan PictureDatas'ý sorgula
        CollectionReference statueDatasRef = documentReference.Collection("CustomizationDatas");
        Query query = statueDatasRef.WhereEqualTo("SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Customize with number of slot {_customizeData.playerCustomizeData.selectedCustomizeSlot} already exists for user.");

                    UpdateCustomizationData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, _customizeData);
                }
                else
                {
                    // Belge yoksa yeni belge ekle
                    Dictionary<string, object> newCustomizeData = new Dictionary<string, object>
                    {
                        { "SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                    statueDatasRef.AddAsync(newCustomizeData).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            //Debug.Log($"Worker data with ID {_customizeData.ID} successfully added.");
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add worker data: {addTask.Exception}");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError($"Error querying worker data: {task.Exception}");
            }
        });
    }

    public async Task<List<WorkerData>> GetCustomizationDataInDatabase(string userId, List<int> _workerIds)
    {
        List<WorkerData> foundWorkers = new List<WorkerData>();

        try
        {
            //Kullaniciya ait belgeleri sorgula
            Query query = db.Collection("Customizations").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    CollectionReference workerIdsRef = documentSnapshot.Reference.Collection("WorkerDatas");
                    foreach (int _workerId in _workerIds)
                    {
                        Query workerQuery = workerIdsRef.WhereEqualTo("ID", _workerId);
                        QuerySnapshot workerQuerySnapshot = await workerQuery.GetSnapshotAsync();
                        WorkerData foundWorker;
                        foreach (DocumentSnapshot workerDocumentSnapshot in workerQuerySnapshot.Documents)
                        {
                            if (workerDocumentSnapshot.Exists)
                            {
                                // Eþleþen tabloyu bulun
                                var workerData = workerDocumentSnapshot.ToDictionary();
                                WorkerData helperWorkerData = WorkerManager.instance.GetAllWorkers().Where(x => x.ID == _workerId).SingleOrDefault().MyDatas;
                                Debug.Log("helperWorkerData.ID => " + helperWorkerData.ID);
                                List<int> workerRoomIds = new List<int>();
                                workerRoomIds = workerData.ContainsKey("WorkRoomsIDs") ? ((List<object>)workerData["WorkRoomsIDs"]).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                                Debug.Log("database workerRoomIds Count => " + workerRoomIds.Count);
                                if (workerRoomIds.Count <= 0) helperWorkerData = null;
                                //Debug.Log("newCustomizeData[\"WorkRoomsIDs\"" + ((List<int>)newCustomizeData["WorkRoomsIDs"]).Count);
                                if (helperWorkerData != null)
                                {
                                    foundWorker = helperWorkerData;
                                    foundWorker.WorkRoomsIDs.Clear();
                                    foundWorker.ID = workerData.ContainsKey("ID") ? Convert.ToInt32(workerData["ID"]) : 0;
                                    foundWorker.Level = workerData.ContainsKey("Level") ? Convert.ToInt32(workerData["Level"]) : 0;
                                    foundWorker.Name = workerData.ContainsKey("Name") ? workerData["Name"].ToString() : "";
                                    foundWorker.Age = workerData.ContainsKey("Age") ? Convert.ToInt32(workerData["Age"]) : 0;
                                    foundWorker.Height = workerData.ContainsKey("Height") ? Convert.ToSingle(workerData["Height"]) : 0;
                                    if (workerData.ContainsKey("WorkerIn"))
                                    {
                                        int workerInValue = Convert.ToInt32(workerData["WorkerIn"]);
                                        foundWorker.WorkerIn = (WorkerIn)workerInValue;
                                        Debug.Log("Enum WorkerIn Value => " + foundWorker.WorkerIn.ToString());
                                    }
                                    foreach (int id in workerRoomIds)
                                    {
                                        foundWorker.WorkRoomsIDs.Add(id);
                                    }
                                    foundWorkers.Add(foundWorker);
                                    break;
                                }
                            }
                        }
                    }
                    if (foundWorkers != null)
                    {
                        Debug.Log("Worker aktarimi sonlandi. Veri tabani worker sayisi => " + foundWorkers.Count);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting worker data: {ex.Message}");
        }

        return foundWorkers;
    }
    public void UpdateCustomizationData(string userId, CharacterCustomizeData _customizeData)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Customizations").WhereEqualTo("userID", userId);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    DocumentSnapshot documentSnapshot = snapshot.Documents.FirstOrDefault();
                    DocumentReference documentReference = documentSnapshot.Reference;

                    // Belge varsa, alt koleksiyon olan PictureDatas'ta tabloyu bul ve güncelle
                    CollectionReference customizationDatasRef = documentReference.Collection("CustomizationDatas");
                    Query customizeDataQuery = customizationDatasRef.WhereEqualTo("SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot);

                    customizeDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(customizeDataTask =>
                    {
                        if (customizeDataTask.IsCompleted)
                        {
                            QuerySnapshot customizeDataSnapshot = customizeDataTask.Result;

                            if (customizeDataSnapshot.Documents.Count() > 0)
                            {
                                foreach (var document in customizeDataSnapshot.Documents)
                                {
                                    List<PlayerExtraCustomizeData> playerExtraCustomizeDatas = _customizeData.playerCustomizeData.AllCustomizeData;
                                    int length1 = playerExtraCustomizeDatas.Count;
                                    for (int k = 0; k < length1; k++)
                                    {
                                        DocumentSnapshot customizeDataDocument = document;
                                        DocumentReference customizeDataRef = customizeDataDocument.Reference;

                                        PlayerExtraCustomizeData currentExtraCustomizeData = playerExtraCustomizeDatas[k];


                                        CollectionReference extraCustomizationDatasRef = documentReference.Collection("ExtraCustomizationDatas");
                                        Query extraCustomizeDataQuery = extraCustomizationDatasRef.WhereEqualTo("ID", currentExtraCustomizeData.ID);

                                        Dictionary<string, object> updates = new Dictionary<string, object>
                                        {
                                            { "IsFemale", currentExtraCustomizeData.isFemale},
                                            { "Timestamp", FieldValue.ServerTimestamp }
                                        };

                                        customizeDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                                        {
                                            if (updateTask.IsCompleted)
                                            {
                                                Debug.Log($"Worker data successfully updated for user {userId}");
                                            }
                                            else if (updateTask.IsFaulted)
                                            {
                                                Debug.LogError($"Failed to update worker data: {updateTask.Exception}");
                                            }
                                        });
                                    }
                                }
                                
                            }
                            else
                            {
                                //Debug.LogError($"No worker data found for ID: {_customizeData.ID}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Error querying picture data: {customizeDataTask.Exception}");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"No document found for user ID: {userId}");
                }
            }
            else
            {
                Debug.LogError($"Error querying documents: {task.Exception}");
            }
        });
    }
}
