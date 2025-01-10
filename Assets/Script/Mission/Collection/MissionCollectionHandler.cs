using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionCollectionHandler : MonoBehaviour
{
    public MissionCollectionController controller;
    public CollectionSpawnController spawnController;
    GameMission currentGameMission;
    private void Awake()
    {
        spawnController = new CollectionSpawnController();
        StartCoroutine(spawnController.WaitForRoomManagerToBeAppointed());
    }

    public void MissionOfCollectionTimeEnding()
    {
        UIController.instance.missionUIHandler.MissionUIActivation(MissionType.Collection, false);
        MissionManager.instance.MissionTimeEnding(currentGameMission.ID);
        if (currentGameMission.GetMissionCollection().IsSpawnedMission())
            DestroySpawnedObjs();
        currentGameMission = null;
    }
    void DestroySpawnedObjs()
    {
        List<SpawnBugHandler> spawnedObjs = FindObjectsOfType<SpawnBugHandler>().ToList();
        for (int i = 0; i < spawnedObjs.Count; i++)
            Destroy(spawnedObjs[i].gameObject);
    }
    public void CollectionProcess()
    {
        if (MissionIsNull()) { Debug.LogError("CollectionProcess metotu gorevini yerine getiremedi cunku mevcut gorev null!"); return; }
        CollectionHelper helper = currentGameMission.GetMissionCollection();
        bool isSpawnable = helper.missionCollectionType == MissionCollectionType.Gem || helper.missionCollectionType == MissionCollectionType.Gold || helper.missionCollectionType == MissionCollectionType.Grass;
        bool isNpcInteraction = helper.missionCollectionType == MissionCollectionType.NpcInteraction;
        if (isSpawnable)
        {
            SpawnCollection();
        }
        else if (isNpcInteraction)
        {

        }
    }
    public void AdvanceMission()
    {
        CollectionHelper helper = currentGameMission.GetMissionCollection();
        helper.StartValue++;
        UIController.instance.missionUIHandler.collectionUIHandler.UpdateUI();
        if (helper.IsFull())
        {
            UIController.instance.missionUIHandler.MissionUIActivation(MissionType.Collection, false);
            MissionManager.instance.ComplateActiveMissionWithID(currentGameMission.ID);
            currentGameMission = null;
            return;
        }
    }
    void SpawnCollection()
    {
        CollectionHelper halper = currentGameMission.GetMissionCollection();
        GameObject collectionObj = controller.GetCollectionPrefab(halper.missionCollectionType);
        spawnController.SpawnObjects(collectionObj, halper.EndValue);
    }
    bool MissionIsNull()
    {
        return currentGameMission == null;
    }
    public void SetMission(GameMission gameMission)
    {
        currentGameMission = gameMission;
    }
    public GameMission GetCurrentMission()
    {
        return currentGameMission;
    }
    public class CollectionSpawnController
    {
        [SerializeField] public float sphereRadius = 100f; // Kürenin yarýçapý
        [SerializeField] public float fixedYPosition = 1f; // Sabit y pozisyonu
        public Vector3 sphereCenter = Vector3.zero;
        public void SpawnObjects(GameObject _objectToSpawn, int spawnCount)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                // Küre içinde rastgele bir pozisyon hesapla

                Vector3 randomPosition = GetRandomPosition();
                Quaternion customRotation = default;
                customRotation = GetCustomRotation(_objectToSpawn);
                Instantiate(_objectToSpawn, randomPosition, customRotation);
            }
            //Objeye kontrol yapilmadan yanlýs spawn olabilecegi noktalar: 
            /*
             * Satin alinmamis odalar
             * Duvarlar
             * Heykellerin konuldugu slotlar
             * Muzenin dis tarafi
             */
        }

        public System.Collections.IEnumerator WaitForRoomManagerToBeAppointed()
        {
            yield return new WaitUntil(() => RoomManager.instance != null);
            RoomData roomData = RoomManager.instance.GetRoomWithRoomCell(new RoomCell(CellLetter.C, 5));
            sphereCenter = roomData.gameObject.transform.position;
        }
        public Vector3 GetRandomPosition()
        {
            Vector3 randomPosition = sphereCenter + Random.insideUnitSphere * sphereRadius;

            // y deðerini sabit bir deðere ayarla
            randomPosition.y = fixedYPosition;
            return randomPosition;
        }
        public Quaternion GetCustomRotation(GameObject _obj)
        {
            Quaternion customRotation = default;
            if (_obj.TryGetComponent(out CollectionDiamondBehaviour diamondBehaviour))
            {
                customRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            else if (_obj.TryGetComponent(out CollectionGoldBehaviour goldBehaviour))
            {
                customRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            return customRotation;
        }
    }
    private void OnDrawGizmos()
    {
        if (spawnController == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnController.sphereCenter, spawnController.sphereRadius);
    }
}
