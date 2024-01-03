using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Housekeeper : Worker, ISleepable, IMoveable
{
    public Housekeeper(int _id, float _speed, float _energy, WorkerType workerType) : base(_id, _speed, _energy, workerType)
    {

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

    public bool CanSleep()
    {
        throw new System.NotImplementedException();
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
}
