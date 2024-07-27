using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Security : Worker, ISleepable, IMoveable
{
    float IncreaseSpeedAmount = 0.3f;
    float BaseSpeed = 3.5f;
    public Task CurrentActiveTask;
    public Vector3 CurrentTarget;
    public NPCBehaviour MyCurrentNPCTarget;
    private bool NpcCanSleep = true;

    public Security(int _id, string _name, int _level, float _speed, float _energy, int _age, float _heigth, bool _isMale, List<int> _iWorkRoomIDs, WorkerType workerType, float _xp, WorkerBehaviour _behaviour) : base(_id, _name, _level, _speed, _energy, _age, _heigth, _isMale, _iWorkRoomIDs, workerType, _xp, _behaviour)
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
        return BaseSpeed + ((Level + 1)*IncreaseSpeedAmount);
    }

    public void Move(Vector3 direction, bool _hasTarget)
    {
        if (MyCurrentNPCTarget != null)
            direction = MyCurrentNPCTarget.transform.position;

        float targetDistance = Vector3.Distance(Behaviour.transform.position, direction);
        if (targetDistance > 1)
        {
            Behaviour.Anim.SetInteger("Walk", _hasTarget ? 102 : 1);
            Behaviour.Agent.speed = _hasTarget ? GetSpeed() : GetSpeed() * 0.5f;
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
                Behaviour.Anim.SetBool("SecurityHit", true);
                Behaviour.CreateEndTask(2);
                Behaviour.Weapon.SetActive(true);
            }
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

    public void CaughtGuilty()
    {
        if (MyCurrentNPCTarget != null)
        {
            Behaviour.EffectParent.transform.position = MyCurrentNPCTarget.transform.position;
            Behaviour.Weapon.SetActive(false);
            MyCurrentNPCTarget.SetNPCState(NPCState.CombatBeaten, true);
            CompleteTask(CurrentActiveTask);
            CurrentActiveTask = null;
            MyCurrentNPCTarget = null;
            NpcManager.instance.RemoveGuiltyNPC(MyCurrentNPCTarget);
            Behaviour.Anim.SetBool("SecurityHit", false);
            Behaviour.CreateNewTargetDelay(UnityEngine.Random.Range(1.00f, 2.50f));

            ParticleSystem[] beatParticle = Behaviour.EffectParent.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in beatParticle)
            {
                var mainModule = item.main;
                //mainModule.duration = 2;
                item.Stop();
                item.Play();
            }
        }
    }

    public void CreateNewTarget()
    {
        MyCurrentNPCTarget = NpcManager.instance.GetNearestGuiltyNPC(Behaviour.transform.position, IWorkRoomsIDs);
        if (MyCurrentNPCTarget != null)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            double unixTimestamp = currentTime.ToUnixTimeMilliseconds();
            string uniq = unixTimestamp + "_" + UnityEngine.Random.Range(100000, 999999);
            Task newTask = new Task(uniq);
            CurrentActiveTask = newTask;
            AssignTask(CurrentActiveTask);
            CurrentTarget = MyCurrentNPCTarget.transform.position;
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
