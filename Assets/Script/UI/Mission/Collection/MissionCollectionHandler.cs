using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCollectionHandler : MonoBehaviour
{
    public MissionCollectionController controller;
    CollectionSpawnController spawnController;
    private void Awake()
    {
        spawnController = new CollectionSpawnController();
    }
    public void SpawnCollection(GameMission gameMission)
    {
        CollectionHalper halper = gameMission.GetMissionCollection();
        GameObject collectionObj = controller.GetCollectionPrefab(halper.missionCollectionType);
        spawnController.SpawnObjects(collectionObj, halper.EndValue);
    }
    public class CollectionSpawnController
    {
        [SerializeField] private float sphereRadius = 10f; // Kürenin yarýçapý

        public void SpawnObjects(GameObject _objectToSpawn, int spawnCount)
        {
            RoomData roomData = RoomManager.instance.GetRoomWithRoomCell(new RoomCell(CellLetter.C, 5));
            Vector3 sphereCenter = roomData.gameObject.transform.position;
            for (int i = 0; i < spawnCount; i++)
            {
                // Küre içinde rastgele bir pozisyon hesapla
                Vector3 randomPosition = sphereCenter + Random.insideUnitSphere * sphereRadius;

                // Objeyi spawn et
                Instantiate(_objectToSpawn, randomPosition, Quaternion.identity);
            }
        }
    }
}
