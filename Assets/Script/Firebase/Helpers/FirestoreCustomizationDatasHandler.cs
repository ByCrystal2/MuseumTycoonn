using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchScript.Examples.Colors;
using UnityEngine;

public class FirestoreCustomizationDatasHandler : MonoBehaviour
{
    FirebaseFirestore db;
    List<Color> DefaultColors;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        DefaultColors = new();
        DefaultColors.Add(new Color(0.2431373f, 0.4196079f, 0.6196079f, 1));
        DefaultColors.Add(new Color(0.8196079f, 0.6431373f, 0.2980392f, 1));
        DefaultColors.Add(new Color(0.282353f, 0.2078432f, 0.1647059f, 1));
        DefaultColors.Add(new Color(0.5960785f, 0.6117647f, 0.627451f, 1));
        DefaultColors.Add(new Color(0.372549f, 0.3294118f, 0.2784314f, 1));
        DefaultColors.Add(new Color(0.1764706f, 0.1960784f, 0.2156863f, 1));
        DefaultColors.Add(new Color(0.345098f, 0.3764706f, 0.3960785f, 1));
        DefaultColors.Add(new Color(0.2627451f, 0.2117647f, 0.1333333f, 1));
        DefaultColors.Add(new Color(1f, 0.8000001f, 0.682353f, 1));
        DefaultColors.Add(new Color(0.8039216f, 0.7019608f, 0.6313726f, 1));
        DefaultColors.Add(new Color(0.9294118f, 0.6862745f, 0.5921569f, 1));
        DefaultColors.Add(new Color(0.2283196f, 0.5822246f, 0.7573529f, 1));
        DefaultColors.Add(new Color(0.2283196f, 0.5822246f, 0.7573529f, 1));
    }

    public async Task AddCustomizationDataWithUserId(string userId, CharacterCustomizeData _customizeData, bool _multiSlotSave = false, int _overrideSlotId = -1)
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
                        { "LastSelectedColorHeader", _customizeData.LastSelectedColorHeader },
                        { "LastSelectedCustomizeSlot", _customizeData.playerCustomizeData.selectedCustomizeSlot }
                    };

                    documentReference.UpdateAsync(updatedData).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            if (_multiSlotSave)
                                CheckAndAddCustomizeData(documentReference, _customizeData, true);
                            else
                                CheckAndAddCustomizeData(documentReference, _customizeData, _overrideSlotId);
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
                        { "LastSelectedCustomizeSlot", _customizeData.playerCustomizeData.selectedCustomizeSlot }
                    };

                    db.Collection("Customizations").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            if (_multiSlotSave)
                                CheckAndAddCustomizeData(documentReference, _customizeData, true);
                            else
                                CheckAndAddCustomizeData(documentReference, _customizeData, _overrideSlotId);
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
    public async Task UpdateBaseValues(CharacterCustomizeData _customizeData, string userId)
    {
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
                        { "LastSelectedColorHeader", _customizeData.LastSelectedColorHeader },
                        { "LastSelectedCustomizeSlot", _customizeData.playerCustomizeData.selectedCustomizeSlot }
                    };
                     documentReference.UpdateAsync(updatedData).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Update customization base values with userId: " + userId);
                            
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
                        { "LastSelectedCustomizeSlot", _customizeData.playerCustomizeData.selectedCustomizeSlot }
                    };

                    db.Collection("Customizations").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            Debug.Log("Add customization base values with userId: "+userId);
                        }
                        else if (addTask.IsFaulted)
                        {
                            Debug.LogError($"Failed to add new document: {addTask.Exception}");
                        }
                    });
                }
            }
        });
    }
    public void CheckAndAddCustomizeData(DocumentReference documentReference, CharacterCustomizeData _customizeData, int _overrideSlotId)
    {
        try
        {
            // Alt koleksiyon olan PictureDatas'ý sorgula
            CollectionReference customizeDatasRef = documentReference.Collection("CustomizationDatas");            
            int currentSlotId = _overrideSlotId == -1 ? _customizeData.playerCustomizeData.selectedCustomizeSlot : _overrideSlotId;
            Query query = customizeDatasRef.WhereEqualTo("SlotNumber", currentSlotId);

            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;

                    if (snapshot.Documents.Count() > 0)
                    {
                        Debug.Log($"Customize with number of slot {currentSlotId} already exists for user.");

                        UpdateCustomizationData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, _customizeData, currentSlotId);
                    }
                    else
                    {
                        // Belge yoksa yeni belge ekle
                        PlayerExtraCustomizeData selectedCustomizeSlotData = _customizeData.playerCustomizeData.AllCustomizeData[currentSlotId];

                        List<Dictionary<string, float>> colorDataList = new List<Dictionary<string, float>>();
                        List<Color> colors = selectedCustomizeSlotData.Colors;
                        foreach (Color color in colors)
                        {
                            // Her bir rengi RGB deðerleriyle bir dictionary olarak kaydediyoruz
                            var colorData = new Dictionary<string, float>
                        {
                            { "red", color.r },
                            { "green", color.g },
                            { "blue", color.b }
                        };

                            colorDataList.Add(colorData);
                        }
                        Dictionary<string, object> newCustomizeData = new Dictionary<string, object>
                    {
                        { "SlotNumber", currentSlotId },
                        { "IsFemale", selectedCustomizeSlotData.isFemale },
                        { "Colors", colorDataList },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                        customizeDatasRef.AddAsync(newCustomizeData).ContinueWithOnMainThread(addTask =>
                        {
                            if (addTask.IsCompleted)
                            {
                                DocumentReference document = addTask.Result;
                                CollectionReference elementDatasRef = document.Collection("CustomizeElements");

                                foreach (CustomizeElement element in selectedCustomizeSlotData.CustomizeElements)
                                {
                                    Query query = customizeDatasRef.WhereEqualTo("elementID", element.elementID);
                                    query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                                    {
                                        if (task.IsCompleted)
                                        {
                                            QuerySnapshot snapshot = task.Result;

                                            if (snapshot.Documents.Count() <= 0)
                                            {
                                                Dictionary<string, object> newElementData = new Dictionary<string, object>
                                            {
                                                { "CustomizeSlot", element.customizeSlot },
                                                { "elementID", element.elementID },
                                            };
                                                elementDatasRef.AddAsync(newElementData).ContinueWithOnMainThread(addTask =>
                                                {
                                                    if (addTask.IsCompleted)
                                                    {
                                                        DocumentReference document = addTask.Result;
                                                    }
                                                });
                                            }
                                            string ids = "";
                                            string slots = "";
                                            foreach (var item in selectedCustomizeSlotData.CustomizeElements)
                                            {
                                                ids += item.elementID + "-";
                                                slots += item.customizeSlot.ToString() + "-";
                                            }
                                            Debug.Log("CustomizeElements added database ids => " + ids);
                                            Debug.Log("CustomizeElements added database slots => " + slots);
                                        }
                                    });
                                }
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
        catch (Exception _ex)
        {
            Debug.LogError("CheckAndAddCustomizeData method has a an error: " + _ex.Message);
        }        
    }
    public void CheckAndAddCustomizeData(DocumentReference documentReference, CharacterCustomizeData _customizeData, bool _multiSlotSave)
    {
        try
        {
            // Alt koleksiyon olan PictureDatas'ý sorgula
            CollectionReference customizeDatasRef = documentReference.Collection("CustomizationDatas");
            int length = 1;
            if (_multiSlotSave) length = _customizeData.playerCustomizeData.AllCustomizeData.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                Query query = customizeDatasRef.WhereEqualTo("SlotNumber", index);
                query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        QuerySnapshot snapshot = task.Result;

                        if (snapshot.Documents.Count() > 0)
                        {
                            Debug.Log($"Customize with number of slot {index} already exists for user.");

                            UpdateCustomizationData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, _customizeData, index);
                        }
                        else
                        {
                            Debug.Log("in CheckAndAddCustomizeData and multiSlotSave true snapshot.Documents.Count:" + snapshot.Documents.Count());
                            // Belge yoksa yeni belge ekle
                            Debug.Log("in CheckAndAddCustomizeData and multiSlotSave true i:" + index);
                            PlayerExtraCustomizeData selectedCustomizeSlotData = _customizeData.playerCustomizeData.AllCustomizeData[index];
                            Debug.Log("in CheckAndAddCustomizeData and multiSlotSave true snapshot.Documents.Count:" + selectedCustomizeSlotData.ID);

                            List<Dictionary<string, float>> colorDataList = new List<Dictionary<string, float>>();
                            List<Color> colors = selectedCustomizeSlotData.Colors;
                            foreach (Color color in colors)
                            {
                                // Her bir rengi RGB deðerleriyle bir dictionary olarak kaydediyoruz
                                var colorData = new Dictionary<string, float>
                            {
                            { "red", color.r },
                            { "green", color.g },
                            { "blue", color.b }
                            };

                                colorDataList.Add(colorData);
                            }
                            Dictionary<string, object> newCustomizeData = new Dictionary<string, object>
                        {
                        { "SlotNumber", index },
                        { "IsFemale", selectedCustomizeSlotData.isFemale },
                        { "Colors", colorDataList },
                        { "Timestamp", FieldValue.ServerTimestamp }
                        };

                            customizeDatasRef.AddAsync(newCustomizeData).ContinueWithOnMainThread(addTask =>
                            {
                                if (addTask.IsCompleted)
                                {
                                    DocumentReference document = addTask.Result;
                                    CollectionReference elementDatasRef = document.Collection("CustomizeElements");

                                    foreach (CustomizeElement element in selectedCustomizeSlotData.CustomizeElements)
                                    {
                                        Query query = customizeDatasRef.WhereEqualTo("elementID", element.elementID);
                                        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                                        {
                                            if (task.IsCompleted)
                                            {
                                                QuerySnapshot snapshot = task.Result;

                                                if (snapshot.Documents.Count() <= 0)
                                                {
                                                    Dictionary<string, object> newElementData = new Dictionary<string, object>
                                                {
                                                { "CustomizeSlot", element.customizeSlot },
                                                { "elementID", element.elementID },
                                                };
                                                    elementDatasRef.AddAsync(newElementData).ContinueWithOnMainThread(addTask =>
                                                    {
                                                        if (addTask.IsCompleted)
                                                        {
                                                            DocumentReference document = addTask.Result;
                                                        }
                                                    });
                                                }
                                                string ids = "";
                                                string slots = "";
                                                foreach (var item in selectedCustomizeSlotData.CustomizeElements)
                                                {
                                                    ids += item.elementID + "-";
                                                    slots += item.customizeSlot.ToString() + "-";
                                                }
                                                Debug.Log("CustomizeElements added database ids => " + ids);
                                                Debug.Log("CustomizeElements added database slots => " + slots);
                                            }
                                        });
                                    }
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
        }
        catch (Exception _ex)
        {
            Debug.LogError("CheckAndAddCustomizeData method has a an error: " + _ex.Message);
        }
    }
    public async Task<CharacterCustomizeData> GetCustomizationDataInDatabase(string userId)
    {
        CharacterCustomizeData foundCustomizeData = new CharacterCustomizeData();
        List<PlayerExtraCustomizeData> foundCustoms = new List<PlayerExtraCustomizeData>();

        try
        {
            //Kullaniciya ait belgeleri sorgula
            Query query = db.Collection("Customizations").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    CollectionReference customizationDatasCollection = documentSnapshot.Reference.Collection("CustomizationDatas");
                    var mainCustomizationData = documentSnapshot.ToDictionary();
                    int lastSelectedColorHeader = mainCustomizationData.ContainsKey("LastSelectedColorHeader") ? Convert.ToInt32(mainCustomizationData["LastSelectedColorHeader"]) : 0;
                    int lastSelectedCustomizeCategory = mainCustomizationData.ContainsKey("LastSelectedCustomizeCategory") ? Convert.ToInt32(mainCustomizationData["LastSelectedCustomizeCategory"]) : 0;
                    int lastSelectedCustomizeHeader = mainCustomizationData.ContainsKey("LastSelectedCustomizeHeader") ? Convert.ToInt32(mainCustomizationData["LastSelectedCustomizeHeader"]) : 1;
                    int lastSelectedCustomizeSlot = mainCustomizationData.ContainsKey("LastSelectedCustomizeSlot") ? Convert.ToInt32(mainCustomizationData["LastSelectedCustomizeSlot"]) : 0;
                List<int> unlockedCustomizeElementIDs = new List<int>();
                if (mainCustomizationData != null && mainCustomizationData.ContainsKey("UnlockedCustomizeElementIDs") && mainCustomizationData["UnlockedCustomizeElementIDs"] is List<object> list)
                {
                    unlockedCustomizeElementIDs = list.Select(x => Convert.ToInt32(x)).ToList();
                }

                    foundCustomizeData.SetDatas(new PlayerCustomizeData(), unlockedCustomizeElementIDs, lastSelectedCustomizeCategory, lastSelectedCustomizeHeader, lastSelectedColorHeader);
                    Debug.Log("foundCustomizeData.unlockedCustomizeElementIDs.Count => " + foundCustomizeData.unlockedCustomizeElementIDs.Count);
                    if (foundCustomizeData.playerCustomizeData.AllCustomizeData == null)
                        foundCustomizeData.playerCustomizeData.AllCustomizeData = new();
                    foundCustomizeData.playerCustomizeData.selectedCustomizeSlot = lastSelectedCustomizeSlot;
                    for (int i = 0; i < 3; i++)
                    {
                        int index = i;
                        Query customDataQuery = customizationDatasCollection.WhereEqualTo("SlotNumber", index);
                        QuerySnapshot customQuerySnapshot = await customDataQuery.GetSnapshotAsync();

                        PlayerExtraCustomizeData playerExtra = null;
                        foreach (DocumentSnapshot customDocumentSnapshot in customQuerySnapshot.Documents)
                        {
                            if (customDocumentSnapshot.Exists)
                            {
                                List<object> colorDataList = customDocumentSnapshot.GetValue<List<object>>("Colors");

                                List<Color> colors = new List<Color>();
                                foreach (var colorDataObj in colorDataList)
                                {
                                    // Her bir renk dictionary'sini alýyoruz
                                    Dictionary<string, object> colorData = colorDataObj as Dictionary<string, object>;

                                    if (colorData != null)
                                    {
                                        // Dictionary içinden RGB deðerlerini alýyoruz
                                        float red = Convert.ToSingle(colorData["red"]);
                                        float green = Convert.ToSingle(colorData["green"]);
                                        float blue = Convert.ToSingle(colorData["blue"]);

                                        // Yeni Color objesi oluþturuyoruz
                                        Color color = new Color(red, green, blue);
                                        colors.Add(color);
                                    }
                                }

                                playerExtra = new();
                                var customData = customDocumentSnapshot.ToDictionary();
                                playerExtra.ID = customData.ContainsKey("ID") ? Convert.ToInt32(customData["ID"]) : 0;
                                playerExtra.isFemale = customData.ContainsKey("IsFemale") && Convert.ToBoolean(customData["IsFemale"]);
                                playerExtra.Colors = new List<Color>(colors);
                                CollectionReference elementsCollectionRef = customDocumentSnapshot.Reference.Collection("CustomizeElements");
                                List<CustomizeElement> elementsInDatabase = new List<CustomizeElement>();
                                QuerySnapshot snapshot = await elementsCollectionRef.GetSnapshotAsync();

                                foreach (var document in snapshot.Documents)
                                {
                                    if (document.Exists)
                                    {
                                        var elementData = document.ToDictionary();
                                        CustomizeElement databaseElement = new CustomizeElement();
                                        databaseElement.customizeSlot = elementData.ContainsKey("CustomizeSlot") && elementData["CustomizeSlot"] != null ? (CustomizeSlot)Enum.Parse(typeof(CustomizeSlot), elementData["CustomizeSlot"].ToString()) : CustomizeSlot.None;
                                        databaseElement.elementID = elementData.ContainsKey("elementID") ? Convert.ToInt32(elementData["elementID"]) : 0;
                                        elementsInDatabase.Add(databaseElement);
                                        continue;
                                    }
                                }
                                playerExtra.CustomizeElements = elementsInDatabase;
                                break;                                
                            }
                        }
                        if (playerExtra != null)
                        {
                            foundCustoms.Add(playerExtra);
                        }
                    }
                    
                }
                if (foundCustoms.Count > 0)
                {
                    foundCustomizeData.playerCustomizeData.AllCustomizeData = foundCustoms;

                    Debug.Log("Customization aktarimi sonlandi. Veri tabani customize sayisi => " + foundCustoms.Count);
                    ExtraCustomizationsFilling(foundCustomizeData);
                    break;
                }
                else
                {
                    Debug.Log("Customization aktarimi sonlandi. Karakter ozellestirme veri tabaninda bulunamadi. Default datalar olusturuluyor...");
                    ExtraCustomizationsFilling(foundCustomizeData);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting customize data: {ex.Message}");
        }
        if (foundCustoms.Count <= 0)
        {
            Debug.Log("Customization aktarimi sonlandi. Karakter ozellestirme veri tabaninda bulunamadi. Default datalar olusturuluyor...");
            ExtraCustomizationsFilling(foundCustomizeData);
        }
        return foundCustomizeData;
    }
    public void UpdateCustomizationData(string userId, CharacterCustomizeData _customizeData, int _overrideSlotId)
    {
        if (GameManager.instance != null && !GameManager.instance.IsWatchTutorial) return;

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

                    // Belgeyi bulduysak alt koleksiyon olan CustomizationDatas'ta tabloyu bul ve güncelle
                    UpdateCustomizationDatas(documentReference, _customizeData, _overrideSlotId);
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

    private void UpdateCustomizationDatas(DocumentReference documentReference, CharacterCustomizeData _customizeData, int _overrideSlotId)
    {
        CollectionReference customizationDatasRef = documentReference.Collection("CustomizationDatas");
        

        Query customizeDataQuery = customizationDatasRef.WhereEqualTo("SlotNumber", _overrideSlotId);
        Debug.Log("in UpdateCustomizationDatas method current selectedCustomizeSlotId: " + _customizeData.playerCustomizeData.selectedCustomizeSlot);
        Debug.Log("in UpdateCustomizationDatas method current overrideSlotId: " + _overrideSlotId);
        customizeDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(customizeDataTask =>
        {
            if (customizeDataTask.IsCompleted)
            {
                QuerySnapshot customizeDataSnapshot = customizeDataTask.Result;

                if (customizeDataSnapshot.Documents.Count() > 0)
                {
                    ProcessCustomizeData(customizeDataSnapshot, _customizeData, _overrideSlotId);

                }
                else
                {
                    Debug.LogError($"No customization data found for selected slot.");
                }
            }
            else
            {
                Debug.LogError($"Error querying customization data: {customizeDataTask.Exception}");
            }
        });
    }

    private void ProcessCustomizeData(QuerySnapshot customizeDataSnapshot, CharacterCustomizeData _customizeData, int _overrideSlotId)
    {
        Debug.Log("Method Controlling in ProcessCustomizeData _overrideSlotId" + _overrideSlotId);
        PlayerExtraCustomizeData playerExtraCustomizeDatas = _customizeData.playerCustomizeData.AllCustomizeData[_overrideSlotId];
        try
        {
            foreach (var document in customizeDataSnapshot.Documents)
            {
                DocumentReference customizeDataRef = document.Reference;
                List<Dictionary<string, float>> colorDataList = new List<Dictionary<string, float>>();
                List<Color> colors = playerExtraCustomizeDatas.Colors;
                foreach (Color color in colors)
                {
                    // Her bir rengi RGB deðerleriyle bir dictionary olarak kaydediyoruz
                    var colorData = new Dictionary<string, float>
                {
                    { "red", color.r },
                    { "green", color.g },
                    { "blue", color.b }
                };

                    colorDataList.Add(colorData);
                }
                Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "IsFemale", playerExtraCustomizeDatas.isFemale },
                { "Colors", colorDataList },
                { "Timestamp", FieldValue.ServerTimestamp }
            };

                customizeDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        UpdateCustomizeElements(customizeDataRef, playerExtraCustomizeDatas);
                    }
                    else
                    {
                        Debug.LogError($"Failed to update customization data: {updateTask.Exception}");
                    }
                });

            }
        }
        catch (Exception _ex)
        {
            Debug.LogError("ProcessCustomizeData method has a an error: " + _ex.Message);
        }        
    }

    private void UpdateCustomizeElements(DocumentReference customizeDataRef, PlayerExtraCustomizeData extraData)
    {
        Debug.Log("Method Controlling in UpdateCustomizeElements extraData.ID:" + extraData.ID);
        CollectionReference elementDatasRef = customizeDataRef.Collection("CustomizeElements");
        HashSet<int> processedElementIDs = new HashSet<int>();

        foreach (CustomizeElement element in extraData.CustomizeElements)
        {
            if (processedElementIDs.Contains(element.elementID)) continue;

            Query query = elementDatasRef.WhereEqualTo("CustomizeSlot", element.customizeSlot);

            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;

                    if (snapshot.Documents.Count() > 0)
                    {
                        // Mevcut elemanlarý guncelle
                        DocumentSnapshot elementDocSnap = snapshot.Documents.FirstOrDefault();
                        DocumentReference elementRef = elementDocSnap.Reference;

                        var databaseElementData = elementDocSnap.ToDictionary();
                        int databaseElementId = databaseElementData.ContainsKey("elementID") ? Convert.ToInt32(databaseElementData["elementID"]) : 0;
                        bool elementHasToDatabase = false;
                        if (databaseElementId == element.elementID) elementHasToDatabase = true; //Element zaten veri tabaninda var mi?

                        if (!elementHasToDatabase)
                        {
                            Dictionary<string, object> elementUpdates = new Dictionary<string, object>()
                            {
                                { "elementID", element.elementID }
                            };

                            elementRef.UpdateAsync(elementUpdates).ContinueWithOnMainThread(updateElementTask =>
                            {
                                if (updateElementTask.IsCompleted)
                                {
                                    Debug.Log($"Element updated successfully: {elementRef.Id}");

                                    // ElementID'yi iþlenmiþ olarak ekleyelim, böylece ayný element tekrar iþlenmez
                                    processedElementIDs.Add(element.elementID);
                                }
                                else
                                {
                                    Debug.LogError($"Failed to update element data: {updateElementTask.Exception}");
                                }
                            });
                        }
                        else
                            Debug.Log($"{element.customizeSlot} slotunda ki {element.elementID}'li element zaten veritabaninda bulunmaktadir.");
                    }
                    else
                    {
                        // Eleman bulunamadýysa yeni eleman olarak ekle
                        Debug.Log($"No element found with elementID: {element.elementID}, adding as new.");
                    }
                }
                else
                {
                    Debug.LogError($"Error querying element data: {task.Exception}");
                }
            });

        }
    }

    void ExtraCustomizationsFilling(CharacterCustomizeData _characterCustomizeData)
    {
        if (_characterCustomizeData.playerCustomizeData == null)
            _characterCustomizeData.playerCustomizeData = new();

        if (_characterCustomizeData.playerCustomizeData.AllCustomizeData == null)
            _characterCustomizeData.playerCustomizeData.AllCustomizeData = new();

        if (_characterCustomizeData.unlockedCustomizeElementIDs == null)
            _characterCustomizeData.unlockedCustomizeElementIDs = new();

        while (_characterCustomizeData.playerCustomizeData.AllCustomizeData.Count < 3)
            _characterCustomizeData.playerCustomizeData.AllCustomizeData.Add(new() { ID = _characterCustomizeData.playerCustomizeData.AllCustomizeData.Count, CustomizeElements = GetDefaultElements(), isFemale = false });


        foreach (var item in _characterCustomizeData.playerCustomizeData.AllCustomizeData)
        {
            if (item.Colors == null)
                item.Colors = new();

            if (item.Colors.Count == 0)
                foreach (var item2 in DefaultColors)
                    item.Colors.Add(item2);
        }
    }
    List<CustomizeElement> GetDefaultElements()
    {
        List<CustomizeElement> _default = new();

        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Head, elementID = 1001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.FacialHair, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Torso, elementID = 4004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Upper_Right, elementID = 5001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Upper_Left, elementID = 6001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Lower_Right, elementID = 7001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Lower_Left, elementID = 8001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hand_Right, elementID = 9001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hand_Left, elementID = 10001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hip, elementID = 11004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Leg_Right, elementID = 12003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Leg_Left, elementID = 13003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Helmet, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Eyebrows, elementID = 102003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hair, elementID = 103004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hat, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Mask, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Helmet_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Back_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Shoulder_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Shoulder_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elbow_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elbow_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hip_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Knee_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Knee_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elf_Ear, elementID = 0 });

        return _default;
    }
}
