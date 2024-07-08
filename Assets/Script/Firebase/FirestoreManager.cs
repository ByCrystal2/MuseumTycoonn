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
    public FirestoreRoomDatasHandler roomDatasHandler;
    public FirestorePictureDatasHandler pictureDatasHandler;
    public FirestoreStatueDatasHandler statueDatasHandler;
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
                AddNewPlayerGameDataToDatabase(task.Result);
            }
            else
            {
                Debug.LogError("Failed to add user: " + task.Exception);
            }
        });
    }
    public void UpdateGameData(string userId)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        Query query = db.Collection("Users").WhereEqualTo("userID", userId);

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
                    CollectionReference pictureDatasRef = documentReference.Collection("GameDatas");

                    pictureDatasRef.GetSnapshotAsync().ContinueWithOnMainThread(pictureDataTask =>
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
                                { "IsWatchTutorial", TutorialLevelManager.instance.IsWatchTutorial },
                                { "IsFirstGame", NpcManager.instance.IsFirstGame },
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
    public async void AddNewPlayerGameDataToDatabase(DocumentReference _documentReference)
    {
        CollectionReference collectionReference = _documentReference.Collection("GameDatas");
        QuerySnapshot documentSnapshot = await collectionReference.GetSnapshotAsync();

        if (documentSnapshot.Count > 0)
        {
            Debug.Log($"GameDatas already exists for user.");
        }
        else
        {
            var userGameData = new Dictionary<string, object>();

            bool watchTutorial = false;
            bool firstGame = true;

            userGameData.Add("IsWatchTutorial", watchTutorial);
            userGameData.Add("IsFirstGame", firstGame);

            DocumentReference addTask = await collectionReference.AddAsync(userGameData);

            if (addTask != null)
                Debug.Log($"Game data with user ID {FirebaseAuthManager.instance.GetCurrentUser().UserId} successfully added.");
            else
                Debug.LogError("Failed to add game data.");
        }

    }
    public bool GetGameDataValue(bool _isFirstGame)
    {
        return _isFirstGame; //burdan devam edilecek.
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
                DocumentReference documentReference = task.Result;
                AddNewPlayerGameDataToDatabase(documentReference);
            }
            else
            {
                Debug.LogError("Failed to add user: " + task.Exception);
            }
        });
    }
}
