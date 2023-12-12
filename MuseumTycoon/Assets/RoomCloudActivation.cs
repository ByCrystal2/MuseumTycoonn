    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

public class RoomCloudActivation : MonoBehaviour
{
    private void Start()
    {
        // RoomData scriptine eriþim
        RoomData roomData = GetComponentInParent<RoomData>();

        if (roomData.isActive)        
            gameObject.SetActive(false);        
        else
            gameObject.SetActive(true);

    }

    public void CloudActivationChange(bool isActive)
    {
        gameObject.SetActive(isActive); // Baðlý olduðu objenin aktifliðini deðiþtirme
    }
}
