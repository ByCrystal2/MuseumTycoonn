using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EditorUserHandler : MonoBehaviour
{
    [SerializeField] Text name_Text;
    [SerializeField] Text iD_Text;
    [SerializeField] Text eMail_Text;
    [SerializeField] Text phone_Text;
    [SerializeField] Button login_Button;
    //DatabaseUser user;
    private void Start()
    {
        login_Button.onClick.RemoveAllListeners();
        login_Button.onClick.AddListener(LoginProcess);
    }
    //public void InitEditorUser(DatabaseUser _user)
    //{
    //    name_Text.text = _user.Name;
    //    iD_Text.text = _user.UserID;
    //    eMail_Text.text = _user.Email;
    //    phone_Text.text = _user.PhoneNumber;
    //    user = new DatabaseUser(_user.Name, _user.Email, _user.PhoneNumber, _user.UserID);
    //}
    void LoginProcess()
    {
        //Data json'dan cekilmeli (editor giris islemleri!!)
        UIInteractHandler.instance.AskQuestion("Login", $"Do you want to log in with the account name {name_Text.text}?", (x) => {
            //UIInteractHandler.instance.AskQuestion("")
            //StartCoroutine(FirestoreManager.instance.CheckIfUserExists(user.UserID, user.Email, user.PhoneNumber, user.Name));

            if (EditorUserSignCanvasController.instance.IsRememberMe())
            {
                //string jsonData = JsonUtility.ToJson(user, true);
                string folderPath = Path.Combine(Application.persistentDataPath, "Admin");
                string filePath = Path.Combine(folderPath, "LoggedIn_EditorUser.json");

                // Klas�r�n var olup olmad���n� kontrol ediyoruz, yoksa olu�turuyoruz
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Debug.Log("Klas�r olu�turuldu: " + folderPath);
                }

                // Dosya yoksa dosyay� olu�turuyoruz
                if (!File.Exists(filePath))
                {
                    Debug.Log("Dosya mevcut de�il, olu�turuluyor...");
                    //File.WriteAllText(filePath, jsonData);
                    Debug.Log("Veri kaydedildi: " + filePath);
                }
                else
                {
                    Debug.Log("File exists.");
                }
            }        
            //FirebaseAuthManager.instance.CreateNewLoading();
        });
    }
}
