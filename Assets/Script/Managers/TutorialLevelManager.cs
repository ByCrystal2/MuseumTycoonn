using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevelManager : MonoBehaviour
{
    //[SerializeField] AnimationClip[] RandomAnimations;
    public List<(List<string> messages, int id)> DialogsMessages = new List<(List<string> message, int id)>();
    public static TutorialLevelManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        DialogsInit();
        //Camera.main.gameObject.SetActive(false);
    }

    private void Start()
    {
        
    }
    //public AnimationClip GetRandomAnim()
    //{
    //    int index = Random.Range(0, RandomAnimations.Length);
    //    return RandomAnimations[index];
    //}
    public void DialogsInit()
    {
        DialogsMessages = new List<(List<string> message, int id)>
        {
            (new List<string>()
            {
                "Merhaba! Krall���ma ho� geldin! Seni burada g�rmekten �ok mutluyum.",
                "Krall���m�z�n en de�erli k�lt�r merkezlerinden biri olan m�zemiz i�in seni y�netici olarak se�tik.",
                "Buna haz�r ve kendinden emin g�r�n�yorsun. M�zeyi �ok iyi bir �ekilde geli�tirece�ine inan�yoruz.",
                "�ncesinde biraz yard�ma ihtiya� duyabilirsin.",
                "Beni takip et.",
            },0),
            (new List<string>()
            {
                "�ncelikle, sergi i�in bir odaya ihtiyac�n var.",
                "Belirtilen yere dokunarak, M�ze D�zenleme Modu'na ge�men gerek.",
            },1),
            (new List<string>()
            {
                "M�ze i�inde bir �ok oda mevcut.",
                "Ancak, bu odalara sahip olmak i�in bir miktar alt�n �demelisin.",
                "Odan�n �zerine dokunarak sat�n alma i�lemi yapabilirsin.",
            },2),
            (new List<string>()
            {
                "Harika! Art�k bir odaya sahipsin.",
                "Hadi odaya bir g�z atal�m ve neler yapabilece�imize bakal�m!",
            },4),
            (new List<string>()
            {
                "Oda D�zenleme Modu'na ge�i� yapt�n.",
                "Bu sayede odalara daha iyi m�dahele edebilirsin.",
                "�imdi ise, belirtilen yere dokunarak g�rsel ��eleri gizleyebilirsin.",
            },5),
            (new List<string>()
            {
                "Depoda eski bir tablo bulunuyor.",
                "Hadi onu bu odadaki bir duvara yerle�tirelim.",
            },6),
            (new List<string>()
            {
                "Odan�n i�ine yerle�tirmek i�in �nce tabloyu se�men ve \"EKLE'men\" gerek.",
            },7),
            (new List<string>()
            {
                "Harika! �lk tablonu yerle�tirdin.",
                "S�rada ki ad�ma ge�elim.",
                "Geli�men i�in sana yard�m etmek istiyoruz.",
                "G�nl�k olarak baz� �d�ller verece�iz.",
                "Hadi ilk �d�l�n� al.",
            },9),
            (new List<string>()
            {
                "Her g�n buray� kontrol etmeyi unutma.",
                "M�zeyi geli�tirmek i�in daha fazla tabloya ihtiyac�n var.",
                "�lk sat�n alaca��n tablo benden sana hediye.",
                "Hadi onu alal�m!",
            },11),
            (new List<string>()
            {
                "Harika! �lk tablonu sat�n ald�n.",
                "�imdi ise temel yetenekleri alal�m.",
            },18),
            (new List<string>()
            {
                "Tebrikler! Temel skilleri ald�n!",
                "M�zen her level atlad���nda 1 yetenek puan�na sahip olursun.",
                "Buraya gelip onlar� de�erlendirebilirsin.",
                "Fakat �unuda unutma, yetenek sat�n almada ve seviye artt�rmada alt�nda �demen gerekir.",
                "�imdi ise m�zenin genel statlar�n�n bulundu�u k�sma ge�elim.",
            },24),
            (new List<string>()
            {
                "Sol k�s�m, tablolar hakk�nda ziyaret�i yorumlar�n�n bulundu yer.",
                "Sa� k�s�m ise, m�zenin mevcut istatistiklerini g�steren yer.",
                "�imdi s�radaki ad�ma ge�elim.",
            },25),
            (new List<string>()
            {
                "Bir heykel sat�n alal�m ve onu yerle�tirelim.",
            },26),
            (new List<string>()
            {
                "Unutma, heykeller; Bulundu�u odaya giren ziyaret�ilere baz� �zellikler kazand�rabilir!",
                "Odaya giren npcleri mutlu etme �zelli�ine sahip bir heykel sat�n alal�m!",
            },28),
            (new List<string>()
            {
                "Ziyaret�iler bazen etraf� kirletebiliyorlar...",
                "Bundan dolay� bir temizlik�i i�e alal�m.",
            },31),
            (new List<string>()
            {
                "Tebrikler! Bir temizlik�i i�e ald�n.",
                "�imdi ise s�rada ki ad�ma ge�elim.",
            },33),
            (new List<string>()
            {
                "Bu panelde, i��ileri �al��malar� i�in odalara y�nlendiriyoruz.",
                "�imdi ise bir i�e ald���m�z i��iyi bir odaya atayal�m.",
            },35),
            (new List<string>()
            {
                "Tebrikler! ��renim k�sm�n� tamamlad�n. Art�k haz�rs�n!",
                "Seni arada tefti� etmeye gelece�im. Her daim haz�r ol.",
            },39),
        };
    }
    public void OnEndFlyCutscene(Camera targetCamera)
    {
        if (AudioManager.instance != null)
        AudioManager.instance.TrailerSource.Stop();
        targetCamera.gameObject.SetActive(true);
        DialogueTrigger kingTrigger = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>();
        kingTrigger.TriggerDialog(Steps.Step1);
        StartCoroutine(WaitingForAnim(kingTrigger));
    }
    IEnumerator WaitingForAnim(DialogueTrigger kingTrigger)
    {
        yield return new WaitForSeconds(0.5f);
        AudioManager.instance.PlayMusicOfTutorial();
        kingTrigger.GetComponent<TutorialNPCBehaviour>().ProcessAnim("Sit", false);
    }
}
