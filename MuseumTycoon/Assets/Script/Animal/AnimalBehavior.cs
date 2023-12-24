using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalBehavior : MonoBehaviour
{
    Animator anim;
    public AnimalStat currentAnimalStat;
    [SerializeField] private float AnimalSpeed;
    NavMeshAgent Agent;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        float s = (int)Random.Range(-100, 51) * 0.01f;
        Agent.speed = AnimalSpeed + s;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GoIdleBoolean(AnimalStat _statType, bool _active, int _idleType)
    {
        if (_statType == AnimalStat.Walk)
        {
            anim.SetBool("Walk", _active);
            anim.SetInteger("Idle", _idleType);
        }
        else if (_statType == AnimalStat.Run)
        {
            anim.SetBool("Run", _active);
            anim.SetInteger("Idle", _idleType);
        }
        else if (_statType == AnimalStat.Eating)
        {
            anim.SetBool("Eating", _active);
            anim.SetInteger("Idle", _idleType);
        }
        else if (_statType == AnimalStat.Stand)
        {
            anim.SetBool("StandToSit", _active);
            anim.SetInteger("Idle", _idleType);
        }
        else if (_statType == AnimalStat.Sit)
        {
            anim.SetBool("SitToStand", _active);
            anim.SetInteger("Idle", _idleType);
        }
    }
}
public enum AnimalStat
{
    None,
    Idle,
    Walk,
    Run,
    Stand,
    Sit,
    Eating,

}