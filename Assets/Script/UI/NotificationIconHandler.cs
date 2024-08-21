using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationIconHandler : MonoBehaviour
{
    [SerializeField] NotificationState state;
    public NotificationState GetIconState()
    {
        return state;
    }
}
