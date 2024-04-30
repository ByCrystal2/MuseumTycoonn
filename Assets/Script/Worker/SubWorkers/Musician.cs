using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Musician : Worker, ISleepable, IMoveable
{
    float HappinessRange = 5f;
    float IncreaseHappinessRange = 1.5f;
    float BaseSpeed = 2.65f;
    public Task CurrentActiveTask;
    public Vector3 CurrentTarget;
    private bool NpcCanSleep = true;

    public Musician(int _id, float _speed, float _energy, WorkerType workerType, float _exp, WorkerBehaviour _behaviour) : base(_id, _speed, _energy, workerType, _exp, _behaviour)
    {

    }

    public override void AssignTask(Task task)
    {
        if (!MyTasks.Contains(task))
            MyTasks.Add(task);
    }

    public override bool CanPerformTask(Task task)
    {
        throw new System.NotImplementedException();
    }
    public override void CompleteTask(Task task)
    {
        if (MyTasks.Contains(task))
            MyTasks.Remove(task);
    }

    public bool CanSleep()
    {
        return NpcCanSleep;
    }

    public float GetSpeed()
    {
        return BaseSpeed * ((Level + 1) * 0.3f);
    }

    public void Move(Vector3 direction, bool _hasTarget)
    {
        float targetDistance = Vector3.Distance(Behaviour.transform.position, direction);
        if (targetDistance > 1)
        {
            Behaviour.Anim.SetInteger("Walk", 1);
            Behaviour.Agent.speed = GetSpeed();
            Behaviour.Agent.SetDestination(direction);
        }
        else
        {
            CurrentTarget = Vector3.zero;
            Behaviour.Anim.SetInteger("Walk", 0);
            Behaviour.Anim.SetBool("PlayGuitar", true);
            Behaviour.Weapon.SetActive(true);
            ParticleSystem[] cleanParticle = Behaviour.EffectParent.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in cleanParticle)
            {
                var mainModule = item.main;
                mainModule.duration = 10;
                item.Stop();
                item.Play();
            }
            Behaviour.PlayWorkSounds();
            Behaviour.CreateEndTask(15);
        }

        //Debug.Log("Moving to current Target: " + CurrentTarget);
    }

    public Vector3 PatrolToRandomPoint(Vector3 originalPosition, float patrolRadius)
    {
        int stuckTimer = 500;
        Vector3 target = Vector3.zero;
        do
        {
            stuckTimer--;
            if (stuckTimer <= 0)
                break;

            Vector3 randomPoint = originalPosition + UnityEngine.Random.insideUnitSphere * patrolRadius;

            //Debug.Log("Creating random point: " + randomPoint);
            // Ensure the point is within the NavMesh bounds
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas);
            //Debug.Log("Random point sampled: " + hit.position);

            if (Behaviour.IsPositionAccessible(hit.position))
                target = hit.position;
        } while (target == Vector3.zero);

        // Generate a random point within the patrol radius

        // Set the destination for the agent
        return target;
    }

    public void EndMusic()
    {
        CompleteTask(CurrentActiveTask);
        CurrentActiveTask = null;
        Behaviour.Anim.SetBool("PlayGuitar", false);
        Behaviour.CreateNewTargetDelay(UnityEngine.Random.Range(1.00f, 2.50f));
        Behaviour.Weapon.SetActive(false);
    }

    public void CreateNewTarget()
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        double unixTimestamp = currentTime.ToUnixTimeMilliseconds();
        string uniq = unixTimestamp + "_" + UnityEngine.Random.Range(100000, 999999);
        Task newTask = new Task(uniq);
        CurrentActiveTask = newTask;
        AssignTask(CurrentActiveTask);
        List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(Behaviour.MyDatas.WorkRoomsIDs);
        int limit = myRooms.Count;
        Vector3 patrolLocation = PatrolToRandomPoint(myRooms[UnityEngine.Random.Range(0, limit)].CenterPoint.position, 7);
        CurrentTarget = patrolLocation;
        Debug.Log("set new target point: " + CurrentTarget);
    }

    public float GetHappinessRange()
    {
        return HappinessRange + Level * IncreaseHappinessRange;
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
