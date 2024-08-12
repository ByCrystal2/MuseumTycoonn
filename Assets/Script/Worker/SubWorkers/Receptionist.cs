using GameTask;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Receptionist : Worker, ISleepable
{
    public Receptionist(int _id, string _name, int _level, float _speed, float _energy, int _age, float _heigth, bool _isMale, List<int> _iWorkRoomIDs, WorkerType workerType, float _xp, WorkerBehaviour _behaviour) : base(_id, _name, _level, _speed, _energy, _age, _heigth, _isMale, _iWorkRoomIDs, workerType, _xp, _behaviour)
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

    public void Sleep()
    {
        throw new System.NotImplementedException();
    }
}
