using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleNPCBehaviour : MonoBehaviour
{
    public Animator animator;
    public string[] animationStates;
    private int currentAnimationIndex = -1;

    void Start()
    {
        PlayRandomAnimation();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 0.999f && !animator.IsInTransition(0))
        {
            PlayRandomAnimation();
        }
    }
    void PlayRandomAnimation()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, animationStates.Length);
        } while (randomIndex == currentAnimationIndex);

        currentAnimationIndex = randomIndex;

        animator.Play(animationStates[currentAnimationIndex]);
    }
}
