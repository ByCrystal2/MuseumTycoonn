using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInteractionOrnek : MonoBehaviour
{
    //Ornek-1, Satin alma islemi
    public void TabloSatinAl()
    {
        int myGold = 100;
        int picturePrice = 75;
        if (picturePrice > myGold)
        {
            //Burada dil islemi yapman gerek, oraya gonderecegin textin mevcut dilde olabilmesi icin
            string header = "Yetersiz altin!";
            string explanation = "Bu ogeyi alabilmek icin yeterli altina sahip degilsiniz.";
            UIInteractHandler.instance.AskQuestion(header, explanation, null, null, null, null, null, null);
            //Kullaniciya sadece bu uyari gosterilir ve tum degerler null gonderilince sadece tamam buttonu gorunur. Bu buttonin su anki tek gorevi tiklandiginda paneli kapatmaktir.
        }
        else
        {
            string header = "Tablo Sayin Alma Islemi";
            string explanation = "Bir tabloyu satin almak uzeresiniz. Fiyati {%price} altindir. Bu islemi onayliyor musunuz?";
            explanation = explanation.Replace("{%price}", picturePrice.ToString("F2")); // Bu yontemle baska dillerde paranin gelmesi gereken dogru konumu koruyabilirsin. 
            UIInteractHandler.instance.AskQuestion(header, explanation, TabloSatinAlmaOnaylandi, null, null, new object[] { myGold, picturePrice }, null, null);
            //YesAction kismina TabloSatinAlmaOnaylandi fonksiyonunu gonderdir, eger oyuncu acilan panelde yes e basarsa bu fonksiyon tetiklenecek.
            //Object array kismi ise 
            //Yes fonksiyonu icin gerek duyabilecegin korumak istedigin degerleri aktarabilirsin. Ornek ( new object[] { myGold, picturePrice } )

            //Ayni islemleri no ve Okay kisimlari icin de yapabilirsin kulalnim ihtiyacina gore, null deger sadece paneli kapatan bir action ayarlar.
            //Ancak mesela No ya basildiginda baska bir eylem tetiklenmesini istersen o eylemi NoAction ve No Object[] kisimlarini doldurarak
            //ayarlayabilirsin.
        }
    }

    public void TabloSatinAlOrnek2()
    {
        int myGold = 100;
        int picturePrice = 75;
        if (picturePrice > myGold)
        {
            //Burada dil islemi yapman gerek, oraya gonderecegin textin mevcut dilde olabilmesi icin
            string header = "Yetersiz altin!";
            string explanation = "Bu ogeyi alabilmek icin yeterli altina sahip degilsiniz.";
            UIInteractHandler.instance.AskQuestion(header, explanation, null, null, YetersizAltinTamamAction, null, null, null);
            //Burada tamama basildiginda Islem Iptal edildi mesaji gosteren bir ornek yaptim.
        }
        else
        {
            string header = "Tablo Sayin Alma Islemi";
            string explanation = "Bir tabloyu satin almak uzeresiniz. Fiyati {%price} altindir. Bu islemi onayliyor musunuz?";
            explanation = explanation.Replace("{%price}", picturePrice.ToString("F2")); // Bu yontemle baska dillerde paranin gelmesi gereken dogru konumu koruyabilirsin. 
            UIInteractHandler.instance.AskQuestion(header, explanation, TabloSatinAlmaOnaylandi, SatinAlmaIptal, null, new object[] { myGold, picturePrice }, new object[] { myGold, picturePrice }, null);
            //Bu sefer evete basilirsa ayni sekilde satin alma devam ederken, hayira basilicna islem iptal edildi gosterelim.
        }
    }

    public void TabloSatinAlmaOnaylandi(object parameter = null)
    {
        if (parameter is object[] param)
        {
            int myGold = 0;
            int price = 0;
            if (param[0] is int _myGold)
                myGold = _myGold;
            if (param[1] is int _price)
                price = _price;

            //Degerleri bu sekilde alip kullanabiliriz.

            //Buradan sonra tablo satin alma islemine yonelebiliriz.
            Debug.Log("Tablo satin alma islemi basariyla tamamlandi => Odenen ucret: " + price);
        }
        else
        {
            Debug.LogError("parameter(s) does not fit! " + parameter);
            return;
        }
    }

    public void YetersizAltinTamamAction(object parameter = null)
    {
        //herhangi bir parametreye ihtiyacimiz yok sadece yeni bir panel acalim.
        string header = "Islem Iptali!";
        string explanation = "Satin alma isleminiz yetersiz altin miktarinizdan dolayi iptal edildi.";
        UIInteractHandler.instance.AskQuestion(header, explanation, null, null, null, null, null, null);
    }

    public void SatinAlmaIptal(object parameter = null)
    {
        if (parameter is object[] param)
        {
            int myGold = 0;
            int price = 0;
            if (param[0] is int _myGold)
                myGold = _myGold;
            if (param[1] is int _price)
                price = _price;

            string header = "Islem Iptal Edildi!";
            string explanation = "{%price} degerindeki tabloyu satin alma isleminizi iptal ettiniz. Mevcut altininiz {%gold}.";
            explanation = explanation.Replace("{%price}", price.ToString("F2")).Replace("{%gold}", myGold.ToString("F2"));
            UIInteractHandler.instance.AskQuestion(header, explanation, null, null, null, null, null, null);
        }
        else
        {
            Debug.LogError("parameter(s) does not fit! " + parameter);
            return;
        }
    }
}
