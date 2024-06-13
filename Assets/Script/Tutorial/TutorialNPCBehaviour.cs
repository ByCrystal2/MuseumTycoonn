using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialNPCBehaviour : MonoBehaviour
{
    Animator anim;
    Animator playerAnim;
    NavMeshAgent agent;
    [SerializeField] Transform followPlayer;
    [SerializeField] NavMeshAgent playerAgent;
    [SerializeField] List<GameObject> MovePoints = new List<GameObject>();
    Transform targetPoint;
    public  TutorialNPCState state = TutorialNPCState.Sit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerAnim = playerAgent.GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    float waitTime = 2f;
    float currentTime = 0;
    float helper = 0;
    private void Update()
    {
        switch (state)
        {
            case TutorialNPCState.Sit:
                // Oturma animasyonu veya iþlemleri burada                
                break;
            case TutorialNPCState.Stand:
                ProcessAnim("Stand", true);

                currentTime += Time.deltaTime;
                if (currentTime >= waitTime)
                {
                    state = TutorialNPCState.Move;
                    currentTime = 0;
                }
                break;
            case TutorialNPCState.Move:
                ProcessAnim("Walk", true);
                if (MovePoints.Count > 0)
                {
                    targetPoint = MovePoints[0].transform;
                    agent.SetDestination(targetPoint.position);
                    helper += Time.deltaTime;
                    if (helper >= 2f)
                    {
                        playerAnim.SetBool("Walk", true);
                        playerAgent.SetDestination(followPlayer.position);
                    }
                    if (Vector3.Distance(transform.position, targetPoint.position) <= 0.5f)
                    {
                        MovePoints.RemoveAt(0);
                    }
                }

                if (MovePoints.Count <= 0)
                {
                    // Kapýya ulaþýldý
                    Debug.Log("From Door.");
                    ProcessAnim("Idle", true);
                    currentTime += Time.deltaTime;
                    if (currentTime >= 0.7f)
                    {
                        playerAnim.SetBool("Walk", false);
                        DialogueManager.instance.SceneTransPanelActivation(true);
                        this.enabled = false;
                    }
                }
                break;
        }
    }

    public void ProcessAnim(string _animName, bool _go)
    {
        anim.SetBool(_animName, _go);
    }

    public void SetState(int _step) // DialogueTrigger UnityEvents.
    {
        if (_step >= 0 && _step < System.Enum.GetValues(typeof(TutorialNPCState)).Length)
        {
            state = (TutorialNPCState)_step;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(_step), "Invalid step index");
        }
    }
}
public enum TutorialNPCState
{
    Sit,
    Stand,
    Move
}
