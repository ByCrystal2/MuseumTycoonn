using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Musician : Worker, ISleepable, IMoveable
{
    public Musician(int _id, float _speed, float _energy, WorkerType workerType, float _exp, WorkerBehaviour _behaviour) : base(_id, _speed, _energy, workerType, _exp, _behaviour)
    {

    }

    public override void CompleteTask(Task task)
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
    
    public bool CanSleep()
    {
        throw new System.NotImplementedException();
    }

    public float GetSpeed()
    {
        throw new System.NotImplementedException();
    }

    public void Move(Vector3 direction, bool _hasTarget)
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

    public Vector3 PatrolToRandomPoint(Vector3 originalPosition, float patrolRadius)
    {
        throw new System.NotImplementedException();
    }
}
