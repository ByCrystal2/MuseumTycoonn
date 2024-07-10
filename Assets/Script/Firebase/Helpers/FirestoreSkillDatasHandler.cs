using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FirestoreSkillDatasHandler : MonoBehaviour
{
    FirebaseFirestore db;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async System.Threading.Tasks.Task AddSkillWithUserId(string userId, SkillNode _node)
    {
        // Kullanýcý ID'si ile belgeyi sorgula
        if (GameManager.instance != null) if (!GameManager.instance.IsWatchTutorial) return;
        Query query = db.Collection("Skills").WhereEqualTo("userID", userId);

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

                    CheckAndAddSkillData(documentReference, _node);
                }
                else
                {

                    Dictionary<string, object> newDocument = new Dictionary<string, object>
                    {
                        { "userID", userId }
                    };

                    db.Collection("Skills").AddAsync(newDocument).ContinueWithOnMainThread(addTask =>
                    {
                        if (addTask.IsCompleted)
                        {
                            documentReference = addTask.Result;
                            Debug.Log($"User document created for user {userId}");
                            CheckAndAddSkillData(documentReference, _node);
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

    private void CheckAndAddSkillData(DocumentReference documentReference, SkillNode _skill)    {

        Debug.Log("Skill CheckAndAdding... Skill name is" + _skill.SkillName);
        CollectionReference skillDatasRef = documentReference.Collection("SkillDatas");
        Query query = skillDatasRef.WhereEqualTo("ID", _skill.ID);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Documents.Count() > 0)
                {
                    Debug.Log($"Skill with ID {_skill.ID} already exists for user.");
                }
                else
                {
                    try
                    {
                        List<eStat> buffs = new List<eStat>();
                        int length1 = _skill.buffs.Count;
                        for (int i = 0; i < length1; i++)
                        {
                            buffs.Add(_skill.buffs[i]);
                        }
                        List<int> buffAmounts = new List<int>();
                        int length = _skill.Amounts.Count;
                        for (int i = 0; i < length; i++)
                        {
                            buffAmounts.Add(_skill.Amounts[i]);
                        }
                        Dictionary<string, object> skillData = new Dictionary<string, object>
                    {
                        { "ID", _skill.ID },
                        { "RequiredPoint", _skill.SkillRequiredPoint },
                        { "RequiredMoney", _skill.SkillRequiredMoney },
                        { "CurrentLevel", _skill.SkillCurrentLevel },
                        { "MaxLevel", _skill.SkillMaxLevel },
                        { "IsLocked", _skill.IsLocked },
                        { "IsPurchased", _skill.IsPurchased },
                        { "Buffs", buffs },
                        { "BuffAmounts", buffAmounts },
                        { "Timestamp", FieldValue.ServerTimestamp }
                    };

                        skillDatasRef.AddAsync(skillData).ContinueWithOnMainThread(addTask =>
                        {
                            if (addTask.IsCompleted)
                            {
                                Debug.Log($"Skill data with ID {_skill.ID} successfully added.");
                            }
                            else if (addTask.IsFaulted)
                            {
                                Debug.LogError($"Failed to add skill data: {addTask.Exception}");
                            }
                        });
                    }
                    catch (Exception _ex)
                    {
                        Debug.LogError("Skill Updating Error: " + _ex.Message);
                    }
                }
            }
            else
            {
                Debug.LogError($"Error querying skill data: {task.Exception}");
            }
        });
    }

    public async System.Threading.Tasks.Task<SkillNode> GetSkillInDatabase(string userId, int _skillId)
    {
        SkillNode foundSkill = null;

        try
        {
            // Kullanýcýya ait belgeleri sorgula
            Query query = db.Collection("Skills").WhereEqualTo("userID", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    CollectionReference skillDatasRef = documentSnapshot.Reference.Collection("SkillDatas");
                    Query skillQuery = skillDatasRef.WhereEqualTo("ID", _skillId);
                    QuerySnapshot skillQuerySnapshot = await skillQuery.GetSnapshotAsync();

                    foreach (DocumentSnapshot skillDocumentSnapshot in skillQuerySnapshot.Documents)
                    {
                        if (skillDocumentSnapshot.Exists)
                        {
                            // Eþleþen tabloyu bulun
                            var skillData = skillDocumentSnapshot.ToDictionary();
                            SkillNode helperSkillData = SkillTreeManager.instance.skillNodes.Where(x => x.ID == _skillId).SingleOrDefault();
                            if (helperSkillData != null)
                            {
                                foundSkill = helperSkillData;
                                foundSkill.buffs.Clear();
                                foundSkill.Amounts.Clear();
                                foundSkill.SkillCurrentLevel = skillData.ContainsKey("CurrentLevel") ? Convert.ToInt32(skillData["CurrentLevel"]) : 0;
                                foundSkill.SkillMaxLevel = skillData.ContainsKey("MaxLevel") ? Convert.ToInt32(skillData["MaxLevel"]) : 0;
                                foundSkill.SkillRequiredMoney = skillData.ContainsKey("RequiredMoney") ? Convert.ToSingle(skillData["RequiredMoney"]) : 0;
                                foundSkill.SkillRequiredPoint = skillData.ContainsKey("RequiredPoint") ? Convert.ToSingle(skillData["RequiredPoint"]) : 0;

                                foundSkill.IsLocked = skillData.ContainsKey("IsLocked") && Convert.ToBoolean(skillData["IsLocked"]);
                                foundSkill.IsPurchased = skillData.ContainsKey("IsPurchased") && Convert.ToBoolean(skillData["IsPurchased"]);
                                List<int> amounts = skillData.ContainsKey("BuffAmounts") ? ((List<object>)skillData["BuffAmounts"]).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                                List<eStat> buffs = skillData.ContainsKey("Buffs") ? ((List<object>)skillData["Buffs"]).Select(x => (eStat)Enum.Parse(typeof(eStat), x.ToString())).ToList() : new List<eStat>();
                                foundSkill.Amounts.Clear();
                                foundSkill.buffs.Clear();
                                foundSkill.Amounts = amounts;
                                foundSkill.buffs = buffs;
                            }
                        }
                    }

                    if (foundSkill != null)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting statue data: {ex.Message}");
        }

        return foundSkill;
    }
}
