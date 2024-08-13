using GameTask;
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
    private bool NpcCanSleep = true;
    private float BaseSpeed = 3.5f;

    public Housekeeper(int _id, string _name, int _level, float _speed, float _energy, int _age, float _heigth, bool _isMale, List<int> _iWorkRoomIDs, WorkerType workerType, float _xp, WorkerBehaviour _behaviour) : base(_id, _name, _level, _speed, _energy, _age, _heigth, _isMale, _iWorkRoomIDs, workerType, _xp, _behaviour)
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
        return NpcCanSleep;
    }

    public float GetSpeed()
    {
        return BaseSpeed;
    }

    public void Move(Vector3 direction, bool _hasTarget)
    {
        float targetDistance = Vector3.Distance(Behaviour.transform.position, direction);
        if (targetDistance > 1)
        {
            Behaviour.Anim.SetInteger("Walk", _hasTarget ? 102 : 1);
            Behaviour.Agent.speed = _hasTarget ? BaseSpeed : BaseSpeed * 0.5f;
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
                //Debug.Log("Arrived target. Creating new Target");
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
                Behaviour.Weapon.SetActive(true);
            }
        }

        //Debug.Log("Moving to current Target: " + CurrentTarget);
    }

    public Vector3 PatrolToRandomPoint(Vector3 originalPosition, float patrolRadius)
    {
        int stuckTimer = 500;
        Vector3 target = Vector3.zero;
        float patrolAreaMultiplier = 1;
        do
        {
            stuckTimer--;
            if (stuckTimer <= 0)
                break;

            Vector3 randomPoint = originalPosition + UnityEngine.Random.insideUnitSphere * patrolRadius * patrolAreaMultiplier;

            //Debug.Log("Creating random point: " + randomPoint);
            // Ensure the point is within the NavMesh bounds
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                //Debug.Log("Random point sampled: " + hit.position);
                if (Behaviour.IsPositionAccessible(hit.position))
                    target = hit.position;
            }
            else
            {
                target = Vector3.zero;
                patrolAreaMultiplier++;
            }
        } while (target == Vector3.zero || target == Vector3.positiveInfinity || target == Vector3.negativeInfinity);

        // Generate a random point within the patrol radius
        
        // Set the destination for the agent
        return target;
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
            Behaviour.Weapon.SetActive(false);
        }
    }

    public void CreateNewTarget()
    {
        MyCurrentMessTask = NpcManager.instance.GetNearestMess(Behaviour.transform, IWorkRoomsIDs);
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
        //Debug.Log("set new target point: " + CurrentTarget);
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
