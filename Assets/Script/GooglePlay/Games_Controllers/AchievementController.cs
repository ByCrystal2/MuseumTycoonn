using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "Google Play Games/Achievements")]
public class AchievementController : ScriptableObject
{
    public int PurchasedRoomCount;
    private List<AchievementControl> purchasedRoomAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQAw", 1),
    };
    public int NumberOfTablesPlaced;
    private List<AchievementControl> placedTableAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQAg", 1),
        new AchievementControl("CgkIzoLC7dsGEAIQCA", 10),
        new AchievementControl("CgkIzoLC7dsGEAIQDg", 50),
        // Daha fazla hedef ekleyebilirsiniz
    };
    public int NumberOfVisitors;
    private List<AchievementControl> visitorCountAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQBw", 1),
        // Daha fazla hedef ekleyebilirsiniz
    };
    public int NumberOfStatuesPlaced;
    private List<AchievementControl> statuesPlacedCountAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQBw", 1),
        new AchievementControl("CgkIzoLC7dsGEAIQDw", 10),
        // Daha fazla hedef ekleyebilirsiniz
    };
    public int TotalNumberOfMuseumVisitors;
    private List<AchievementControl> totalVisitorCountAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQEA", 50),
        // Daha fazla hedef ekleyebilirsiniz
    };

    //WorkerHiringCount
    public int TotalWorkerHiringCount;
    private List<AchievementControl> totalWorkerHiringAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQDQ", 10),
        // Daha fazla hedef ekleyebilirsiniz
    };
    private List<WorkerAchievement> WorkerHiringAchievementControl = new List<WorkerAchievement>()
    {
        new WorkerAchievement(0, // => Security
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                         new AchievementControl("CgkIzoLC7dsGEAIQCw", 1)
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Housekeeper
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                        new AchievementControl("CgkIzoLC7dsGEAIQCg", 1),
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Musician
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                        new AchievementControl("CgkIzoLC7dsGEAIQFQ", 1),
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Receptionist
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {

                    }
                )
            }
        ),
        new WorkerAchievement(0, // => BrochureSeller
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {

                    }
                )
            }
        )
    };
    //WorkerHiringCount

    //WorkerAssignCount
    public int TotalWorkerAssignCount;
    private List<AchievementControl> totalWorkerAssignAchievements = new List<AchievementControl>
    {
        new AchievementControl("CgkIzoLC7dsGEAIQEQ", 1),
        new AchievementControl("CgkIzoLC7dsGEAIQEg", 5),
    };
    private List<WorkerAchievement> WorkerAssignAchievementControl = new List<WorkerAchievement>()
    {
        new WorkerAchievement(0, // => Security
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                       
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Housekeeper
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                        new AchievementControl("CgkIzoLC7dsGEAIQEw", 2)
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Musician
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {
                        new AchievementControl("CgkIzoLC7dsGEAIQFA", 1)
                    }
                )
            }
        ),
        new WorkerAchievement(0, // => Receptionist
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {

                    }
                )
            }
        ),
        new WorkerAchievement(0, // => BrochureSeller
            new List<WorkerAchievementHelper>()
            {
                new WorkerAchievementHelper
                (
                    new List<AchievementControl>
                    {

                    }
                )
            }
        )
    };
    //WorkerAssignCount

    private GPGAchievement _achievements = new GPGAchievement();
    public void ResetValues()
    {
        PurchasedRoomCount = 0;
        NumberOfTablesPlaced = 0;
        NumberOfVisitors = 0;
        NumberOfStatuesPlaced = 0;
        TotalNumberOfMuseumVisitors = 0;
        TotalWorkerHiringCount = 0;
        TotalWorkerAssignCount = 0;

        for (int i = 0; i < WorkerHiringAchievementControl.Count; i++)
        {
            var workerAchievements = WorkerHiringAchievementControl[i].WorkerAchievements;
            WorkerHiringAchievementControl[i] = new WorkerAchievement(0, workerAchievements);
        }

        for (int i = 0; i < WorkerAssignAchievementControl.Count; i++)
        {
            var workerAchievements = WorkerAssignAchievementControl[i].WorkerAchievements;
            WorkerAssignAchievementControl[i] = new WorkerAchievement(0, workerAchievements);
        }

        Debug.Log("All values have been reset.");
    }
    public void SetDatas(int _purchasedRoomControl, int _numberOfTablePlaced, int _numberOfVisitors,int _numberOfStatuesPlaced, int _totalNumberOfVisitorsCount, int _workerHiringCount, int _workerAssignCount)
    {
        PurchasedRoomCount = _purchasedRoomControl;
        NumberOfTablesPlaced = _numberOfTablePlaced;
        NumberOfVisitors = _numberOfVisitors;
        NumberOfStatuesPlaced = _numberOfStatuesPlaced;
        TotalNumberOfMuseumVisitors = _totalNumberOfVisitorsCount;
        TotalWorkerHiringCount = _workerHiringCount;
        TotalWorkerAssignCount = _workerAssignCount;
    }
    public void ShowAchievements()
    {
        _achievements.ShowAchievementsUI();
    }
    public void FirstGameAchievement()
    {
        _achievements.ShowAchievementInSentId("CgkIzoLC7dsGEAIQBQ"); // ilk giris
    }
    public void IncreasePurchasedRoomCount()
    {
        int maxTargetNumber = purchasedRoomAchievements.Last().TargetNumber;

        if (PurchasedRoomCount >= maxTargetNumber)
            return;
        PurchasedRoomCount++;
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void IncreaseNumberOfTablesPlaced()
    {
        int maxTargetNumber = placedTableAchievements.Last().TargetNumber;

        if (NumberOfTablesPlaced >= maxTargetNumber)
            return;
        NumberOfTablesPlaced++;
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void IncreaseNumberOfVisitors()
    {
        int maxTargetNumber = visitorCountAchievements.Last().TargetNumber;

        if (NumberOfVisitors >= maxTargetNumber)
            return;
        NumberOfVisitors++;
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void IncreaseNumberOfStatuesPlaced()
    {
        int maxTargetNumber = statuesPlacedCountAchievements.Last().TargetNumber;

        if (NumberOfStatuesPlaced >= maxTargetNumber)
            return;

        NumberOfStatuesPlaced++;
        //#if UNITY_EDITOR
        //    FirestoreManager.instance.UpdateGameData("ahmet123");
        //#else
        //FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
        //#endif
    }

    public void IncreaseOrDecreaseTotalNumberOfMuseumVisitor(bool _increase)
    {
        int maxTargetNumber = totalVisitorCountAchievements.Last().TargetNumber;

        if (TotalNumberOfMuseumVisitors >= maxTargetNumber)
        {
            Debug.LogWarning("Total number of museum visitors has already reached the maximum target number.");
            return;
        }

        if (!_increase && TotalNumberOfMuseumVisitors <= 0)
        {
            Debug.LogWarning("Total number of museum visitors cannot be less than 0.");
            return;
        }

        if (_increase)
            TotalNumberOfMuseumVisitors++;
        else
            TotalNumberOfMuseumVisitors--;

        //#if UNITY_EDITOR
        //    FirestoreManager.instance.UpdateGameData("ahmet123");
        //#else
        //FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
        //#endif
    }

    public void IncreaseWorkerHiringCount(WorkerType _workerType)
    {
        int index = (int)_workerType - 1;
        if (index >= 0 && index < WorkerHiringAchievementControl.Count)
        {
            WorkerAchievement workerAchievement = WorkerHiringAchievementControl[index];

            // Son elemanýn TargetNumber'ýný alýyoruz
            int maxTargetNumber = workerAchievement.WorkerAchievements.Last().Controls.Max(control => control.TargetNumber);

            // Eðer mevcut iþçi sayýsý maxTargetNumber'dan büyükse, metottan çýk
            if (workerAchievement.NumberOfWorkersOwned > maxTargetNumber)
            {
                Debug.LogWarning($"WorkerHiringCount has already reached the maximum target number for {workerAchievement}");
                return;
            }

            workerAchievement.NumberOfWorkersOwned++;
            WorkerHiringAchievementControl[index] = workerAchievement;
            TotalWorkerHiringCount++;

            //#if UNITY_EDITOR
            //        FirestoreManager.instance.UpdateGameData("ahmet123");
            //#else
            //FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
            //#endif
        }
        else
        {
            Debug.LogError($"index:{index} | WorkerHiringAchievementControl.Count:{WorkerHiringAchievementControl.Count}");
        }
    }


    public void IncreaseWorkerAssignCount(WorkerType _workerType)
    {
        int index = (int)_workerType - 1;
        if (index >= 0 && index < WorkerAssignAchievementControl.Count)
        {
            WorkerAchievement workerAchievement = WorkerAssignAchievementControl[index];
            WorkerAchievementHelper helper = workerAchievement.WorkerAchievements.Last();
            if (helper.Controls.Count <= 0)
            {
                Debug.Log("Bu tur herhangi bir basarým bulundurmamaktadir. Tur:" + _workerType + " basarim icerigi sayisi:" + helper.Controls.Count);
                return;
            }
            // Son elemanýn TargetNumber'ýný alýyoruz
            int maxTargetNumber = helper.Controls.Max(control => control.TargetNumber);

            // Eðer mevcut iþçi sayýsý maxTargetNumber'dan büyükse, metottan çýk
            if (workerAchievement.NumberOfWorkersOwned > maxTargetNumber)
            {
                Debug.LogWarning($"WorkerAssignCount has already reached the maximum target number for {workerAchievement}");
                return;
            }

            workerAchievement.NumberOfWorkersOwned++;
            WorkerAssignAchievementControl[index] = workerAchievement;
            TotalWorkerAssignCount++;

            //#if UNITY_EDITOR
            //        FirestoreManager.instance.UpdateGameData("ahmet123");
            //#else
            //FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
            //#endif
        }
        else
        {
            Debug.LogError($"index:{index} | WorkerAssignAchievementControl.Count:{WorkerAssignAchievementControl.Count}");
        }
    }


    private void TotalWorkerHiringControler()
    {//toplam satin alinan isci sayisi controlu        
        foreach (var control in totalWorkerHiringAchievements)
        {
            if (TotalWorkerHiringCount >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    private void TotalWorkerAssignControler()
    {//toplam gorev atanan isci sayisi controlu        
        foreach (var control in totalWorkerAssignAchievements)
        {
            if (TotalWorkerAssignCount >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    public void WorkerHiringControl(WorkerType _workerType)
    {
        int index = (int)_workerType - 1;
        if (index >= 0 && index < WorkerHiringAchievementControl.Count)
        {
            WorkerAchievement workerAchievement = WorkerHiringAchievementControl[index];
            foreach (var helper in workerAchievement.WorkerAchievements)
            {
                foreach (var control in helper.Controls)
                {
                    if (workerAchievement.NumberOfWorkersOwned >= control.TargetNumber)
                    {
                        _achievements.ShowAchievementInSentId(control.AchievementID);
                    }
                }
            }
        }
        else
            Debug.LogError($"index:{index} | WorkerHiringAchievementControl.Count:{WorkerHiringAchievementControl.Count}");
        TotalWorkerHiringControler();
    }

    public void WorkerAssignControl(WorkerType _workerType)
    {//isci gorev atama sayisi contorlu
        int index = (int)_workerType - 1;
        if (index >= 0 && index < WorkerAssignAchievementControl.Count)
        {
            WorkerAchievement workerAchievement = WorkerAssignAchievementControl[index];
            foreach (var helper in workerAchievement.WorkerAchievements)
            {
                foreach (var control in helper.Controls)
                {
                    if (workerAchievement.NumberOfWorkersOwned >= control.TargetNumber)
                    {
                        _achievements.ShowAchievementInSentId(control.AchievementID);
                    }
                }
            }
        }
        else
            Debug.LogError($"index:{index} | WorkerAssignAchievementControl.Count:{WorkerAssignAchievementControl.Count}");
        TotalWorkerAssignControler();
    }
    public void PurchasedRoomControl()
    {
        Debug.Log("Achievement/PurchasedRoomControl => " + PurchasedRoomCount);

        foreach (var control in purchasedRoomAchievements)
        {
            if (PurchasedRoomCount >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    public void PlacedTableControl()
    {
        Debug.Log("Achievement/NumberOfTablesPlaced => " + NumberOfTablesPlaced);

        foreach (var control in placedTableAchievements)
        {
            if (NumberOfTablesPlaced >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    public void VisitorCountControl()
    {
        Debug.Log("Achievement/NumberOfVisitors => " + NumberOfVisitors);

        foreach (var control in visitorCountAchievements)
        {
            if (NumberOfVisitors >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    public void StatuesPlacedCountControl()
    {
        Debug.Log("Achievement/NumberOfStatuesPlaced => " + NumberOfStatuesPlaced);

        foreach (var control in statuesPlacedCountAchievements)
        {
            if (NumberOfStatuesPlaced >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
    public void TotalVisitorCountControl()
    {
        Debug.Log("Achievement/TotalNumberOfMuseumVisitors => " + TotalNumberOfMuseumVisitors);

        foreach (var control in totalVisitorCountAchievements)
        {
            if (TotalNumberOfMuseumVisitors >= control.TargetNumber)
            {
                _achievements.ShowAchievementInSentId(control.AchievementID);
            }
        }
    }
}
[System.Serializable]
public struct WorkerAchievement
{
    public int NumberOfWorkersOwned;
    public List<WorkerAchievementHelper> WorkerAchievements;
    public WorkerAchievement(int _numberOfWorkersOwned, List<WorkerAchievementHelper> _workerAchievementLists)
    {
        NumberOfWorkersOwned = _numberOfWorkersOwned;
        WorkerAchievements = _workerAchievementLists;
    }
}
[System.Serializable]
public struct WorkerAchievementHelper
{
    public List<AchievementControl> Controls;
    public WorkerAchievementHelper(List<AchievementControl> _controls)
    {
        Controls = _controls;
    }
}
[System.Serializable]
public struct AchievementControl
{
    public string AchievementID;
    public int TargetNumber;
    public AchievementControl(string _achievementID, int _targetNumber)
    {
        AchievementID = _achievementID;
        TargetNumber = _targetNumber;
    }    
}
//[CustomEditor(typeof(AchievementController))]
//public class AchievementControllerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        AchievementController myScript = (AchievementController)target;
//        if (GUILayout.Button("Reset Values"))
//        {
//            myScript.ResetValues();
//            EditorUtility.SetDirty(myScript);
//        }
//    }
//}
