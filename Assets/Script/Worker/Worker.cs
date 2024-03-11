using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public abstract class Worker: ITaskAssignable
{
    public int ID;
    public string Name;
    public int Level;
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

    public Worker(int _id,float _speed, float _energy, WorkerType workerType,float _xp, WorkerBehaviour _behaviour)
    {
        this.ID = _id;
        this.Speed = _speed;
        this.Energy = _energy;
        this.Energy = Random.Range(1, 101); // Gecici Kod.
        this.Age = Random.Range(20, 500); // Gecici Kod.
        this.Height = Random.Range(_energy, 250); // Gecici Kod.
        this.isMale = Random.Range(0, 2) == 1 ? true : false; // Gecici Kod.
        this.Name = "Kosippy Worker"; // Gecici Kod.
        this.Level = Random.Range(1, MaxLevel + 1); // Gecici Kod.
        this.Exp = _xp;
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