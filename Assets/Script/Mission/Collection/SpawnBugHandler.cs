using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SpawnBugHandler : MonoBehaviour
{
    [SerializeField] private float checkRadius = 2f; // Kontrol yar��ap�
    [SerializeField] private LayerMask unwantedLayers; // �stenmeyen alanlar i�in LayerMask
    private bool isObjInRightSpot = false;

    private void Start()
    {
        StartCoroutine(CheckSpawnPositionCoroutine());
    }

    private IEnumerator CheckSpawnPositionCoroutine()
    {
        while (!isObjInRightSpot)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, unwantedLayers);

            if (colliders.Length > 0)
            {
                Debug.Log("Spawn objesi yanlis bir konumda, yeniden yerlestiriliyor.");
                Vector3 randomPosition = MissionManager.instance.collectionHandler.spawnController.GetRandomPosition();
                Quaternion customRotation = MissionManager.instance.collectionHandler.spawnController.GetCustomRotation();
                gameObject.transform.SetPositionAndRotation(randomPosition, customRotation);
                yield return new WaitForSeconds(0.5f); // Kontroller aras�na s�re koyarak yo�unlu�u azalt�n
            }
            else
            {
                isObjInRightSpot = true;
            }

            yield return null; // Bir sonraki frame'e kadar bekle
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Kontrol yar��ap�n� g�rselle�tirin
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}

