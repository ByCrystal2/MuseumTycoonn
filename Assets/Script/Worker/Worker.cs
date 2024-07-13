using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public abstract class Worker: ITaskAssignable
{
    public int ID;
    public string Name;
    public int Level = 1;
    public float Exp;
    public const int MaxLevel = 5;
    public float Speed;
    public float Energy;
    public int Age;
    public float Height;
    public bool isMale;
    public List<int> IWorkRoomsIDs = new List<int>();
    public List<Task> MyTasks = new List<Task>();
    public WorkerType WorkerType;
    public WorkerBehaviour Behaviour;

    public Worker(int _id,string _name,int _level,float _speed, float _energy,int _age, float _heigth, bool _isMale, List<int> _iWorkRoomIDs, WorkerType workerType,float _xp, WorkerBehaviour _behaviour)
    {
        this.ID = _id;
        this.Name = _name;
        this.Level = _level;
        this.Exp = _xp;
        this.Speed = _speed;
        this.Energy = _energy;
        this.Age = _age;
        this.Height = _heigth;
        this.isMale = _isMale;
        IWorkRoomsIDs.Clear();
        int length = _iWorkRoomIDs.Count;
        for (int i = 0; i < length; i++)
        {
            IWorkRoomsIDs.Add(_iWorkRoomIDs[i]);
        }
        this.WorkerType = workerType;
        this.Behaviour = _behaviour;
    }

    public abstract void AssignTask(Task task);
    public abstract bool CanPerformTask(Task task);
    public abstract void CompleteTask(Task task);
}
[System.Serializable]
public class Task
{
    public string taskName;
    public bool isCompleted;

    public Task(string name)
    {
        taskName = name;
        isCompleted = false;
    }

    public void CompleteTask()
    {
        isCompleted = true;
    }
}
public enum WorkerType
{
    None,
    Security,
    Housekeeper,
    Musician,
    Receptionist,
    BrochureSeller
}
public interface ISleepable
{
    void Sleep();
    bool CanSleep();
}
public interface IToiletUsable
{
    void UseToilet();
    bool CanUseToilet();
}
public interface IEnergyUsable
{
    void ConsumeEnergy(float amount);
    void RechargeEnergy(float amount);
    float GetCurrentEnergy();
}
public interface IMoveable
{
    Vector3 PatrolToRandomPoint(Vector3 originalPosition, float patrolRadius);
    void Move(Vector3 direction, bool _hasTarget);
    void Rotate(Vector3 axis, float angle);
    float GetSpeed();
}
public interface ITaskAssignable // allWarker
{
    void AssignTask(Task task);
    void CompleteTask(Task task);
    bool CanPerformTask(Task task);
}