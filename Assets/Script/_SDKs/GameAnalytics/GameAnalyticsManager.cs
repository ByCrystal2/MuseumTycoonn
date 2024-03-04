using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAnalyticsManager : MonoBehaviour, IGameAnalyticsATTListener
{

    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
    }
    public void GameAnalyticsATTListenerAuthorized()
    {
        throw new System.NotImplementedException();
    }

    public void GameAnalyticsATTListenerDenied()
    {
        throw new System.NotImplementedException();
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
        throw new System.NotImplementedException();
    }

    public void GameAnalyticsATTListenerRestricted()
    {
        throw new System.NotImplementedException();
    }

    
}
