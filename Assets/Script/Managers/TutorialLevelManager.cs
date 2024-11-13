using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevelManager : MonoBehaviour
{
    //[SerializeField] AnimationClip[] RandomAnimations;
    public List<DialogTranslationHelper> DialogsMessages = new List<DialogTranslationHelper>();
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
        DialogsMessages = new List<DialogTranslationHelper>
    {
        new DialogTranslationHelper(new List<string>()
        {
            "Merhaba! Krallýðýma hoþ geldin! Seni burada görmekten çok mutluyum.",
            "Krallýðýmýzýn en deðerli kültür merkezlerinden biri olan müzemiz için seni yönetici olarak seçtik.",
            "Buna hazýr ve kendinden emin görünüyorsun. Müzeyi çok iyi bir þekilde geliþtireceðine inanýyoruz.",
            "Öncesinde biraz yardýma ihtiyaç duyabilirsin.",
            "Beni takip et.",
        }, 0),

        new DialogTranslationHelper(new List<string>()
        {
            "Öncelikle, sergi için bir odaya ihtiyacýn var.",
            "Belirtilen yere dokunarak, Müze Düzenleme Modu'na geçmen gerek.",
        }, 1),

        new DialogTranslationHelper(new List<string>()
        {
            "Müze içinde birçok oda mevcut.",
            "Ancak, bu odalara sahip olmak için bir miktar altýn ödemelisin.",
            "Odanýn üzerine dokunarak satýn alma iþlemi yapabilirsin.",
        }, 2),

        new DialogTranslationHelper(new List<string>()
        {
            "Harika! Artýk bir odaya sahipsin.",
            "Hadi odaya bir göz atalým ve neler yapabileceðimize bakalým!",
        }, 4),

        new DialogTranslationHelper(new List<string>()
        {
            "Oda Düzenleme Modu'na geçiþ yaptýn.",
            "Bu sayede odalara daha iyi müdahale edebilirsin.",
            "Þimdi ise, belirtilen yere dokunarak görsel öðeleri gizleyebilirsin.",
        }, 5),

        new DialogTranslationHelper(new List<string>()
        {
            "Depoda eski bir tablo bulunuyor.",
            "Hadi onu bu odadaki bir duvara yerleþtirelim.",
        }, 6),

        new DialogTranslationHelper(new List<string>()
        {
            "Odanýn içine yerleþtirmek için önce tabloyu seçmen ve \"EKLE'men\" gerek.",
        }, 7),

        new DialogTranslationHelper(new List<string>()
        {
            "Harika! Ýlk tablonu yerleþtirdin.",
            "Sýrada ki adýma geçelim.",
            "Geliþmen için sana yardým etmek istiyoruz.",
            "Günlük olarak bazý ödüller vereceðiz.",
            "Hadi ilk ödülünü al.",
        }, 9),

        new DialogTranslationHelper(new List<string>()
        {
            "Her gün burayý kontrol etmeyi unutma.",
            "Müzeyi geliþtirmek için daha fazla tabloya ihtiyacýn var.",
            "Ýlk satýn alacaðýn tablo benden sana hediye.",
            "Hadi onu alalým!",
        }, 11),

        new DialogTranslationHelper(new List<string>()
        {
            "Harika! Ýlk tablonu satýn aldýn.",
            "Þimdi ise temel yetenekleri alalým.",
        }, 18),

        new DialogTranslationHelper(new List<string>()
        {
            "Tebrikler! Temel skilleri aldýn!",
            "Müzen her level atladýðýnda 1 yetenek puanýna sahip olursun.",
            "Buraya gelip onlarý deðerlendirebilirsin.",
            "Fakat þunu da unutma, yetenek satýn almada ve seviye arttýrmada altýnda ödemen gerekir.",
            "Þimdi ise müzenin genel statlarýnýn bulunduðu kýsma geçelim.",
        }, 24),

        new DialogTranslationHelper(new List<string>()
        {
            "Sol kýsým, tablolar hakkýnda ziyaretçi yorumlarýnýn bulunduðu yer.",
            "Sað kýsým ise, müzenin mevcut istatistiklerini gösteren yer.",
            "Þimdi sýradaki adýma geçelim.",
        }, 25),

        new DialogTranslationHelper(new List<string>()
        {
            "Bir heykel satýn alalým ve onu yerleþtirelim.",
        }, 26),

        new DialogTranslationHelper(new List<string>()
        {
            "Unutma, heykeller; Bulunduðu odaya giren ziyaretçilere bazý özellikler kazandýrabilir!",
            "Odaya giren NPC'leri mutlu etme özelliðine sahip bir heykel satýn alalým!",
        }, 28),

        new DialogTranslationHelper(new List<string>()
        {
            "Ziyaretçiler bazen etrafý kirletebiliyorlar...",
            "Bundan dolayý bir temizlikçi iþe alalým.",
        }, 31),

        new DialogTranslationHelper(new List<string>()
        {
            "Tebrikler! Bir temizlikçi iþe aldýn.",
            "Þimdi ise sýradaki adýma geçelim.",
        }, 34),

        new DialogTranslationHelper(new List<string>()
        {
            "Bu panelde, iþçileri çalýþmalarý için odalara yönlendiriyoruz.",
            "Þimdi ise iþe aldýðýmýz iþçiyi bir odaya atayalým.",
        }, 36),

        new DialogTranslationHelper(new List<string>()
        {
            "Tebrikler! Öðrenim kýsmýný tamamladýn. Artýk hazýrsýn!",
            "Seni arada teftiþ etmeye geleceðim. Her daim hazýr ol.",
        }, 40),
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
    public struct DialogTranslationHelper
    {
        public List<string> messages;
        public int ID;

        public DialogTranslationHelper(List<string> _messages, int _id)
        {
            this.messages = _messages;
            this.ID = _id;
        }
    }
}
