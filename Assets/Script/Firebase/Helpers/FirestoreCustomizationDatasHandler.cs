using Firebase.Extensions;
using Firebase.Firestore;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

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

    public async Task AddCustomizationDataWithUserId(string userId, CharacterCustomizeData _customizeData)
    {
        // Kullan�c� ID'si ile belgeyi sorgula
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
        // Alt koleksiyon olan PictureDatas'� sorgula
        CollectionReference customizeDatasRef = documentReference.Collection("CustomizationDatas");
        Query query = customizeDatasRef.WhereEqualTo("SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot);

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
                    PlayerExtraCustomizeData selectedCustomizeSlotData = _customizeData.playerCustomizeData.AllCustomizeData[_customizeData.playerCustomizeData.selectedCustomizeSlot];
                    Dictionary<string, object> newCustomizeData = new Dictionary<string, object>
                    {
                        { "SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot },
                        { "IsFemale", selectedCustomizeSlotData.isFemale },
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
                    int lastSelectedCustomizeHeader = mainCustomizationData.ContainsKey("LastSelectedCustomizeHeader") ? Convert.ToInt32(mainCustomizationData["LastSelectedCustomizeHeader"]) : 0;
                List<int> unlockedCustomizeElementIDs = new List<int>();
                if (mainCustomizationData != null && mainCustomizationData.ContainsKey("UnlockedCustomizeElementIDs") && mainCustomizationData["UnlockedCustomizeElementIDs"] is List<object> list)
                {
                    unlockedCustomizeElementIDs = list.Select(x => Convert.ToInt32(x)).ToList();
                }

                    foundCustomizeData.SetDatas(new PlayerCustomizeData(), unlockedCustomizeElementIDs, lastSelectedCustomizeCategory, lastSelectedCustomizeHeader, lastSelectedColorHeader);
                    Debug.Log("foundCustomizeData.unlockedCustomizeElementIDs.Count => " + foundCustomizeData.unlockedCustomizeElementIDs.Count);
                    if (foundCustomizeData.playerCustomizeData.AllCustomizeData == null)
                        foundCustomizeData.playerCustomizeData.AllCustomizeData = new();

                    for (int i = 0; i < 3; i++)
                    {
                        Query customDataQuery = customizationDatasCollection.WhereEqualTo("SlotNumber", i);
                        QuerySnapshot customQuerySnapshot = await customDataQuery.GetSnapshotAsync();

                        PlayerExtraCustomizeData playerExtra = null;
                        foreach (DocumentSnapshot customDocumentSnapshot in customQuerySnapshot.Documents)
                        {
                            if (customDocumentSnapshot.Exists)
                            {
                                playerExtra = new();
                                var customData = customDocumentSnapshot.ToDictionary();
                                playerExtra.ID = customData.ContainsKey("ID") ? Convert.ToInt32(customData["ID"]) : 0;
                                playerExtra.isFemale = customData.ContainsKey("IsFemale") && Convert.ToBoolean(customData["IsFemale"]);

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
    public void UpdateCustomizationData(string userId, CharacterCustomizeData _customizeData)
    {
        if (GameManager.instance != null && !GameManager.instance.IsWatchTutorial) return;

        // Kullan�c� ID'si ile belgeyi sorgula
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

                    // Belgeyi bulduysak alt koleksiyon olan CustomizationDatas'ta tabloyu bul ve g�ncelle
                    UpdateCustomizationDatas(documentReference, _customizeData);
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

    private void UpdateCustomizationDatas(DocumentReference documentReference, CharacterCustomizeData _customizeData)
    {
        CollectionReference customizationDatasRef = documentReference.Collection("CustomizationDatas");
        Query customizeDataQuery = customizationDatasRef.WhereEqualTo("SlotNumber", _customizeData.playerCustomizeData.selectedCustomizeSlot);

        customizeDataQuery.GetSnapshotAsync().ContinueWithOnMainThread(customizeDataTask =>
        {
            if (customizeDataTask.IsCompleted)
            {
                QuerySnapshot customizeDataSnapshot = customizeDataTask.Result;

                if (customizeDataSnapshot.Documents.Count() > 0)
                {
                    List<CustomizeElement> newElements = new List<CustomizeElement>();
                    ProcessCustomizeData(customizeDataSnapshot, customizationDatasRef, _customizeData, newElements);

                    // Yeni elemanlar varsa bunlar� ekle
                    if (newElements.Count > 0)
                    {
                        AddNewElements(customizationDatasRef, newElements);
                    }
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

    private void ProcessCustomizeData(QuerySnapshot customizeDataSnapshot, CollectionReference customizationDatasRef, CharacterCustomizeData _customizeData, List<CustomizeElement> newElements)
    {
        List<PlayerExtraCustomizeData> playerExtraCustomizeDatas = _customizeData.playerCustomizeData.AllCustomizeData;
        HashSet<int> processedSlots = new HashSet<int>();  // Ayn� slotun tekrar i�lenmesini �nlemek i�in

        foreach (var document in customizeDataSnapshot.Documents)
        {
            DocumentReference customizeDataRef = document.Reference;

            foreach (var extraData in playerExtraCustomizeDatas)
            {
                if (processedSlots.Contains(extraData.ID)) continue;  // Ayn� slot bir kez i�lenir
                processedSlots.Add(extraData.ID);

                Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "IsFemale", extraData.isFemale },
                { "Timestamp", FieldValue.ServerTimestamp }
            };

                customizeDataRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        UpdateCustomizeElements(customizeDataRef, extraData, newElements);
                    }
                    else
                    {
                        Debug.LogError($"Failed to update customization data: {updateTask.Exception}");
                    }
                });
            }
        }
    }

    private void UpdateCustomizeElements(DocumentReference customizeDataRef, PlayerExtraCustomizeData extraData, List<CustomizeElement> newElements)
    {
        CollectionReference elementDatasRef = customizeDataRef.Collection("CustomizeElements");

        // ��lenen elementID'leri takip etmek i�in bir HashSet olu�turuyoruz
        HashSet<int> processedElementIDs = new HashSet<int>();

        foreach (CustomizeElement element in extraData.CustomizeElements)
        {
            // E�er elementID daha �nce i�lendiyse, bu d�ng�y� atla
            if (processedElementIDs.Contains(element.elementID)) continue;

            Query query = elementDatasRef.WhereEqualTo("elementID", element.elementID);

            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;

                    if (snapshot.Documents.Count() > 0)
                    {
                        // Mevcut elemanlar� g�ncelle
                        foreach (DocumentSnapshot elementDoc in snapshot.Documents)
                        {
                            DocumentReference elementRef = elementDoc.Reference;
                            Dictionary<string, object> elementUpdates = new Dictionary<string, object>
                        {
                            { "CustomizeSlot", element.customizeSlot },
                            { "elementID", element.elementID }
                        };

                            elementRef.UpdateAsync(elementUpdates).ContinueWithOnMainThread(updateElementTask =>
                            {
                                if (updateElementTask.IsCompleted)
                                {
                                    Debug.Log($"Element updated successfully: {elementRef.Id}");

                                    // ElementID'yi i�lenmi� olarak ekleyelim, b�ylece ayn� element tekrar i�lenmez
                                    processedElementIDs.Add(element.elementID);
                                }
                                else
                                {
                                    Debug.LogError($"Failed to update element data: {updateElementTask.Exception}");
                                }
                            });
                        }
                    }
                    else
                    {
                        // Eleman bulunamad�ysa yeni eleman olarak ekle
                        Debug.Log($"No element found with elementID: {element.elementID}, adding as new.");
                        newElements.Add(element);
                    }
                }
                else
                {
                    Debug.LogError($"Error querying element data: {task.Exception}");
                }
            });
        }
    }


    private void AddNewElements(CollectionReference customizationDatasRef, List<CustomizeElement> newElements)
    {
        foreach (CustomizeElement newElement in newElements)
        {
            customizationDatasRef.Document(newElement.elementID.ToString()).SetAsync(new Dictionary<string, object>
        {
            { "CustomizeSlot", newElement.customizeSlot },
            { "elementID", newElement.elementID }
        }).ContinueWithOnMainThread(addTask =>
        {
            if (addTask.IsCompleted)
            {
                Debug.Log($"New element added: {newElement.elementID}");
            }
            else
            {
                Debug.LogError($"Failed to add new element: {addTask.Exception}");
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
