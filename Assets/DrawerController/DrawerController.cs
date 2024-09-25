using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerController : MonoBehaviour
{
    //1-)Butonun On.Click eventinde panelin setactive a��p kapat�lacak.+
    //2-)On.Enabled kullanarak panelin i�levleri yerine getirilir.+
    //On.Enabled islevleri;
    //2.1-)Panelin scalenin x de�eri artt�r�lacak.(1.31 olana kadar)
    //2.2-)Stop metodunu kullanarak scalenin artt�rlma tweenini durdurabiliriz.
    //3-)Butona tekrar tiklanildiginda setactive kapat�lacak.
    //4-)On.Disable kullanarak panelin i�levleri yerine getirilir.
    //On.Disable islevleri;
    //4.1-)�nlem ama�l� objenin setactiveni tekrardan true edelim.
    //4.2-)Objenin scale de�erini eski haline getirmemiz gerekiyor.
    [SerializeField] float maxScaleValue;
    [SerializeField] float minScaleValue;
    [SerializeField] float openDuration=1;
    [SerializeField] float closeDuration=1;
    static Tween scaleTween;

    
    void ScaleToEnd()
    {
      scaleTween =  transform.DOScaleX(maxScaleValue, openDuration);
    }
    void ScaleToStart()
    {
       scaleTween = transform.DOScaleX(minScaleValue, closeDuration).OnComplete(()=>gameObject.SetActive(false));
        
    }
    public static bool TweenIsPlaying()
    {
        if (scaleTween == null) return false;
        return  scaleTween.IsPlaying();

    } 
    public void ScaleMove(bool _open)
    {
        if (_open)
            ScaleToStart();
        else
            ScaleToEnd();
    }
    //public static bool KillCurrentTween()
    //{
    //    if (scaleTween != null && scaleTween.IsPlaying())
    //    {

    //        DOTween.Kill(scaleTween);
    //        return true;
    //    }
    //    return false;
    //}
}
