using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Purchasing;
using System;

public class FirestoreManager : MonoBehaviour
{
    public FirebaseFirestore db;
    public FirestoreItemsManager firestoreItemsManager;
    public static FirestoreManager instance { get; private set;}
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);       
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(innerTask =>
                {
                    Firebase.DependencyStatus dependencyStatus = innerTask.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        // Firebase is ready to use.
                        db = FirebaseFirestore.DefaultInstance;
                        firestoreItemsManager = new FirestoreItemsManager();
                        Debug.Log("Firebase Firestore is ready to use.");
                    }
                    else
                    {
                        Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to check and fix Firebase dependencies.");
            }
        });
    }
    public IEnumerator CheckIfUserExists()
    {
        yield return new WaitUntil(() => db != null);
        Query userQuery = db.Collection("Users").WhereEqualTo("userID", FirebaseAuthManager.instance.GetCurrentUser().UserId);

        userQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Count > 0)
                {
                    Debug.Log("User exists in database.");
                    //veriler burda yuklenecek...

                }
                else
                {
                    AddNewPlayerToDataBase();
                }
            }
            else
            {
                Debug.LogError("Error getting document: " + task.Exception);
            }
        });
    }
    
    private void AddNewPlayerToDataBase()
    {
        Firebase.Auth.FirebaseUser user = FirebaseAuthManager.instance.GetCurrentUser();
        var userRegistration = new Dictionary<string, object>();

        string id = user.UserId;
        string email = user.Email;
        string telNo = user.PhoneNumber;
        string name = user.DisplayName;
        var signInDate = FieldValue.ServerTimestamp;

        userRegistration.Add("userID", id);
        userRegistration.Add("userEmail", email);
        userRegistration.Add("userTelNo", telNo);
        userRegistration.Add("userName", name);
        userRegistration.Add("userSignInDate", signInDate);
        db.Collection("Users").AddAsync(userRegistration).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User added successfully.");
            }
            else
            {
                Debug.LogError("Failed to add user: " + task.Exception);
            }
        });
    }
    public IEnumerator CheckIfUserExists(string _overrideUserID, string _overrideUserEmail, string _overrideUserTelNo, string _overrideUserName)
    {
        yield return new WaitUntil(() => db != null);
        Query userQuery = db.Collection("Users").WhereEqualTo("userID", _overrideUserID);

        userQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Count > 0)
                {
                    Debug.Log("User exists in database.");
                }
                else
                {
                    AddNewPlayerToDataBase(_overrideUserID, _overrideUserEmail, _overrideUserTelNo, _overrideUserName);
                }
            }
            else
            {
                Debug.LogError("Error getting document: " + task.Exception);
            }
        });
    }
    private void AddNewPlayerToDataBase(string _overrideUserID, string _overrideUserEmail, string _overrideUserTelNo, string _overrideUserName)
    {
        var userRegistration = new Dictionary<string, object>();
        var signInDate = FieldValue.ServerTimestamp;

        userRegistration.Add("userID", _overrideUserID);
        userRegistration.Add("userEmail", _overrideUserEmail);
        userRegistration.Add("userTelNo", _overrideUserTelNo);
        userRegistration.Add("userName", _overrideUserName);
        userRegistration.Add("userSignInDate", signInDate);

        db.Collection("Users").AddAsync(userRegistration).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User added successfully.");
            }
            else
            {
                Debug.LogError("Failed to add user: " + task.Exception);
            }
        });
    }
}
public sealed class FirestoreItemsManager
{
    FirebaseFirestore db;
    public FirestoreItemsManager()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    public void AddRoomWithUserId(string userId, RoomData _roomData)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Rooms").WhereEqualTo("userID", userId);

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

                    // Belge varsa, alt koleksiyon olan PictureDatas'a yeni belge ekle
                    CheckAndAddRoomData(documentReference, _roomData);
                }
                else
                {
                    // Belge yoksa yeni belge oluþtur ve alt koleksiyon ekle

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId }
                    };

                    db.Collection("Rooms").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddRoomData(documentReference, _roomData);
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
    private void CheckAndAddRoomData(DocumentReference documentReference, RoomData _roomData)
    {
        // Alt koleksiyon olan PictureDatas'ý sorgula
        CollectionReference roomDatasRef = documentReference.Collection("RoomDatas");
        Query query = roomDatasRef.WhereEqualTo("ID", _roomData.ID);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                Debug.Log(_roomData.name + " adli check ediliyor...(for Firebase)");
                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Picture with ID {_roomData.ID} already exists for user.");
                }
                else
                {
                    /*
                    public string availableRoomCell;
                    public bool isLock = true;
                    public bool isActive = false;
                    public float RequiredMoney;
                    public bool IsHasStatue = false;
                    public EditObjData MyStatue;
                    public List<int> MyRoomWorkersIDs = new List<int>();
                    */
                    // Belge yoksa yeni belge ekle
                    Debug.Log(_roomData.name + " adli oda verileri databaseye aktariliyor...");
                    Dictionary<string, object> roomData = new Dictionary<string, object>
                    {
                        { "RoomCell", _roomData.availableRoomCell.CellLetter.ToString() + _roomData.availableRoomCell.CellNumber },
                        { "IsActive", _roomData.isActive },
                        { "IsLock", _roomData.isLock },
                        { "ID", _roomData.ID },
                        { "IsHasStatue", _roomData.isHasStatue },
                        { "StatueID", _roomData.GetMyStatueInTheMyRoom().ID },
                        { "RequiredMoney", _roomData.RequiredMoney },
                        { "RoomWorkersIDs", _roomData.MyRoomWorkersIDs },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                    roomDatasRef.AddAsync(roomData).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            Debug.Log($"Picture data with ID {_roomData.ID} successfully added.");
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add picture data: {addTask.Exception}");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError($"Error querying picture data: {task.Exception}");
            }
        });
    }
    public void AddPictureIdWithUserId(string userId, PictureData pictureElement)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Pictures").WhereEqualTo("userID", userId);

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

                    // Belge varsa, alt koleksiyon olan PictureDatas'a yeni belge ekle
                    CheckAndAddPictureData(documentReference, pictureElement);
                }
                else
                {
                    // Belge yoksa yeni belge oluþtur ve alt koleksiyon ekle

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId }
                    };

                    db.Collection("Pictures").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddPictureData(documentReference, pictureElement);
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

    private void CheckAndAddPictureData(DocumentReference documentReference, PictureData pictureElement)
    {
        // Alt koleksiyon olan PictureDatas'ý sorgula
        CollectionReference pictureDatasRef = documentReference.Collection("PictureDatas");
        Query query = pictureDatasRef.WhereEqualTo("ID", pictureElement.painterData.ID);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Picture with ID {pictureElement.painterData.ID} already exists for user.");
                }
                else
                {
                    // Belge yoksa yeni belge ekle
                    Dictionary<string, object> pictureData = new Dictionary<string, object>
                    {
                        { "IsActive", pictureElement.isActive },
                        { "IsFirst", pictureElement.isFirst },
                        { "IsLocked", pictureElement.isLocked },
                        { "RoomID", pictureElement.RoomID },
                        { "TextureID", pictureElement.TextureID },
                        { "ID", pictureElement.painterData.ID },
                        { "TabloID", pictureElement.id },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                    pictureDatasRef.AddAsync(pictureData).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            Debug.Log($"Picture data with ID {pictureElement.painterData.ID} successfully added.");
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add picture data: {addTask.Exception}");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError($"Error querying picture data: {task.Exception}");
            }
        });
    }
    public void UpdatePictureData(string userId, int pictureDataId, PictureData updatedData)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Pictures").WhereEqualTo("userID", userId);

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
                    CollectionReference pictureDatasRef = documentReference.Collection("PictureDatas");
                    Query pictureDataQuery = pictureDatasRef.WhereEqualTo("ID", pictureDataId);

                    pictureDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(pictureDataTask =>
                    {
                        if (pictureDataTask.IsCompleted)
                        {
                            QuerySnapshot pictureDataSnapshot = pictureDataTask.Result;

                            if (pictureDataSnapshot.Documents.Count() > 0)
                            {
                                DocumentSnapshot pictureDataDocument = pictureDataSnapshot.Documents.FirstOrDefault();
                                DocumentReference pictureDataRef = pictureDataDocument.Reference;

                                Dictionary<string, object> updates = new Dictionary<string, object>
                            {
                                { "IsActive", updatedData.isActive },
                                { "IsFirst", updatedData.isFirst },
                                { "IsLocked", updatedData.isLocked },
                                { "TabloID", updatedData.id },
                                //{ "RoomID", updatedData.RoomID },
                                //{ "TextureID", updatedData.TextureID },
                                { "Timestamp", FieldValue.ServerTimestamp }
                            };

                                pictureDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                                {
                                    if (updateTask.IsCompleted)
                                    {
                                        Debug.Log($"Picture data successfully updated for user {userId}");
                                    }
                                    else if (updateTask.IsFaulted)
                                    {
                                        Debug.LogError($"Failed to update picture data: {updateTask.Exception}");
                                    }
                                });
                            }
                            else
                            {
                                Debug.LogError($"No picture data found for ID: {pictureDataId}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Error querying picture data: {pictureDataTask.Exception}");
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
    public async Task<List<PictureData>> GetAllPictureInDatabase(string _userId)
    {
        List<PictureData> foundPictures = new List<PictureData>();
        Query query = db.Collection("Pictures").WhereEqualTo("userID", _userId);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            if (documentSnapshot.Exists)
            {
                CollectionReference pictureIdsRef = documentSnapshot.Reference.Collection("PictureDatas");
                QuerySnapshot pictureQuerySnapshot = await pictureIdsRef.GetSnapshotAsync();
                foreach (DocumentSnapshot pictureRef in pictureQuerySnapshot.Documents)
                {
                    if (pictureRef.Exists)
                    {
                        var pictureData = pictureRef.ToDictionary();
                        PainterData pd = new PainterData();
                        int painterId = pictureData.ContainsKey("ID") ? Convert.ToInt32(pictureData["ID"]) : 0;
                        pd = ItemManager.instance.GetPainterDataWithID(painterId);
                        PictureData foundPicture = new PictureData
                        {
                            painterData = pd,
                            id = pictureData.ContainsKey("TabloID") ? Convert.ToInt32(pictureData["TabloID"]) : 0,
                            isActive = pictureData.ContainsKey("IsActive") && Convert.ToBoolean(pictureData["IsActive"]),
                            isFirst = pictureData.ContainsKey("IsFirst") && Convert.ToBoolean(pictureData["IsFirst"]),
                            isLocked = pictureData.ContainsKey("IsLocked") && Convert.ToBoolean(pictureData["IsLocked"]),
                            RoomID = pictureData.ContainsKey("RoomID") ? Convert.ToInt32(pictureData["RoomID"]) : 0,
                            TextureID = pictureData.ContainsKey("TextureID") ? Convert.ToInt32(pictureData["TextureID"]) : 0
                        };
                        foundPictures.Add(foundPicture);
                    }
                }
            }
        }
        if (foundPictures.Count <= 0)
        {
            Debug.Log(_userId + " ID'li kullanicinin tablolari veritabaninda bulunmamaktadir.");
            return new List<PictureData>();
        }
        else
        {
            Debug.Log(_userId + " ID'li kullanicinin veritabaninda " + foundPictures.Count + " adedince tablo bulunmaktadir.");
            return foundPictures;
        }
    }
    public async Task<PictureData> GetPictureInDatabase(string userId, int pictureId)
    {
        PictureData foundPicture = null;

        // Pictures koleksiyonunda belirli bir kullanýcýya ait belgeleri sorgula
        Query query = db.Collection("Pictures").WhereEqualTo("userID", userId);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            if (documentSnapshot.Exists)
            {
                // Belgenin PictureIDs alt koleksiyonuna eriþ
                CollectionReference pictureIdsRef = documentSnapshot.Reference.Collection("PictureDatas");
                Query pictureQuery = pictureIdsRef.WhereEqualTo("TabloID", pictureId);
                QuerySnapshot pictureQuerySnapshot = await pictureQuery.GetSnapshotAsync();

                foreach (DocumentSnapshot pictureDocumentSnapshot in pictureQuerySnapshot.Documents)
                {
                    if (pictureDocumentSnapshot.Exists)
                    {
                        // Eþleþen tabloyu bulun
                        var pictureData = pictureDocumentSnapshot.ToDictionary();
                        PainterData pd = new PainterData();
                        int painterId = pictureData.ContainsKey("ID") ? Convert.ToInt32(pictureData["ID"]) : 0;
                        pd = ItemManager.instance.GetPainterDataWithID(painterId);
                        foundPicture = new PictureData
                        {                            
                            painterData = pd,
                            id = pictureData.ContainsKey("TabloID") ? Convert.ToInt32(pictureData["TabloID"]) : 0,
                            isActive = pictureData.ContainsKey("IsActive") && Convert.ToBoolean(pictureData["IsActive"]),
                            isFirst = pictureData.ContainsKey("IsFirst") && Convert.ToBoolean(pictureData["IsFirst"]),
                            isLocked = pictureData.ContainsKey("IsLocked") && Convert.ToBoolean(pictureData["IsLocked"]),
                            RoomID = pictureData.ContainsKey("RoomID") ? Convert.ToInt32(pictureData["RoomID"]) : 0,
                            TextureID = pictureData.ContainsKey("TextureID") ? Convert.ToInt32(pictureData["TextureID"]) : 0
                        };
                        break;
                    }
                }

                if (foundPicture != null)
                {
                    break;
                }
            }
        }

        return foundPicture;
    }
}
