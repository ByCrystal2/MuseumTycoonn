using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase;
using System.Linq;
using System;
using TaskExtensions;

public class FirestoreManager : MonoBehaviour
{
    public FirebaseFirestore db;
    public FirestoreRoomDatasHandler roomDatasHandler;
    public FirestorePictureDatasHandler pictureDatasHandler;
    public FirestoreStatueDatasHandler statueDatasHandler;
    public FirestoreSkillDatasHandler skillDatasHandler;
    public FirestoreWorkerDatasHandler workerDatasHandler;
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
    DocumentReference gameDataRef;
    public async System.Threading.Tasks.Task UpdateGameData(string userId, bool _overwrite = false)
    {
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial && !_overwrite) return;
        // Kullanýcý ID'si ile belgeyi sorgula
        var museumDatas = MuseumManager.instance.GetSaveData();
        Debug.Log("UpdateGameData method is starting...");

        if (gameDataRef == null)
        {
            Query query = db.Collection("Users").WhereEqualTo("userID", userId);
            try
            {

                await query.GetSnapshotAsync().ContinueWithOnMainThread(async task =>
            {
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;

                    if (snapshot.Documents.Count() > 0)
                    {
                        DocumentSnapshot documentSnapshot = snapshot.Documents.FirstOrDefault();
                        DocumentReference documentReference = documentSnapshot.Reference;

                        // Belge varsa, alt koleksiyon olan PictureDatas'ta tabloyu bul ve güncelle
                        CollectionReference gameDatasRef = documentReference.Collection("GameDatas");

                        await gameDatasRef.GetSnapshotAsync().ContinueWithOnMainThread(async gameDataTask =>
                        {
                            if (gameDataTask.IsCompleted)
                            {
                                QuerySnapshot gameDataSnapshot = gameDataTask.Result;

                                if (gameDataSnapshot.Documents.Count() > 0)
                                {
                                    Debug.Log("GameData is exits with " + userId + " userId");
                                    DocumentSnapshot gameDataDocument = gameDataSnapshot.Documents.FirstOrDefault();
                                    gameDataRef = gameDataDocument.Reference;

                                    UpdateGameDatasHelper(museumDatas, gameDataRef, userId);

                                }
                            }
                            else
                            {
                                Debug.LogError($"Error querying game data: {gameDataTask.Exception}");
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
            catch (Exception _ex)
            {
                Debug.LogError($"UpdateGameData method error cathing. Error: " + _ex.Message);
            } 
        }
        else
        {
            UpdateGameDatasHelper(museumDatas, gameDataRef, userId);
            Debug.Log("GameData is not null!");
        }
    }
    async void UpdateGameDatasHelper((float _gold,float _Culture, float _Gem, float _SkillPoint, int _CurrentCultureLevel) museumDatas, DocumentReference gameDataRef, string userId)
    {
        try
        {
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "IsWatchTutorial",  GameManager.instance.IsWatchTutorial},
                { "IsFirstGame", GameManager.instance.IsFirstGame },
                { "Gold", museumDatas._gold },
                { "Culture", museumDatas._Culture },
                { "Gem", museumDatas._Gem },
                { "SkillPoint", museumDatas._SkillPoint },
                { "CurrentCultureLevel", museumDatas._CurrentCultureLevel },
                { "GameLanguage", GameManager.instance.GetGameLanguage() },
                { "ActiveRoomsRequiredMoney", GameManager.instance.ActiveRoomsRequiredMoney },
                { "BaseWorkerHiringPrice", GameManager.instance.BaseWorkerHiringPrice },
                { "PurchasedItemIDs", MuseumManager.instance.PurchasedItems.Select(x=> x.ID).ToList() },
                { "DailyRewardItemIDs", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.ID).ToList() },
                { "DailyRewardItemsPurchased", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.IsPurchased).ToList() },
                { "DailyRewardItemsLocked", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.IsLocked).ToList() },
                { "WorkersInInventoryIDs", MuseumManager.instance.WorkersInInventory.Select(x=> x.ID).ToList()},
                { "LastDailyRewardTime", MuseumManager.instance.lastDailyRewardTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "WhatDay", TimeManager.instance.WhatDay },
                { "RemoveAllAds", GoogleAdsManager.instance.adsData.RemovedAllAds },

            //Achievements
            { "PurchasedRoomCount", GPGamesManager.instance.achievementController.PurchasedRoomCount },
            { "NumberOfTablesPlaced", GPGamesManager.instance.achievementController.NumberOfTablesPlaced },
            { "NumberOfVisitors", GPGamesManager.instance.achievementController.NumberOfVisitors },
            { "NumberOfStatuesPlaced", GPGamesManager.instance.achievementController.NumberOfStatuesPlaced },
            { "TotalNumberOfMuseumVisitors", GPGamesManager.instance.achievementController.TotalNumberOfMuseumVisitors },
            { "TotalWorkerHiringCount", GPGamesManager.instance.achievementController.TotalWorkerHiringCount },
            { "TotalWorkerAssignCount", GPGamesManager.instance.achievementController.TotalWorkerAssignCount },
                //Achievements
                { "Timestamp", FieldValue.ServerTimestamp }

            };

            await gameDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsCompleted)
                {
                    Debug.Log($"Game data successfully updated for user {userId}");
                }
                else if (updateTask.IsFaulted)
                {
                    Debug.LogError($"Failed to update game data: {updateTask.Exception}");
                }
            });
        }
        catch (Exception _ex)
        {
            Debug.LogError($"Error updating game data: {_ex}");
        }
    }
    string updatedLanguage = "";
    public async System.Threading.Tasks.Task UpdateGameLanguageInGameDatas(string userId)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        if (updatedLanguage == GameManager.instance.GetGameLanguage()) return;

        Query query = db.Collection("Users").WhereEqualTo("userID", userId);
        Debug.Log("UpdateGameLanguageInGameDatas method is starting...");

        try
        {
            QuerySnapshot snapshot = await query.GetSnapshotAsync().WithCancellation(GameManager.instance.GetFirebaseToken().Token);

            if (snapshot.Documents.Count() > 0)
            {
                DocumentSnapshot documentSnapshot = snapshot.Documents.FirstOrDefault();
                DocumentReference documentReference = documentSnapshot.Reference;

                // Belge varsa, alt koleksiyon olan GameDatas'ta tabloyu bul ve güncelle
                CollectionReference gameDatasRef = documentReference.Collection("GameDatas");

                QuerySnapshot gameDataSnapshot = await gameDatasRef.GetSnapshotAsync().WithCancellation(GameManager.instance.GetFirebaseToken().Token);

                if (gameDataSnapshot.Documents.Count() > 0)
                {
                    Debug.Log("GameData exists with userId " + userId);
                    DocumentSnapshot gameDataDocument = gameDataSnapshot.Documents.FirstOrDefault();
                    DocumentReference gameDataRef = gameDataDocument.Reference;

                    var museumDatas = MuseumManager.instance.GetSaveData();

                    try
                    {
                        Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { "GameLanguage", GameManager.instance.GetGameLanguage() },
                        { "Timestamp", FieldValue.ServerTimestamp }
                        };

                        await gameDataRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted && !task.IsFaulted)
                            {
                                Debug.Log($"Game data successfully updated for user {userId}");
                                updatedLanguage = GameManager.instance.GetGameLanguage();
                            }
                        });                        
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error updating game data: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("No game data documents found.");
                }
            }
            else
            {
                Debug.LogWarning($"No document found for user ID: {userId}");
            }
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("UpdateGameLanguageInGameDatasAsync operation was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error querying documents: {ex.Message}");
        }
    }
    QuerySnapshot _gameDataQuerySnapshot;
    public async System.Threading.Tasks.Task<Dictionary<string, object>> GetGameDataInDatabase(string userId)
    {
        var gameDataDictonary = new Dictionary<string, object>();
        Debug.Log("_gameDataQuerySnapshot GetGameDataInDatabase method is starting.");
        try
        {
            // Kullanýcýya ait belgeleri sorgula
            if (_gameDataQuerySnapshot == null)
            {
                Debug.Log("_gameDataQuerySnapshot is null");
                Query query = db.Collection("Users").WhereEqualTo("userID", userId);
                QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
                {
                    if (documentSnapshot.Exists)
                    {
                        Debug.Log("_gameDataQuerySnapshot documentSnapshot.Exists true.");
                        CollectionReference gameDataRef = documentSnapshot.Reference.Collection("GameDatas");
                        QuerySnapshot gameDataQuerySnapshot = await gameDataRef.GetSnapshotAsync();

                        foreach (DocumentSnapshot gameDataDocumentSnapshot in gameDataQuerySnapshot.Documents)
                        {
                            if (gameDataDocumentSnapshot.Exists)
                            {
                                gameDataDictonary = gameDataDocumentSnapshot.ToDictionary();
                            }
                        }
                        _gameDataQuerySnapshot = gameDataQuerySnapshot;
                    }
                } 
            }
            else
            {
                Debug.Log("_gameDataQuerySnapshot is not null");
                foreach (DocumentSnapshot gameDataDocumentSnapshot in _gameDataQuerySnapshot.Documents)
                    if (gameDataDocumentSnapshot.Exists)
                    {
                        gameDataDictonary = gameDataDocumentSnapshot.ToDictionary();
                        Debug.Log("_gameDataQuerySnapshot documentSnapshot.Exists true.");
                    }

            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting statue data: {ex.Message}");
        }
        return gameDataDictonary;
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

            float gold = 0f, culture = 0f, gem = 0f, skillPoint = 0f;
            int currentCultureLevel = 0;
            userGameData.Add("IsWatchTutorial", watchTutorial);
            userGameData.Add("IsFirstGame", firstGame);
            userGameData.Add("Gold", gold);
            userGameData.Add("Culture", culture);
            userGameData.Add("Gem", gem);
            userGameData.Add("SkillPoint", skillPoint);
            userGameData.Add("CurrentCultureLevel", currentCultureLevel);
            userGameData.Add("GameLanguage", "English");
            userGameData.Add("ActiveRoomsRequiredMoney", GameManager.instance.ActiveRoomsRequiredMoney);
            userGameData.Add("BaseWorkerHiringPrice", GameManager.instance.BaseWorkerHiringPrice);
            userGameData.Add("PurchasedItemIDs", MuseumManager.instance.PurchasedItems.Select(x=> x.ID).ToList());
            userGameData.Add("DailyRewardItemIDs", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.ID).ToList());
            userGameData.Add("DailyRewardItemsPurchased", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.IsPurchased).ToList());
            userGameData.Add("DailyRewardItemsLocked", ItemManager.instance.CurrentDailyRewardItems.Select(x=> x.IsLocked).ToList());
            userGameData.Add("WorkersInInventoryIDs", MuseumManager.instance.WorkersInInventory.Select(x=> x.ID).ToList());
            userGameData.Add("LastDailyRewardTime", MuseumManager.instance.lastDailyRewardTime.ToString("yyyy-MM-dd HH:mm:ss"));
            userGameData.Add("WhatDay", TimeManager.instance.WhatDay);
            userGameData.Add("RemoveAllAds", GoogleAdsManager.instance.adsData.RemovedAllAds);
            //Achievements
            userGameData.Add("PurchasedRoomCount", GPGamesManager.instance.achievementController.PurchasedRoomCount);
            userGameData.Add("NumberOfTablesPlaced", GPGamesManager.instance.achievementController.NumberOfTablesPlaced );
            userGameData.Add("NumberOfVisitors", GPGamesManager.instance.achievementController.NumberOfVisitors);
            userGameData.Add("NumberOfStatuesPlaced", GPGamesManager.instance.achievementController.NumberOfStatuesPlaced);
            userGameData.Add("TotalNumberOfMuseumVisitors", GPGamesManager.instance.achievementController.TotalNumberOfMuseumVisitors);
            userGameData.Add("TotalWorkerHiringCount", GPGamesManager.instance.achievementController.TotalWorkerHiringCount);
            userGameData.Add("TotalWorkerAssignCount", GPGamesManager.instance.achievementController.TotalWorkerAssignCount);
            //Achievements
            userGameData.Add( "Timestamp", FieldValue.ServerTimestamp);

            DocumentReference addTask = await collectionReference.AddAsync(userGameData);

            if (addTask != null)
                Debug.Log($"Game data with user ID {FirebaseAuthManager.instance.GetCurrentUser()?.UserId} successfully added.");
            else
                Debug.LogError("Failed to add game data.");
        }

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
