using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    

    public PlayerStats currentPlayerStat;
    PlayerMovement playerMove;

    private void Awake()
    {        
        currentPlayerStat = PlayerStats.Idle;
        playerMove = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
       
    }


}

public enum PlayerStats
{
    Idle,
    Move,
    Jump,
}
