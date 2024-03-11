using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

[System.Serializable]
public class Housekeeper : Worker, ISleepable, IMoveable
{
    float DecreaseCleanTimeAmount = 0.8f;
    float BaseCleanTimeAmount = 5f;
    public Task CurrentActiveTask;
    public Vector3 CurrentTarget;
    public NpcMess MyCurrentMessTask;
    public Coroutine currentActiveCoroutine;
    public Housekeeper(int _id, float _speed, float _energy, WorkerType workerType, float _exp, WorkerBehaviour _behaviour) : base(_id, _speed, _energy, workerType, _exp, _behaviour)
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
        if(MyTasks.Contains(task))
            MyTasks.Remove(task);
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
        float targetDistance = Vector3.Distance(Behaviour.transform.position, direction);
        if (targetDistance > 1)
        {
            Behaviour.Anim.SetInteger("Walk", _hasTarget ? 102 : 1);
            Behaviour.Agent.SetDestination(direction);
        }
        else
        {
            CurrentTarget = Vector3.zero;
            Behaviour.Anim.SetInteger("Walk", 0);
            if (!_hasTarget)
            {
                Behaviour.CreateNewTargetDelay(UnityEngine.Random.Range(2.00f, 4.00f));
                //Behaviour.Invoke(nameof(CreateNewTarget), UnityEngine.Random.Range(2.00f, 4.00f));
                Debug.Log("Arrived target. Creating new Target");
            }
            else
            {
                Behaviour.Anim.SetBool("Clean", true);
                float CleanLength = BaseCleanTimeAmount - ((Level - 1) * DecreaseCleanTimeAmount);
                ParticleSystem cleanParticle = Behaviour.EffectParent.GetComponentInChildren<ParticleSystem>();
                var mainModule = cleanParticle.main;
                mainModule.duration = CleanLength;
                cleanParticle.Stop();
                cleanParticle.Play();
                Behaviour.CreateEndTask(CleanLength);
            }
        }

        Debug.Log("Moving to current Target: " + CurrentTarget);
    }

    public Vector3 PatrolToRandomPoint(Vector3 originalPosition, float patrolRadius)
    {
        // Generate a random point within the patrol radius
        Vector3 randomPoint = originalPosition + UnityEngine.Random.insideUnitSphere * patrolRadius;

        Debug.Log("Creating random point: " + randomPoint);
        // Ensure the point is within the NavMesh bounds
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas);
        Debug.Log("Random point sampled: " + hit.position);
        // Set the destination for the agent
        return hit.position;
    }

    public void EndClean()
    {
        if (MyCurrentMessTask != null)
        {
            CompleteTask(CurrentActiveTask);
            CurrentActiveTask = null;
            NpcManager.instance.DestroyMess(MyCurrentMessTask.gameObject);
            Behaviour.Anim.SetBool("Clean", false);
            Behaviour.CreateNewTargetDelay(UnityEngine.Random.Range(1.00f, 2.50f));
        }
    }

    public void CreateNewTarget()
    {
        MyCurrentMessTask = NpcManager.instance.GetNearestMess(Behaviour.transform.position, IWorkRoomsIDs);
        if (MyCurrentMessTask != null)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            double unixTimestamp = currentTime.ToUnixTimeMilliseconds();
            string uniq = unixTimestamp + "_" + UnityEngine.Random.Range(100000, 999999);
            Task newTask = new Task(uniq);
            CurrentActiveTask = newTask;
            AssignTask(CurrentActiveTask);
            CurrentTarget = MyCurrentMessTask.transform.position;
            NpcManager.instance.SetMessCleaning(MyCurrentMessTask.transform);
        }
        else
        {
            List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(Behaviour.MyDatas.WorkRoomsIDs);
            int limit = myRooms.Count;
            Vector3 patrolLocation = PatrolToRandomPoint(myRooms[UnityEngine.Random.Range(0, limit)].CenterPoint.position, 7);
            //Vector3 patrolLocation = PatrolToRandomPoint(Behaviour.transform.position, Behaviour.PatrolRadius);
            CurrentTarget = patrolLocation;
        }
        Debug.Log("set new target point: " + CurrentTarget);
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
