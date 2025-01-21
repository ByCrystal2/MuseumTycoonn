using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class LanguageDatabase : MonoBehaviour
{
    public MainLanguageData Language = new MainLanguageData();
    public static LanguageDatabase instance { get; private set; }
    [SerializeField] bool shouldTranslateAndSave = false;
    [Header("NPCManager'da ceviri islemi gerceklessin mi?\n(Toplu ceviri islemi yapilacaksa kapatilmasi onerilir.)")]
    [SerializeField] public bool TranslationWillBeProcessed;

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

    private void Update()
    {
        if (!TranslationWillBeProcessed && shouldTranslateAndSave && !waitBeforeTranslate)
        {
            shouldTranslateAndSave = false;
            TranslateAndSaveAllLanguageData();
        }
    }

    public async Task LoadLanguageData()
    {
        string filePath = $"Languages/Language_{GameManager.instance.GetLanguageShortString(GameManager.instance.GetGameLanguage())}";

        TextAsset jsonAsset = Resources.Load<TextAsset>(filePath);
        if (jsonAsset != null)
        {
            // JSON dosyasýný yükle.
            Debug.Log("jsonAsset is not null. filePath => " + filePath);
            Language = JsonUtility.FromJson<MainLanguageData>(jsonAsset.text);
        }
        else
        {
            // Dosya yoksa çeviri yap ve JSON dosyasýný oluþtur.
            Debug.Log("jsonAsset is null. filePath => " + filePath);
            await TranslateAndSaveLanguageData();
        }
    }

    void LanguageFilling()
    {
        //Items
        List<ItemData> allItems = ItemManager.instance.AllItems;
        int allItemLength = allItems.Count;
        for (int i = 0; i < allItemLength; i++)
        {
            if (allItems[i].Description == string.Empty)
                continue;
            Language.ItemDescriptions.Add(new LanguageData(allItems[i].ID, allItems[i].Description));
        }
        //Items

        //Skills
        List<SkillNode> allSkills = SkillTreeManager.instance.skillNodes.ToList();
        int allSkillsLength = allSkills.Count;
        for (int i = 0; i < allSkillsLength; i++)
        {
            if (allSkills[i].SkillName == string.Empty)
                continue;
            Language.SkillNames.Add(new LanguageData(allSkills[i].ID, allSkills[i].SkillName));
        }

        for (int i = 0; i < allSkillsLength; i++)
        {
            if (allSkills[i].SkillDescription == string.Empty)
                continue;
            Language.SkillDescriptions.Add(new LanguageData(allSkills[i].ID, allSkills[i].SkillDescription));
        }
        for (int i = 0; i < allSkillsLength; i++)
        {
            if (allSkills[i].SkillEffect == string.Empty)
                continue;
            Language.SkillEffects.Add(new LanguageData(allSkills[i].ID, allSkills[i].SkillEffect));
        }

        for (int i = 0; i < allSkillsLength; i++)
        {
            if (allSkills[i].defaultEffectString == string.Empty)
                continue;
            Language.SkillDefaultStrings.Add(new LanguageData(allSkills[i].ID, allSkills[i].defaultEffectString));
        }

        Language.SkillInfoStrings = new List<LanguageData>() { new LanguageData(0, "Required Skill Score"), new LanguageData(1, "Max Level"), new LanguageData(2, "Increase Level"), new LanguageData(3, "Locked") };
        //Skills

        //NPCCommends
        List<TableCommentEvaluationData> allComment = TableCommentEvaluationManager.instance.datas;
        int allCommentLength = allComment.Count;
        for (int i = 0; i < allCommentLength; i++)
        {
            if (allComment[i].Message == string.Empty)
                continue;
            Language.CommendEvulations.Add(new LanguageData(allComment[i].ID, allComment[i].Message));
        }
        //NPCCommends

        //Notifications
        List<Notification> allNotifications = NotificationManager.instance.Notifications;
        int allNotificationLength = allNotifications.Count;
        for (int i = 0; i < allNotificationLength; i++)
        {
            if (allNotifications[i].Message == string.Empty)
                continue;
            Language.NotificationMessages.Add(new LanguageData(allNotifications[i].ID, allNotifications[i].Message));
        }
        //Missions
        List<GameMission> allMissions = MissionManager.instance.GetAllMissionDatas();
        int allMissionLength = allMissions.Count;
        for (int i = 0; i < allMissionLength; i++)
        {
            if (allMissions[i].Header != string.Empty)                
                Language.MissionHeaderMessages.Add(new LanguageData(allMissions[i].ID, allMissions[i].Header));
            if (allMissions[i].Description != string.Empty)
                Language.MissionDescriptionMessages.Add(new LanguageData(allMissions[i].ID, allMissions[i].Description));
        }
        Language.MissionDescriptionMessages.Add(new LanguageData(999, "Required"));
        Language.MissionNpcInteractionTypeMessages = new List<LanguageData> { new LanguageData(0, "Visitor to interaction"), new LanguageData(1, "Visitor to beat") };
        Language.MissionNpcInteractionColorTypeMessages = new List<LanguageData> { new LanguageData(1, "loved"), new LanguageData(2, "unloved") };
        Language.MissionNpcInteractionColorMessages = new List<LanguageData> { new LanguageData(0, "Black"), new LanguageData(1, "White"), new LanguageData(2, "Red"), new LanguageData(3, "Green"), new LanguageData(4, "Blue"), new LanguageData(5, "Cyan"), new LanguageData(6, "Yellow"), new LanguageData(7, "Purple") };
        Language.MissionNpcInteractionStateMessages = new List<LanguageData> { new LanguageData(0, ""), new LanguageData(1, "Happy"), new LanguageData(2, "Sad"), new LanguageData(3, "Stressed"), new LanguageData(4, "Relaxed"), new LanguageData(5, "Need to toilet") };
        Language.MissionNpcInteractionHelperMessages = new List<LanguageData> { new LanguageData(0, "and") };
        //Missions
        //Notifications
        //DialogsMessages
        if (TutorialLevelManager.instance != null)
        {
            var dialogs = TutorialLevelManager.instance.DialogsMessages;
            int allDialogLength = dialogs.Count;
            for (int i = 0; i < allDialogLength; i++)
            {
                var dialog = dialogs[i];
                int dialogId = dialog.ID;
                int messagesLength = dialog.messages.Count;
                for (int k = 0; k < messagesLength; k++)
                {
                    if (dialog.messages[k] == string.Empty)
                        continue;
                    Language.DialogsMessages.Add(new LanguageData(dialogId, dialog.messages[k]));
                }
            }
        }
        //DialogsMessages
        //General
        Language.PictureInfoStrings = new List<LanguageData>() { new LanguageData(0, "Added"), new LanguageData(1, "Updated"), new LanguageData(2, "Select Painting"), new LanguageData(3, "Enough"), new LanguageData(4, "Add"), new LanguageData(5, "Update"), new LanguageData(6, "Insufficient") };

        Language.InternetCheckInfoStrings = new List<LanguageData>() { new LanguageData(0, "Internet Connection"), new LanguageData(1, "Your internet connection has been lost."), new LanguageData(2, "Please check your connection."), new LanguageData(3, "Your internet connection has been restored.") };

        Language.ShopQuestionInfoStrings = new List<LanguageData>() { new LanguageData(0, "Purchase Process"), new LanguageData(1, "Do you confirm the purchase?"), new LanguageData(2, "Available"), new LanguageData(3, "Gem"), new LanguageData(4, "Gold"), new LanguageData(5, "Product Price"), new LanguageData(6, "Paid") };

        Language.WorkerInfoStrings = new List<LanguageData>() { new LanguageData(0, "Security"), new LanguageData(1, "Housekeeper"), new LanguageData(2, "Musician"), new LanguageData(3, "Receptionist"), new LanguageData(4, "Brochure Seller") };

        //Customization
        Language.customize_SelectedAnEquipmentStrings = new List<LanguageData> { new LanguageData(0, "Do you want to buy the item?"), new LanguageData(1, "Price") };
        Language.customize_OnExitProcessStrings = new List<LanguageData> { new LanguageData(0, "Karakter Özelleþtirmesi"), new LanguageData(1, "Deðiþiklikleri onaylýyor musunuz?") };
        //Customization
        //General
    }
    bool waitBeforeTranslate;
    private async Task TranslateAndSaveLanguageData()
    {
        waitBeforeTranslate = true;
        LanguageFilling();
        //ITEMS
        List<string> itemDescKeysToTranslate = new List<string>();
        List<string> itemDescTranslatedTexts = new List<string>();
        //ITEMS
        //SKILLS
        List<string> skillNameKeysToTranslate = new List<string>();
        List<string> skillNameTranslatedTexts = new List<string>();

        List<string> skillDescKeysToTranslate = new List<string>();
        List<string> skillDescTranslatedTexts = new List<string>();

        List<string> skillEffectKeysToTranslate = new List<string>();
        List<string> skillEffectTranslatedTexts = new List<string>();

        List<string> skillDefaultStringKeysToTranslate = new List<string>();
        List<string> skillDefaultStringTranslatedTexts = new List<string>();

        List<string> skillInfoKeysToTranslate = new List<string>();
        List<string> skillInfoTranslatedTexts = new List<string>();
        //SKILLS

        //NPCS
        List<string> npcCommendKeysToTranslate = new List<string>();
        List<string> npcCommendTranslatedTexts = new List<string>();
        //NPCS

        //NOTIFICATIONS
        List<string> notificationKeysToTranslate = new List<string>();
        List<string> notificationTranslatedTexts = new List<string>();

            //Missions
            List<string> M_HeaderKeysToTranslate = new List<string>();
            List<string> M_HeaderTranslatedTexts = new List<string>();

            List<string> M_DescKeysToTranslate = new List<string>();
            List<string> M_DescTranslatedTexts = new List<string>();
                //NPCInteraction
                List<string> M_NpcInteractionTypeKeysToTranslate = new List<string>();
                List<string> M_NpcInteractionTypeTranslatedTexts = new List<string>();

                List<string> M_NpcInteractionColorTypeKeysToTranslate = new List<string>();
                List<string> M_NpcInteractionColorTypeTranslatedTexts = new List<string>();

                List<string> M_NpcInteractionTargetColorKeysToTranslate = new List<string>();
                List<string> M_NpcInteractionTargetColorTranslatedTexts = new List<string>();

                List<string> M_NpcInteractionStateKeysToTranslate = new List<string>();
                List<string> M_NpcInteractionStateTranslatedTexts = new List<string>();

                List<string> M_NpcInteractionHelperKeysToTranslate = new List<string>();
                List<string> M_NpcInteractionHelperTranslatedTexts = new List<string>();
                //NPCInteraction
            //Missions
        //NOTIFICATIONS

        //Dialogs
        List<string> dialogKeysToTranslate = new List<string>();
        List<string> dialogTranslatedTexts = new List<string>();
        //Dialogs

        //General
        List<string> pictureInfoKeysToTranslate = new List<string>();
        List<string> pictureInfoTranslatedTexts = new List<string>();

        List<string> internetCheckInfoKeysToTranslate = new List<string>();
        List<string> internetCheckInfoTranslatedTexts = new List<string>();

        List<string> shopQuestionInfoKeysToTranslate = new List<string>();
        List<string> shopQuestionInfoTranslatedTexts = new List<string>();

        List<string> workerInfoKeysToTranslate = new List<string>();
        List<string> workerInfoTranslatedTexts = new List<string>();

        //Customization
        List<string> customize_OnSelectedAnEquipmentKeysToTranslate = new List<string>();
        List<string> customize_OnSelectedAnEquipmentTranslatedTexts = new List<string>();

        List<string> customize_OnExitProcessKeysToTranslate = new List<string>();
        List<string> customize_OnExitProcessTranslatedTexts = new List<string>();
        //Customization
        //General

        //SKILLS
        foreach (var item in Language.SkillNames)
        {
            skillNameKeysToTranslate.Add(item.Key);
        }
        await GameManager.instance.BulkTranslateAndAssignAsync(skillNameKeysToTranslate, (result) =>
        {
            skillNameTranslatedTexts = result;
        });

        foreach (var item in Language.SkillDescriptions)
        {
            skillDescKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(skillDescKeysToTranslate, (result) =>
        {
            skillDescTranslatedTexts = result;
        });

        foreach (var item in Language.SkillEffects)
        {
            skillEffectKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(skillEffectKeysToTranslate, (result) =>
        {
            skillEffectTranslatedTexts = result;
        });

        foreach (var item in Language.SkillDefaultStrings)
        {
            skillDefaultStringKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(skillDefaultStringKeysToTranslate, (result) =>
        {
            skillDefaultStringTranslatedTexts = result;
        });

        foreach (var item in Language.SkillInfoStrings)
        {
            skillInfoKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(skillInfoKeysToTranslate, (result) =>
        {
            skillInfoTranslatedTexts = result;
        });
        //SKILLS

        //ITEM
        foreach (var item in Language.ItemDescriptions)
        {
            if (item.Key == string.Empty)
                continue;
            itemDescKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(itemDescKeysToTranslate, (result) =>
        {
            itemDescTranslatedTexts = result;
        });
        //ITEM

        //NPCS
        foreach (var item in Language.CommendEvulations)
        {
            if (item.Key == string.Empty)
                continue;
            npcCommendKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(npcCommendKeysToTranslate, (result) =>
        {
            npcCommendTranslatedTexts = result;
        });
        //NPCS

        //DIALOGS
        foreach (var item in Language.DialogsMessages)
        {
            if (item.Key == string.Empty)
                continue;
            dialogKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(dialogKeysToTranslate, (result) =>
        {
            dialogTranslatedTexts = result;
        });
        //DIALOGS

        //NOTIFICATIONS
        foreach (var item in Language.NotificationMessages)
        {
            if (item.Key == string.Empty)
                continue;
            notificationKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(notificationKeysToTranslate, (result) =>
        {
            notificationTranslatedTexts = result;
        });

            //Missions
            foreach (var item in Language.MissionHeaderMessages)
            {
                if (item.Key == string.Empty)
                    continue;
                M_HeaderKeysToTranslate.Add(item.Key);
            }

            await GameManager.instance.BulkTranslateAndAssignAsync(M_HeaderKeysToTranslate, (result) =>
            {
                M_HeaderTranslatedTexts = result;
            });

            foreach (var item in Language.MissionDescriptionMessages)
            {
                if (item.Key == string.Empty)
                    continue;
                M_DescKeysToTranslate.Add(item.Key);
            }

            await GameManager.instance.BulkTranslateAndAssignAsync(M_DescKeysToTranslate, (result) =>
            {
                M_DescTranslatedTexts = result;
            });

                //NPCInteraction
                foreach (var item in Language.MissionNpcInteractionTypeMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    M_NpcInteractionTypeKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(M_NpcInteractionTypeKeysToTranslate, (result) =>
                {
                    M_NpcInteractionTypeTranslatedTexts = result;
                });

                foreach (var item in Language.MissionNpcInteractionColorTypeMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    M_NpcInteractionColorTypeKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(M_NpcInteractionColorTypeKeysToTranslate, (result) =>
                {
                    M_NpcInteractionColorTypeTranslatedTexts = result;
                });

                foreach (var item in Language.MissionNpcInteractionColorMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    M_NpcInteractionTargetColorKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(M_NpcInteractionTargetColorKeysToTranslate, (result) =>
                {
                    M_NpcInteractionTargetColorTranslatedTexts = result;
                });

                foreach (var item in Language.MissionNpcInteractionStateMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    M_NpcInteractionStateKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(M_NpcInteractionStateKeysToTranslate, (result) =>
                {
                    M_NpcInteractionStateTranslatedTexts = result;
                });

                foreach(var item in Language.MissionNpcInteractionHelperMessages)
                        {
                    if (item.Key == string.Empty)
                        continue;
                    M_NpcInteractionHelperKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(M_NpcInteractionHelperKeysToTranslate, (result) =>
                {
                    M_NpcInteractionHelperTranslatedTexts = result;
                });
        //NPCInteraction
        //Missions

        //NOTIFICATIONS

        //GENERAL
        foreach (var item in Language.PictureInfoStrings)
        {
            if (item.Key == string.Empty)
                continue;
            pictureInfoKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(pictureInfoKeysToTranslate, (result) =>
        {
            pictureInfoTranslatedTexts = result;
        });

        foreach (var item in Language.InternetCheckInfoStrings)
        {
            if (item.Key == string.Empty)
                continue;
            internetCheckInfoKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(internetCheckInfoKeysToTranslate, (result) =>
        {
            internetCheckInfoTranslatedTexts = result;
        });

        foreach (var item in Language.ShopQuestionInfoStrings)
        {
            if (item.Key == string.Empty)
                continue;
            shopQuestionInfoKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(shopQuestionInfoKeysToTranslate, (result) =>
        {
            shopQuestionInfoTranslatedTexts = result;
        });

        foreach (var item in Language.WorkerInfoStrings)
        {
            if (item.Key == string.Empty)
                continue;
            workerInfoKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(workerInfoKeysToTranslate, (result) =>
        {
            workerInfoTranslatedTexts = result;
        });

        //Customization
        foreach (var item in Language.customize_SelectedAnEquipmentStrings)
        {
            if (item.Key == string.Empty)
                continue;
            customize_OnSelectedAnEquipmentKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(customize_OnSelectedAnEquipmentKeysToTranslate, (result) =>
        {
            customize_OnSelectedAnEquipmentTranslatedTexts = result;
        });

        foreach (var item in Language.customize_OnExitProcessStrings)
        {
            if (item.Key == string.Empty)
                continue;
            customize_OnExitProcessKeysToTranslate.Add(item.Key);
        }

        await GameManager.instance.BulkTranslateAndAssignAsync(customize_OnExitProcessKeysToTranslate, (result) =>
        {
            customize_OnExitProcessTranslatedTexts = result;
        });
        //Customization
        //GENERAL

        //ItemAdding
        Debug.Log("Language.ItemDescriptions.Count => " + Language.ItemDescriptions.Count + " itemDescTranslatedTexts.Count => " + itemDescTranslatedTexts.Count);
        for (int i = 0; i < itemDescTranslatedTexts.Count; i++)
        {
            Debug.Log("itemDescTranslatedTexts[i] => " + itemDescTranslatedTexts[i] + " and i => " + i);
            Language.ItemDescriptions[i].ActiveLanguage = itemDescTranslatedTexts[i];
        }
        //ItemAdding
        //SkillAdding
        Debug.Log("Language.SkillNames.Count => " + Language.SkillNames.Count + " skillNameTranslatedTexts.Count => " + skillNameTranslatedTexts.Count);
        for (int i = 0; i < skillNameTranslatedTexts.Count; i++)
        {
            Language.SkillNames[i].ActiveLanguage = skillNameTranslatedTexts[i];
        }
        Debug.Log("Language.SkillDescriptions.Count => " + Language.SkillDescriptions.Count + " skillDescTranslatedTexts.Count => " + skillDescTranslatedTexts.Count);
        for (int i = 0; i < skillDescTranslatedTexts.Count; i++)
        {
            if (i >= Language.SkillDescriptions.Count) break;
            Language.SkillDescriptions[i].ActiveLanguage = skillDescTranslatedTexts[i];
        }
        Debug.Log("Language.SkillEffects.Count => " + Language.SkillEffects.Count + " skillEffectTranslatedTexts.Count => " + skillEffectTranslatedTexts.Count);
        for (int i = 0; i < skillEffectTranslatedTexts.Count; i++)
        {
            Language.SkillEffects[i].ActiveLanguage = skillEffectTranslatedTexts[i];
        }
        Debug.Log("Language.SkillDefaultStrings.Count => " + Language.SkillDefaultStrings.Count + "skillDefaultStringTranslatedTexts.Count => " + skillDefaultStringTranslatedTexts.Count);
        for (int i = 0; i < skillDefaultStringTranslatedTexts.Count; i++)
        {
            Language.SkillDefaultStrings[i].ActiveLanguage = skillDefaultStringTranslatedTexts[i];
        }
        Debug.Log("Language.SkillInfoStrings.Count => " + Language.SkillInfoStrings.Count + " skillInfoTranslatedTexts.Count => " + skillInfoTranslatedTexts.Count);
        for (int i = 0; i < skillInfoTranslatedTexts.Count; i++)
        {
            Language.SkillInfoStrings[i].ActiveLanguage = skillInfoTranslatedTexts[i];
        }
        //SkillAdding

        //NPCAdding
        Debug.Log("Language.CommendEvulations.Count => " + Language.CommendEvulations.Count + " npcCommendTranslatedTexts.Count => " + npcCommendTranslatedTexts.Count);
        for (int i = 0; i < npcCommendTranslatedTexts.Count; i++)
        {
            Language.CommendEvulations[i].ActiveLanguage = npcCommendTranslatedTexts[i];
        }
        //NPCAdding

        //DialogAdding
        Debug.Log("Language.DialogsMessages.Count => " + Language.DialogsMessages.Count + " dialogTranslatedTexts.Count => " + dialogTranslatedTexts.Count);
        for (int i = 0; i < dialogTranslatedTexts.Count; i++)
        {
            Language.DialogsMessages[i].ActiveLanguage = dialogTranslatedTexts[i];
        }
        //DialogAdding

        //NotificationAdding
        Debug.Log("Language.NotificationMessages.Count => " + Language.NotificationMessages.Count + " notificationTranslatedTexts.Count => " + notificationTranslatedTexts.Count);
        for (int i = 0; i < notificationTranslatedTexts.Count; i++)
        {
            Language.NotificationMessages[i].ActiveLanguage = notificationTranslatedTexts[i];
        }
            //MissionsAdding
            Debug.Log("Language.MissionHeaderMessages.Count => " + Language.MissionHeaderMessages.Count + " M_HeaderTranslatedTexts.Count => " + M_HeaderTranslatedTexts.Count);
            for (int i = 0; i < M_HeaderTranslatedTexts.Count; i++)
            {
                Language.MissionHeaderMessages[i].ActiveLanguage = M_HeaderTranslatedTexts[i];
            }

            Debug.Log("Language.MissionDescriptionMessages.Count => " + Language.MissionDescriptionMessages.Count + " M_DescTranslatedTexts.Count => " + M_DescTranslatedTexts.Count);
            for (int i = 0; i < M_DescTranslatedTexts.Count; i++)
            {
                Language.MissionDescriptionMessages[i].ActiveLanguage = M_DescTranslatedTexts[i];
            }

                //NPCInteractionAdding
                Debug.Log("Language.MissionNpcInteractionTypeMessages.Count => " + Language.MissionNpcInteractionTypeMessages.Count + " M_NpcInteractionTypeTranslatedTexts.Count => " + M_NpcInteractionTypeTranslatedTexts.Count);
                for (int i = 0; i < M_NpcInteractionTypeTranslatedTexts.Count; i++)
                {
                    Language.MissionNpcInteractionTypeMessages[i].ActiveLanguage = M_NpcInteractionTypeTranslatedTexts[i];
                }

                Debug.Log("Language.MissionNpcInteractionColorTypeMessages.Count => " + Language.MissionNpcInteractionColorTypeMessages.Count + " M_NpcInteractionColorTypeTranslatedTexts.Count => " + M_NpcInteractionColorTypeTranslatedTexts.Count);
                for (int i = 0; i < M_NpcInteractionColorTypeTranslatedTexts.Count; i++)
                {
                    Language.MissionNpcInteractionColorTypeMessages[i].ActiveLanguage = M_NpcInteractionColorTypeTranslatedTexts[i];
                }

                Debug.Log("Language.MissionNpcInteractionColorMessages.Count => " + Language.MissionNpcInteractionColorMessages.Count + " M_NpcInteractionTargetColorTranslatedTexts.Count => " + M_NpcInteractionTargetColorTranslatedTexts.Count);
                for (int i = 0; i < M_NpcInteractionTargetColorTranslatedTexts.Count; i++)
                {
                    Language.MissionNpcInteractionColorMessages[i].ActiveLanguage = M_NpcInteractionTargetColorTranslatedTexts[i];
                }

                Debug.Log("Language.MissionNpcInteractionStateMessages.Count => " + Language.MissionNpcInteractionStateMessages.Count + " M_NpcInteractionStateTranslatedTexts.Count => " + M_NpcInteractionStateTranslatedTexts.Count);
                for (int i = 0; i < M_NpcInteractionStateTranslatedTexts.Count; i++)
                {
                    Language.MissionNpcInteractionStateMessages[i].ActiveLanguage = M_NpcInteractionStateTranslatedTexts[i];
                }

                Debug.Log("Language.MissionNpcInteractionHelperMessages.Count => " + Language.MissionNpcInteractionHelperMessages.Count + " M_NpcInteractionHelperTranslatedTexts.Count => " + M_NpcInteractionHelperTranslatedTexts.Count);
                for (int i = 0; i < M_NpcInteractionHelperTranslatedTexts.Count; i++)
                {
                    Language.MissionNpcInteractionHelperMessages[i].ActiveLanguage = M_NpcInteractionHelperTranslatedTexts[i];
                }

                //NPCInteractionAdding
            //MissionsAdding
        //NotificationAdding

        //GeneralAdding
        Debug.Log("Language.PictureInfoStrings.Count => " + Language.PictureInfoStrings.Count + "pictureInfoTranslatedTexts.Count => " + pictureInfoTranslatedTexts.Count);
        for (int i = 0; i < pictureInfoTranslatedTexts.Count; i++)
        {
            Language.PictureInfoStrings[i].ActiveLanguage = pictureInfoTranslatedTexts[i];
        }

        Debug.Log("Language.InternetCheckInfoStrings.Count => " + Language.InternetCheckInfoStrings.Count + "internetCheckInfoTranslatedTexts.Count => " + internetCheckInfoTranslatedTexts.Count);
        for (int i = 0; i < internetCheckInfoTranslatedTexts.Count; i++)
        {
            Language.InternetCheckInfoStrings[i].ActiveLanguage = internetCheckInfoTranslatedTexts[i];
        }

        Debug.Log("Language.ShopQuestionInfoStrings.Count => " + Language.ShopQuestionInfoStrings.Count + "shopQuestionInfoTranslatedTexts.Count => " + shopQuestionInfoTranslatedTexts.Count);
        for (int i = 0; i < shopQuestionInfoTranslatedTexts.Count; i++)
        {
            Language.ShopQuestionInfoStrings[i].ActiveLanguage = shopQuestionInfoTranslatedTexts[i];
        }

        Debug.Log("Language.WorkerInfoStrings.Count => " + Language.WorkerInfoStrings.Count + "workerInfoTranslatedTexts.Count => " + workerInfoTranslatedTexts.Count);
        for (int i = 0; i < workerInfoTranslatedTexts.Count; i++)
        {
            Language.WorkerInfoStrings[i].ActiveLanguage = workerInfoTranslatedTexts[i];
        }

        //Customization
        Debug.Log("Language.customize_SelectedAnEquipmentStrings.Count => " + Language.customize_SelectedAnEquipmentStrings.Count + "customize_OnSelectedAnEquipmentTranslatedTexts.Count => " + customize_OnSelectedAnEquipmentTranslatedTexts.Count);
        for (int i = 0; i < customize_OnSelectedAnEquipmentTranslatedTexts.Count; i++)
        {
            Language.customize_SelectedAnEquipmentStrings[i].ActiveLanguage = customize_OnSelectedAnEquipmentTranslatedTexts[i];
        }

        Debug.Log("Language.customize_OnExitProcessStrings.Count => " + Language.customize_OnExitProcessStrings.Count + "customize_OnExitProcessTranslatedTexts.Count => " + customize_OnExitProcessTranslatedTexts.Count);
        for (int i = 0; i < customize_OnExitProcessTranslatedTexts.Count; i++)
        {
            Language.customize_OnExitProcessStrings[i].ActiveLanguage = customize_OnExitProcessTranslatedTexts[i];
        }
        //Customization
        //GeneralAdding
        // JSON dosyasýný kaydet
        string jsonString = JsonUtility.ToJson(Language);
        string directoryPath = "Assets/Resources/Languages"; // Resources klasörü içindeki Languages klasörü
        string filePath = Path.Combine(directoryPath, $"Language_{GameManager.instance.GetLanguageShortString(GameManager.instance.GetGameLanguage())}.json");

        // Dosya yolu var mý kontrol et, yoksa oluþtur
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Dosyayý oluþtur ve içine yaz
        File.WriteAllText(filePath, jsonString);

        Debug.Log("Çeviri ve kayýt iþlemi tamamlandý.");
        waitBeforeTranslate = false;
    }
    public async Task TranslateAndSaveAllLanguageData()
    {
        try
        {
            waitBeforeTranslate = true;
            LanguageFilling();

            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                //ITEMS
                List<string> itemDescKeysToTranslate = new List<string>();
                List<string> itemDescTranslatedTexts = new List<string>();
                //ITEMS
                //SKILLS
                List<string> skillNameKeysToTranslate = new List<string>();
                List<string> skillNameTranslatedTexts = new List<string>();

                List<string> skillDescKeysToTranslate = new List<string>();
                List<string> skillDescTranslatedTexts = new List<string>();

                List<string> skillEffectKeysToTranslate = new List<string>();
                List<string> skillEffectTranslatedTexts = new List<string>();

                List<string> skillDefaultStringKeysToTranslate = new List<string>();
                List<string> skillDefaultStringTranslatedTexts = new List<string>();

                List<string> skillInfoKeysToTranslate = new List<string>();
                List<string> skillInfoTranslatedTexts = new List<string>();
                //SKILLS

                //NPCS
                List<string> npcCommendKeysToTranslate = new List<string>();
                List<string> npcCommendTranslatedTexts = new List<string>();
                //NPCS

                //Dialogs
                List<string> dialogKeysToTranslate = new List<string>();
                List<string> dialogTranslatedTexts = new List<string>();
                //Dialogs

                //NOTIFICATIONS
                List<string> notificationKeysToTranslate = new List<string>();
                List<string> notificationTranslatedTexts = new List<string>();

                    //Missions
                    List<string> M_HeaderKeysToTranslate = new List<string>();
                    List<string> M_HeaderTranslatedTexts = new List<string>();

                    List<string> M_DescKeysToTranslate = new List<string>();
                    List<string> M_DescTranslatedTexts = new List<string>();
                        //NPCInteraction
                        List<string> M_NpcInteractionTypeKeysToTranslate = new List<string>();
                        List<string> M_NpcInteractionTypeTranslatedTexts = new List<string>();

                        List<string> M_NpcInteractionColorTypeKeysToTranslate = new List<string>();
                        List<string> M_NpcInteractionColorTypeTranslatedTexts = new List<string>();

                        List<string> M_NpcInteractionTargetColorKeysToTranslate = new List<string>();
                        List<string> M_NpcInteractionTargetColorTranslatedTexts = new List<string>();

                        List<string> M_NpcInteractionStateKeysToTranslate = new List<string>();
                        List<string> M_NpcInteractionStateTranslatedTexts = new List<string>();

                        List<string> M_NpcInteractionHelperKeysToTranslate = new List<string>();
                        List<string> M_NpcInteractionHelperTranslatedTexts = new List<string>();
                        //NPCInteraction
                    //Missions
                //NOTIFICATIONS

                //General
                List<string> pictureInfoKeysToTranslate = new List<string>();
                List<string> pictureInfoTranslatedTexts = new List<string>();

                List<string> internetCheckInfoKeysToTranslate = new List<string>();
                List<string> internetCheckInfoTranslatedTexts = new List<string>();

                List<string> shopQuestionInfoKeysToTranslate = new List<string>();
                List<string> shopQuestionInfoTranslatedTexts = new List<string>();

                List<string> workerInfoKeysToTranslate = new List<string>();
                List<string> workerInfoTranslatedTexts = new List<string>();

                //Customization
                List<string> customize_OnSelectedAnEquipmentKeysToTranslate = new List<string>();
                List<string> customize_OnSelectedAnEquipmentTranslatedTexts = new List<string>();

                List<string> customize_OnExitProcessKeysToTranslate = new List<string>();
                List<string> customize_OnExitProcessTranslatedTexts = new List<string>();
                //Customization
                //General

                //SKILLS
                foreach (var item in Language.SkillNames)
                {
                    skillNameKeysToTranslate.Add(item.Key);
                }
                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), skillNameKeysToTranslate, (result) =>
                {
                    skillNameTranslatedTexts = result;
                });

                foreach (var item in Language.SkillDescriptions)
                {
                    skillDescKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), skillDescKeysToTranslate, (result) =>
                {
                    skillDescTranslatedTexts = result;
                });

                foreach (var item in Language.SkillEffects)
                {
                    skillEffectKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), skillEffectKeysToTranslate, (result) =>
                {
                    skillEffectTranslatedTexts = result;
                });

                foreach (var item in Language.SkillDefaultStrings)
                {
                    skillDefaultStringKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), skillDefaultStringKeysToTranslate, (result) =>
                {
                    skillDefaultStringTranslatedTexts = result;
                });

                foreach (var item in Language.SkillInfoStrings)
                {
                    skillInfoKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), skillInfoKeysToTranslate, (result) =>
                {
                    skillInfoTranslatedTexts = result;
                });
                //SKILLS

                //ITEM
                foreach (var item in Language.ItemDescriptions)
                {
                    if (item.Key == string.Empty)
                        continue;
                    itemDescKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), itemDescKeysToTranslate, (result) =>
                {
                    itemDescTranslatedTexts = result;
                });
                //ITEM

                //NPCS
                foreach (var item in Language.DialogsMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    dialogKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), dialogKeysToTranslate, (result) =>
                {
                    dialogTranslatedTexts = result;
                });
                //NPCS

                //NPCSCommend
                foreach (var item in Language.CommendEvulations)
                {
                    if (item.Key == string.Empty)
                        continue;
                    npcCommendKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), npcCommendKeysToTranslate, (result) =>
                {
                    npcCommendTranslatedTexts = result;
                });
                //NPCSCommend

                //NOTIFICATIONS
                foreach (var item in Language.NotificationMessages)
                {
                    if (item.Key == string.Empty)
                        continue;
                    notificationKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), notificationKeysToTranslate, (result) =>
                {
                    notificationTranslatedTexts = result;
                });

                    //Missions
                    foreach (var item in Language.MissionHeaderMessages)
                    {
                        if (item.Key == string.Empty)
                            continue;
                        M_HeaderKeysToTranslate.Add(item.Key);
                    }

                    await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_HeaderKeysToTranslate, (result) =>
                    {
                        M_HeaderTranslatedTexts = result;
                    });

                    foreach (var item in Language.MissionDescriptionMessages)
                    {
                        if (item.Key == string.Empty)
                            continue;
                        M_DescKeysToTranslate.Add(item.Key);
                    }

                    await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_DescKeysToTranslate, (result) =>
                    {
                        M_DescTranslatedTexts = result;
                    });

                        //NPCInteraction
                        foreach (var item in Language.MissionNpcInteractionTypeMessages)
                        {
                            if (item.Key == string.Empty)
                                continue;
                            M_NpcInteractionTypeKeysToTranslate.Add(item.Key);
                        }

                        await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_NpcInteractionTypeKeysToTranslate, (result) =>
                        {
                            M_NpcInteractionTypeTranslatedTexts = result;
                        });

                        foreach (var item in Language.MissionNpcInteractionColorTypeMessages)
                        {
                            if (item.Key == string.Empty)
                                continue;
                            M_NpcInteractionColorTypeKeysToTranslate.Add(item.Key);
                        }

                        await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_NpcInteractionColorTypeKeysToTranslate, (result) =>
                        {
                            M_NpcInteractionColorTypeTranslatedTexts = result;
                        });

                        foreach (var item in Language.MissionNpcInteractionColorMessages)
                        {
                            if (item.Key == string.Empty)
                                continue;
                            M_NpcInteractionTargetColorKeysToTranslate.Add(item.Key);
                        }

                        await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_NpcInteractionTargetColorKeysToTranslate, (result) =>
                        {
                            M_NpcInteractionTargetColorTranslatedTexts = result;
                        });

                        foreach (var item in Language.MissionNpcInteractionStateMessages)
                        {
                            if (item.Key == string.Empty)
                                continue;
                            M_NpcInteractionStateKeysToTranslate.Add(item.Key);
                        }

                        await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_NpcInteractionStateKeysToTranslate, (result) =>
                        {
                            M_NpcInteractionStateTranslatedTexts = result;
                        });

                        foreach (var item in Language.MissionNpcInteractionHelperMessages)
                        {
                            if (item.Key == string.Empty)
                                continue;
                            M_NpcInteractionHelperKeysToTranslate.Add(item.Key);
                        }

                        await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), M_NpcInteractionHelperKeysToTranslate, (result) =>
                        {
                            M_NpcInteractionHelperTranslatedTexts = result;
                        });
                        //NPCInteraction
                    //Missions
                //NOTIFICATIONS

                //GENERAL
                foreach (var item in Language.PictureInfoStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    pictureInfoKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), pictureInfoKeysToTranslate, (result) =>
                {
                    pictureInfoTranslatedTexts = result;
                });
                foreach (var item in Language.InternetCheckInfoStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    internetCheckInfoKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), internetCheckInfoKeysToTranslate, (result) =>
                {
                    internetCheckInfoTranslatedTexts = result;
                });

                foreach (var item in Language.ShopQuestionInfoStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    shopQuestionInfoKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), shopQuestionInfoKeysToTranslate, (result) =>
                {
                    shopQuestionInfoTranslatedTexts = result;
                });

                foreach (var item in Language.WorkerInfoStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    workerInfoKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), workerInfoKeysToTranslate, (result) =>
                {
                    workerInfoTranslatedTexts = result;
                });

                //Customization
                foreach (var item in Language.customize_SelectedAnEquipmentStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    customize_OnSelectedAnEquipmentKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), customize_OnSelectedAnEquipmentKeysToTranslate, (result) =>
                {
                    customize_OnSelectedAnEquipmentTranslatedTexts = result;
                });

                foreach (var item in Language.customize_OnExitProcessStrings)
                {
                    if (item.Key == string.Empty)
                        continue;
                    customize_OnExitProcessKeysToTranslate.Add(item.Key);
                }

                await GameManager.instance.BulkTranslateAndAssignAsync(GetEnumDescription(language), customize_OnExitProcessKeysToTranslate, (result) =>
                {
                    customize_OnExitProcessTranslatedTexts = result;
                });
                //Customization
                //GENERAL

                //ItemAdding
                Debug.Log("Language.ItemDescriptions.Count => " + Language.ItemDescriptions.Count + "itemDescTranslatedTexts.Count => " + itemDescTranslatedTexts.Count);
                for (int i = 0; i < itemDescTranslatedTexts.Count; i++)
                {
                    Debug.Log("itemDescTranslatedTexts[i] => " + itemDescTranslatedTexts[i] + " and i => " + i);
                    Language.ItemDescriptions[i].ActiveLanguage = itemDescTranslatedTexts[i];
                }
                //ItemAdding
                //SkillAdding
                Debug.Log("Language.SkillNames.Count => " + Language.SkillNames.Count + "skillNameTranslatedTexts.Count => " + skillNameTranslatedTexts.Count);
                for (int i = 0; i < skillNameTranslatedTexts.Count; i++)
                {
                    Language.SkillNames[i].ActiveLanguage = skillNameTranslatedTexts[i];
                }
                Debug.Log("Language.SkillDescriptions.Count => " + Language.SkillDescriptions.Count + "skillDescTranslatedTexts.Count => " + skillDescTranslatedTexts.Count);
                for (int i = 0; i < skillDescTranslatedTexts.Count; i++)
                {
                    if (i >= Language.SkillDescriptions.Count) break;
                    Language.SkillDescriptions[i].ActiveLanguage = skillDescTranslatedTexts[i];
                }
                Debug.Log("Language.SkillEffects.Count => " + Language.SkillEffects.Count + "skillEffectTranslatedTexts.Count => " + skillEffectTranslatedTexts.Count);
                for (int i = 0; i < skillEffectTranslatedTexts.Count; i++)
                {
                    Language.SkillEffects[i].ActiveLanguage = skillEffectTranslatedTexts[i];
                }
                Debug.Log("Language.SkillDefaultStrings.Count => " + Language.SkillDefaultStrings.Count + "skillDefaultStringTranslatedTexts.Count => " + skillDefaultStringTranslatedTexts.Count);
                for (int i = 0; i < skillDefaultStringTranslatedTexts.Count; i++)
                {
                    Language.SkillDefaultStrings[i].ActiveLanguage = skillDefaultStringTranslatedTexts[i];
                }
                Debug.Log("Language.SkillInfoStrings.Count => " + Language.SkillInfoStrings.Count + "skillInfoTranslatedTexts.Count => " + skillInfoTranslatedTexts.Count);
                for (int i = 0; i < skillInfoTranslatedTexts.Count; i++)
                {
                    Language.SkillInfoStrings[i].ActiveLanguage = skillInfoTranslatedTexts[i];
                }
                //SkillAdding

                //NPCCommendAdding
                Debug.Log("Language.CommendEvulations.Count => " + Language.CommendEvulations.Count + "npcCommendTranslatedTexts.Count => " + npcCommendTranslatedTexts.Count);
                for (int i = 0; i < npcCommendTranslatedTexts.Count; i++)
                {
                    Language.CommendEvulations[i].ActiveLanguage = npcCommendTranslatedTexts[i];
                }
                //NPCCommendAdding

                //NPCSAdding
                Debug.Log("Language.DialogsMessages.Count => " + Language.DialogsMessages.Count + "dialogTranslatedTexts.Count => " + dialogTranslatedTexts.Count);
                for (int i = 0; i < dialogTranslatedTexts.Count; i++)
                {
                    Language.DialogsMessages[i].ActiveLanguage = dialogTranslatedTexts[i];
                }
                //NPCSAdding

                //NotificationAdding
                Debug.Log("Language.NotificationMessages.Count => " + Language.NotificationMessages.Count + "notificationTranslatedTexts.Count => " + notificationTranslatedTexts.Count);
                for (int i = 0; i < notificationTranslatedTexts.Count; i++)
                {
                    Language.NotificationMessages[i].ActiveLanguage = notificationTranslatedTexts[i];
                }

                    //MissionsAdding
                    Debug.Log("Language.MissionHeaderMessages.Count => " + Language.MissionHeaderMessages.Count + " M_HeaderTranslatedTexts.Count => " + M_HeaderTranslatedTexts.Count);
                    for (int i = 0; i < M_HeaderTranslatedTexts.Count; i++)
                    {
                        Language.MissionHeaderMessages[i].ActiveLanguage = M_HeaderTranslatedTexts[i];
                    }

                    Debug.Log("Language.MissionDescriptionMessages.Count => " + Language.MissionDescriptionMessages.Count + " M_DescTranslatedTexts.Count => " + M_DescTranslatedTexts.Count);
                    for (int i = 0; i < M_DescTranslatedTexts.Count; i++)
                    {
                        Language.MissionDescriptionMessages[i].ActiveLanguage = M_DescTranslatedTexts[i];
                    }

                        //NPCInteractionAdding
                        Debug.Log("Language.MissionNpcInteractionTypeMessages.Count => " + Language.MissionNpcInteractionTypeMessages.Count + " M_NpcInteractionTypeTranslatedTexts.Count => " + M_NpcInteractionTypeTranslatedTexts.Count);
                        for (int i = 0; i < M_NpcInteractionTypeTranslatedTexts.Count; i++)
                        {
                            Language.MissionNpcInteractionTypeMessages[i].ActiveLanguage = M_NpcInteractionTypeTranslatedTexts[i];
                        }

                        Debug.Log("Language.MissionNpcInteractionColorTypeMessages.Count => " + Language.MissionNpcInteractionColorTypeMessages.Count + " M_NpcInteractionColorTypeTranslatedTexts.Count => " + M_NpcInteractionColorTypeTranslatedTexts.Count);
                        for (int i = 0; i < M_NpcInteractionColorTypeTranslatedTexts.Count; i++)
                        {
                            Language.MissionNpcInteractionColorTypeMessages[i].ActiveLanguage = M_NpcInteractionColorTypeTranslatedTexts[i];
                        }

                        Debug.Log("Language.MissionNpcInteractionColorMessages.Count => " + Language.MissionNpcInteractionColorMessages.Count + " M_NpcInteractionTargetColorTranslatedTexts.Count => " + M_NpcInteractionTargetColorTranslatedTexts.Count);
                        for (int i = 0; i < M_NpcInteractionTargetColorTranslatedTexts.Count; i++)
                        {
                            Language.MissionNpcInteractionColorMessages[i].ActiveLanguage = M_NpcInteractionTargetColorTranslatedTexts[i];
                        }

                        Debug.Log("Language.MissionNpcInteractionStateMessages.Count => " + Language.MissionNpcInteractionStateMessages.Count + " M_NpcInteractionStateTranslatedTexts.Count => " + M_NpcInteractionStateTranslatedTexts.Count);
                        for (int i = 0; i < M_NpcInteractionStateTranslatedTexts.Count; i++)
                        {
                            Language.MissionNpcInteractionStateMessages[i].ActiveLanguage = M_NpcInteractionStateTranslatedTexts[i];
                        }

                        Debug.Log("Language.MissionNpcInteractionHelperMessages.Count => " + Language.MissionNpcInteractionHelperMessages.Count + " M_NpcInteractionHelperTranslatedTexts.Count => " + M_NpcInteractionHelperTranslatedTexts.Count);
                        for (int i = 0; i < M_NpcInteractionHelperTranslatedTexts.Count; i++)
                        {
                            Language.MissionNpcInteractionHelperMessages[i].ActiveLanguage = M_NpcInteractionHelperTranslatedTexts[i];
                        }

                        //NPCInteractionAdding
                    //MissionsAdding
                //NotificationAdding

                //GeneralAdding
                Debug.Log("Language.PictureInfoStrings.Count => " + Language.PictureInfoStrings.Count + "pictureInfoTranslatedTexts.Count => " + pictureInfoTranslatedTexts.Count);
                for (int i = 0; i < pictureInfoTranslatedTexts.Count; i++)
                {
                    Language.PictureInfoStrings[i].ActiveLanguage = pictureInfoTranslatedTexts[i];
                }

                Debug.Log("Language.InternetCheckInfoStrings.Count => " + Language.InternetCheckInfoStrings.Count + "internetCheckInfoTranslatedTexts.Count => " + internetCheckInfoTranslatedTexts.Count);
                for (int i = 0; i < internetCheckInfoTranslatedTexts.Count; i++)
                {
                    Language.InternetCheckInfoStrings[i].ActiveLanguage = internetCheckInfoTranslatedTexts[i];
                }

                Debug.Log("Language.ShopQuestionInfoStrings.Count => " + Language.ShopQuestionInfoStrings.Count + "shopQuestionInfoTranslatedTexts.Count => " + shopQuestionInfoTranslatedTexts.Count);
                for (int i = 0; i < shopQuestionInfoTranslatedTexts.Count; i++)
                {
                    Language.ShopQuestionInfoStrings[i].ActiveLanguage = shopQuestionInfoTranslatedTexts[i];
                }

                Debug.Log("Language.WorkerInfoStrings.Count => " + Language.WorkerInfoStrings.Count + "workerInfoTranslatedTexts.Count => " + workerInfoTranslatedTexts.Count);
                for (int i = 0; i < workerInfoTranslatedTexts.Count; i++)
                {
                    Language.WorkerInfoStrings[i].ActiveLanguage = workerInfoTranslatedTexts[i];
                }
                //Customization
                Debug.Log("Language.customize_SelectedAnEquipmentStrings.Count => " + Language.customize_SelectedAnEquipmentStrings.Count + "customize_OnSelectedAnEquipmentTranslatedTexts.Count => " + customize_OnSelectedAnEquipmentTranslatedTexts.Count);
                for (int i = 0; i < customize_OnSelectedAnEquipmentTranslatedTexts.Count; i++)
                {
                    Language.customize_SelectedAnEquipmentStrings[i].ActiveLanguage = customize_OnSelectedAnEquipmentTranslatedTexts[i];
                }

                Debug.Log("Language.customize_OnExitProcessStrings.Count => " + Language.customize_OnExitProcessStrings.Count + "customize_OnExitProcessTranslatedTexts.Count => " + customize_OnExitProcessTranslatedTexts.Count);
                for (int i = 0; i < customize_OnExitProcessTranslatedTexts.Count; i++)
                {
                    Language.customize_OnExitProcessStrings[i].ActiveLanguage = customize_OnExitProcessTranslatedTexts[i];
                }
                //Customization
                //GeneralAdding
                // JSON dosyasýný kaydet
                Debug.LogWarning("Language Localization Test 1");
                string jsonString = JsonUtility.ToJson(Language);
                string directoryPath = "Assets/Resources/Languages"; // Resources klasörü içindeki Languages klasörü
                string filePath = Path.Combine(directoryPath, $"Language_{GameManager.instance.GetLanguageShortString(GetEnumDescription(language))}.json");
                Debug.LogWarning("Language Localization Test 2");
                // Dosya yolu var mý kontrol et, yoksa oluþtur
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                Debug.LogWarning("Language Localization Test 3");
                // Dosyayý oluþtur ve içine yaz
                File.WriteAllText(filePath, jsonString);
                Debug.LogWarning("Language Localization Test 4");
            }


            Debug.Log("Çeviri ve kayýt iþlemi tamamlandý.");
            waitBeforeTranslate = false;
        }
        catch (Exception _ex)
        {
            Console.WriteLine("Language Database ceviri islamleri sirasinda bir sorunla karsilasildi:\n"+_ex.Message);
        }
        
    }
    private string GetEnumDescription(Languages value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }

    public enum Languages
    {
        English,
        Turkish,
        Thai,
        Spanish,
        [Description("Chinese (Traditional)")]
        TW_Chinese,
        [Description("Chinese (Simplified)")]
        CN_Chinese,
        Kurdish,
        Russian,
        German,
        French,
        Japanese,
        Korean,
        Arabic,
    }
}

[System.Serializable]
public class MainLanguageData
{
    //Items
    public List<LanguageData> ItemDescriptions = new List<LanguageData>();
    //Items
    //Skills
    public List<LanguageData> SkillNames = new List<LanguageData>();
    public List<LanguageData> SkillDescriptions = new List<LanguageData>();
    public List<LanguageData> SkillEffects = new List<LanguageData>();
    public List<LanguageData> SkillDefaultStrings = new List<LanguageData>();
    public List<LanguageData> SkillInfoStrings = new List<LanguageData>();
    //Skills
    //NPCCommend
    public List<LanguageData> CommendEvulations = new List<LanguageData>();
    //NPCCommend
    //Notifications
    public List<LanguageData> NotificationMessages = new List<LanguageData>();
        //Notification-Reward
        public List<LanguageData> NotificationRewardMessages = new List<LanguageData>();
        //Notification-Reward
    //Notifications
    //Missions
    public List<LanguageData> MissionHeaderMessages = new List<LanguageData>();
    public List<LanguageData> MissionDescriptionMessages = new List<LanguageData>();
        //Mission-Collections
        public List<LanguageData> MissionNpcInteractionHelperMessages = new List<LanguageData>();
        public List<LanguageData> MissionNpcInteractionColorTypeMessages = new List<LanguageData>();
        public List<LanguageData> MissionNpcInteractionColorMessages = new List<LanguageData>();
        public List<LanguageData> MissionNpcInteractionStateMessages = new List<LanguageData>();
        public List<LanguageData> MissionNpcInteractionTypeMessages = new List<LanguageData>();
        //Mission-Collections
    //Missions
    //Dialogs
    public List<LanguageData> DialogsMessages = new List<LanguageData>();
    //Dialogs
    //General
    public List<LanguageData> PictureInfoStrings = new List<LanguageData>();
    public List<LanguageData> InternetCheckInfoStrings = new List<LanguageData>();
    public List<LanguageData> ShopQuestionInfoStrings = new List<LanguageData>();
    public List<LanguageData> WorkerInfoStrings = new List<LanguageData>();
    //Customization
    public List<LanguageData> customize_SelectedAnEquipmentStrings;
    public List<LanguageData> customize_OnExitProcessStrings;
    //Customization
    //General
}

[System.Serializable]
public class LanguageData
{
    public int TargetID;
    public string Key;
    public string ActiveLanguage;
    public LanguageData(int _targetID, string _key)
    {
        TargetID = _targetID;
        Key = _key;
    }
}
