using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        //General
        Language.PictureInfoStrings = new List<LanguageData>() { new LanguageData(0, "Added"), new LanguageData(1, "Updated"), new LanguageData(2, "Select Painting"), new LanguageData(3, "Enough"), new LanguageData(4, "Add"), new LanguageData(5, "Update"), new LanguageData(6, "Insufficient") };

        Language.InternetCheckInfoStrings = new List<LanguageData>() { new LanguageData(0, "Internet Connection"), new LanguageData(1, "Your internet connection has been lost."), new LanguageData(2, "Please check your connection."), new LanguageData(3, "Your internet connection has been restored.") };

        Language.ShopQuestionInfoStrings = new List<LanguageData>() { new LanguageData(0, "Purchase Process"), new LanguageData(1, "Do you confirm the purchase?"), new LanguageData(2, "Available"), new LanguageData(3, "Gem"), new LanguageData(4, "Gold"), new LanguageData(5, "Product Price"), new LanguageData(6, "Paid") };
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

        //NPCCOMMENDS
        List<string> npcCommendKeysToTranslate = new List<string>();
        List<string> npcCommendTranslatedTexts = new List<string>();
        //NPCCOMMENDS

        //General
        List<string> pictureInfoKeysToTranslate = new List<string>();
        List<string> pictureInfoTranslatedTexts = new List<string>();

        List<string> internetCheckInfoKeysToTranslate = new List<string>();
        List<string> internetCheckInfoTranslatedTexts = new List<string>();

        List<string> shopQuestionInfoKeysToTranslate = new List<string>();
        List<string> shopQuestionInfoTranslatedTexts = new List<string>();
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

        //NPCCOMMEND
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
        //NPCCOMMEND

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

            //NPCCOMMENDS
            List<string> npcCommendKeysToTranslate = new List<string>();
            List<string> npcCommendTranslatedTexts = new List<string>();
            //NPCCOMMENDS

            //General
            List<string> pictureInfoKeysToTranslate = new List<string>();
            List<string> pictureInfoTranslatedTexts = new List<string>();

            List<string> internetCheckInfoKeysToTranslate = new List<string>();
            List<string> internetCheckInfoTranslatedTexts = new List<string>();

            List<string> shopQuestionInfoKeysToTranslate = new List<string>();
            List<string> shopQuestionInfoTranslatedTexts = new List<string>();
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

            //NPCCOMMEND
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
            //NPCCOMMEND

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
            //GeneralAdding
            // JSON dosyasýný kaydet
            string jsonString = JsonUtility.ToJson(Language);
            string directoryPath = "Assets/Resources/Languages"; // Resources klasörü içindeki Languages klasörü
            string filePath = Path.Combine(directoryPath, $"Language_{GameManager.instance.GetLanguageShortString(GetEnumDescription(language))}.json");

            // Dosya yolu var mý kontrol et, yoksa oluþtur
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Dosyayý oluþtur ve içine yaz
            File.WriteAllText(filePath, jsonString);
        }


        Debug.Log("Çeviri ve kayýt iþlemi tamamlandý.");
        waitBeforeTranslate = false;
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
    //General
    public List<LanguageData> PictureInfoStrings = new List<LanguageData>();
    public List<LanguageData> InternetCheckInfoStrings = new List<LanguageData>();
    public List<LanguageData> ShopQuestionInfoStrings = new List<LanguageData>();
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
