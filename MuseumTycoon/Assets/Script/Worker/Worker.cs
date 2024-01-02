using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Worker: ITaskAssignable
{
    public int ID;
    public string Name;
    public float Speed;
    public float Energy;
    public int Age;
    public float Height;
    public bool isMale;
    public List<int> IWorkRoomsID = new List<int>();
    public List<Task> MyTasks = new List<Task>();
    public Worker(int _id,float _speed, float _energy)
    {
        this.ID = _id;
        this.Speed = _speed;
        this.Energy = _energy;
        this.Name = "Kosippy Worker";
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
    void Move(Vector3 direction);
    void Rotate(Vector3 axis, float angle);
    float GetSpeed();
}
public interface ITaskAssignable // allWarker
{
    void AssignTask(Task task);
    void CompleteTask(Task task);
    bool CanPerformTask(Task task);
}