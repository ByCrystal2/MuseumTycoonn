//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class LanguageHandler : MonoBehaviour
//{
//    public static LanguageHandler instance { get; private set; }

//    public List<TMPTextAndHeaderCombine> TextMeshTexts;
//    public List<TextAndHeaderCombine> TextTexts;

//    public List<string> TextMeshStrings = new List<string>();
//    public List<string> TextStrings = new List<string>();
//    public bool RemoveNumbers;
//    // Start is called before the first frame update
//    void Awake()
//    {
//        if (instance)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        instance = this;
//        RefreshAllTexts();
//    }

//    public void RefreshAllTexts()
//    {
//        int length = TextMeshTexts.Count;
//        for (int i = 0; i < length; i++)
//            TextMeshTexts[i].Text.text = LanguageDatabase.instance.GetTextWithID(TextMeshTexts[i]._Header);

//        length = TextTexts.Count;
//        for (int i = 0; i < length; i++)
//            TextTexts[i].Text.text = LanguageDatabase.instance.GetTextWithID(TextTexts[i]._Header);
//    }

//    private void OnDrawGizmosSelected()
//    {
//        if (RemoveNumbers)
//        {
//            TextMeshTexts.Clear();
//            TextTexts.Clear();
//            TextMeshProUGUI[] textMeshPros = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
//            foreach (var item in textMeshPros)
//            {
//                TMPTextAndHeaderCombine t = new TMPTextAndHeaderCombine();
//                t.Text = item;
//                TextMeshTexts.Add(t);
//            }

//            Text[] textPros = Resources.FindObjectsOfTypeAll<Text>();
//            foreach (var item in textPros)
//            {
//                TextAndHeaderCombine t = new TextAndHeaderCombine();
//                t.Text = item;
//                TextTexts.Add(t);
//            }

//            RemoveNumbers = false;
//            int length = TextMeshTexts.Count;
//            for (int i = length - 1; i >= 0; i--)
//            {
//                if (int.TryParse(TextMeshTexts[i].Text.text, out int result))
//                    TextMeshTexts.RemoveAt(i);
//                else if (TextMeshTexts[i] == null)
//                    TextMeshTexts.RemoveAt(i);
//                else if (TextMeshTexts[i].Text.text.Contains("/"))
//                    TextMeshTexts.RemoveAt(i);

//                TextMeshTexts[i]._Header = TextMeshTexts[i].Text.name + "_" + TextMeshTexts[i].Text.text;
//                if (!TextMeshStrings.Contains(TextMeshTexts[i]._Header))
//                    TextMeshStrings.Add(TextMeshTexts[i]._Header);
//            }

//            length = TextTexts.Count;
//            for (int i = length - 1; i >= 0; i--)
//            {
//                if (int.TryParse(TextTexts[i].Text.text, out int result))
//                    TextTexts.RemoveAt(i);
//                else if (TextTexts[i] == null)
//                    TextTexts.RemoveAt(i);
//                else if (TextTexts[i].Text.text.Contains("/"))
//                    TextTexts.RemoveAt(i);

//                TextTexts[i]._Header = TextTexts[i].Text.name + "_" + TextTexts[i].Text.text;

//                if (!TextStrings.Contains(TextTexts[i]._Header))
//                    TextStrings.Add(TextTexts[i]._Header);
//            }

//            HeadersForLanguage h = new HeadersForLanguage();
//            h.AllHeaders = "";
//            foreach (var item in TextMeshStrings)
//                h.AllHeaders += item + Environment.NewLine;

//            foreach (var item in TextStrings)
//                h.AllHeaders += item + Environment.NewLine;

//            string filePath = "Assets/Resources/HeaderList.txt";

//            using (StreamWriter writer = new StreamWriter(filePath))
//                writer.Write(h.AllHeaders);
            
//            //UnityEditor.AssetDatabase.Refresh(); => build error veriyor bundan dolayi
//        }
//    }

//    [System.Serializable]
//    public class HeadersForLanguage
//    {
//        public string AllHeaders;
//    }

//    [System.Serializable]
//    public class TMPTextAndHeaderCombine
//    {
//        public TextMeshProUGUI Text;
//        public string _Header;
//    }

//    [System.Serializable]
//    public class TextAndHeaderCombine
//    {
//        public Text Text;
//        public string _Header;
//    }
//}
