using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LanguageDatabase : MonoBehaviour
{
    public static LanguageDatabase instance { get; private set; }

    public Dictionary<string, string> texts = new Dictionary<string, string>();

    public eLanguage currentActiveLanguage;
    public void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        currentActiveLanguage = eLanguage.Thai;
        InstallLanguage();
        DontDestroyOnLoad(gameObject);
    }

    private void InstallLanguage()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("MuseumLanguageDatas");
        Debug.Log("jsonFile.text: " + jsonFile.text);
        LanguageDataCore data = JsonUtility.FromJson<LanguageDataCore>(jsonFile.text);

        texts.Clear();
        int length = data.LanguageDataList.Count;
        switch (currentActiveLanguage)
        {
            case eLanguage.English:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].en);
                break;
            case eLanguage.Turkish:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].tr);
                break;
            case eLanguage.Thai:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].th);
                break;
            case eLanguage.Spanish:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].es);
                break;
            case eLanguage.ChineseTraditional:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].zh_TW);
                break;
            case eLanguage.ChineseSimplified:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].zh_CH);
                break;
            case eLanguage.Russia:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].ru);
                break;
            case eLanguage.Deutch:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].de);
                break;
            case eLanguage.French:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].fr);
                break;
            case eLanguage.Japanese:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].ja);
                break;
            case eLanguage.Korean:
                for (int i = 0; i < length; i++)
                    SetTextToCorrectLanguage(data.LanguageDataList[i].header, data.LanguageDataList[i].ko);
                break;
            default:
                break;
        }
    }

    private void SetTextToCorrectLanguage(string _header, string _text)
    {
        Debug.Log("currentLanguage: " + currentActiveLanguage.ToString() + " / _header: " + _header + " / _text: " + _text);
        texts.Add(_header, _text);
    }

    public string GetTextWithID(string _header)
    {
        if (texts.ContainsKey(_header))
            return texts[_header];
        else
            return "Null";
    }

    [System.Serializable]
    public class LanguageData
    {
        public string header;
        public string category;
        public string en;
        public string tr;
        public string th;
        public string es;
        public string zh_TW;
        public string zh_CH;
        public string ru;
        public string de;
        public string fr;
        public string ja;
        public string ko;
    }

    [System.Serializable]
    public class LanguageDataCore
    {
        public List<LanguageData> LanguageDataList;
    }

    public enum eLanguage
    {
        English,
        Turkish,
        Thai,
        Spanish,
        ChineseTraditional,
        ChineseSimplified,
        Russia,
        Deutch,
        French,
        Japanese,
        Korean,
    }
}
