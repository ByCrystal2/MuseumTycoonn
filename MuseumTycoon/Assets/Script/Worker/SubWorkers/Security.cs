using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class Security : Worker, ISleepable, IMoveable
{
    public Security(int _id,float _speed, float _energy) : base(_id,_speed, _energy)
    {

    }

    public bool CanSleep()
    {
        throw new System.NotImplementedException();
    }
    public void WatchThief(GameObject thief)
    {
        // Hýrsýzý izleme kodlarý
        // Mesafe kontrolü, görüþ açýsý gibi faktörleri kullanarak hýrsýzý izleme iþlemleri
    }

    public void CatchThief(GameObject thief)
    {
        // Hýrsýzý yakalama kodlarý
        // Mesafe kontrolü, yakalanma animasyonlarý veya durumu kontrol etme iþlemleri
    }

    public float GetSpeed()
    {
        throw new System.NotImplementedException();
    }

    public void Move(Vector3 direction)
    {
        throw new System.NotImplementedException();
    }

    public void Rotate(Vector3 axis, float angle)
    {
        throw new System.NotImplementedException();
    }

    public void Sleep()
    {
        throw new System.NotImplementedException();
    }

    public override void AssignTask(Task task)
    {
        throw new System.NotImplementedException();
    }

    public override bool CanPerformTask(Task task)
    {
        throw new System.NotImplementedException();
    }

    public override void CompleteTask(Task task)
    {
        throw new System.NotImplementedException();
    }    
}
