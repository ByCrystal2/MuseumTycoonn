using UnityEngine;

public class MissionCollectionHandler : MonoBehaviour
{
    public MissionCollectionController controller;
    CollectionSpawnController spawnController;
    GameMission currentGameMission;
    private void Awake()
    {
        spawnController = new CollectionSpawnController();
    }
    public void SetMission(GameMission gameMission)
    {
        currentGameMission = gameMission;
    }
    public GameMission GetCurrentMission()
    {
        return currentGameMission;
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
    public class CollectionSpawnController
    {
        [SerializeField] private float sphereRadius = 10f; // Kürenin yarýçapý
        [SerializeField] private float fixedYPosition = 1f; // Sabit y pozisyonu

        public void SpawnObjects(GameObject _objectToSpawn, int spawnCount)
        {
            RoomData roomData = RoomManager.instance.GetRoomWithRoomCell(new RoomCell(CellLetter.C, 5));
            Vector3 sphereCenter = roomData.gameObject.transform.position;
            for (int i = 0; i < spawnCount; i++)
            {
                // Küre içinde rastgele bir pozisyon hesapla
                Vector3 randomPosition = sphereCenter + Random.insideUnitSphere * sphereRadius;

                // y deðerini sabit bir deðere ayarla
                randomPosition.y = fixedYPosition;

                // Objeyi spawn et
                Quaternion customRotation = Quaternion.Euler(-90f, 0f, 0f);
                Instantiate(_objectToSpawn, randomPosition, customRotation);
            }
        }
    }

}
