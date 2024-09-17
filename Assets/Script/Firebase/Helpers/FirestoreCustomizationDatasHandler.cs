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

    public async System.Threading.Tasks.Task AddCustomizationDataWithUserId(string userId, WorkerData _worker)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        Query query = db.Collection("Customization").WhereEqualTo("userID", userId);

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

                    CheckAndAddStatueData(documentReference, _worker);
                }
                else
                {

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId }
                    };

                    db.Collection("Customization").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddStatueData(documentReference, _worker);
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

    private void CheckAndAddStatueData(DocumentReference documentReference, WorkerData _worker)
    {
        // Alt koleksiyon olan PictureDatas'ý sorgula
        CollectionReference statueDatasRef = documentReference.Collection("WorkerDatas");
        Query query = statueDatasRef.WhereEqualTo("ID", _worker.ID);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Worker with ID {_worker.ID} already exists for user.");

                    UpdateWorkerData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, _worker);
                }
                else
                {
                    // Belge yoksa yeni belge ekle
                    List<int> workRoomIds = new List<int>();
                    int length = _worker.WorkRoomsIDs.Count;
                    for (int i = 0; i < length; i++)
                    {
                        workRoomIds.Add(_worker.WorkRoomsIDs[i]);
                    }
                    Dictionary<string, object> workerData = new Dictionary<string, object>
                    {
                        { "ID", _worker.ID },
                        { "Name", _worker.Name},
                        { "Age", _worker.Age},
                        { "Height", _worker.Height},
                        { "Level", _worker.Level},
                        { "WorkerIn", _worker.WorkerIn },
                        { "WorkRoomsIDs", _worker.WorkRoomsIDs },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                    statueDatasRef.AddAsync(workerData).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            Debug.Log($"Worker data with ID {_worker.ID} successfully added.");
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
            Query query = db.Collection("Customization").WhereEqualTo("userID", userId);
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
                                //Debug.Log("workerData[\"WorkRoomsIDs\"" + ((List<int>)workerData["WorkRoomsIDs"]).Count);
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
    public void UpdateWorkerData(string userId, WorkerData _workerData)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Workers").WhereEqualTo("userID", userId);

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
                    CollectionReference workerDatasRef = documentReference.Collection("WorkerDatas");
                    Query workerDataQuery = workerDatasRef.WhereEqualTo("ID", _workerData.ID);

                    workerDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(workerDataTask =>
                    {
                        if (workerDataTask.IsCompleted)
                        {
                            QuerySnapshot workerDataSnapshot = workerDataTask.Result;

                            if (workerDataSnapshot.Documents.Count() > 0)
                            {
                                DocumentSnapshot workerDataDocument = workerDataSnapshot.Documents.FirstOrDefault();
                                DocumentReference workerDataRef = workerDataDocument.Reference;

                                Dictionary<string, object> updates = new Dictionary<string, object>
                            {
                                { "Level", _workerData.Level },
                                { "WorkerIn", _workerData.WorkerIn },
                                { "WorkRoomsIDs", _workerData.WorkRoomsIDs },
                                { "Timestamp", FieldValue.ServerTimestamp }
                            };

                                workerDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
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
                            else
                            {
                                Debug.LogError($"No worker data found for ID: {_workerData.ID}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Error querying picture data: {workerDataTask.Exception}");
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
