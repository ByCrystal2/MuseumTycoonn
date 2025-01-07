using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CollectionDiamondBehaviour : MonoBehaviour
{
    Collider _collider;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartWinProcess();
        }
    }
    void StartWinProcess()
    {
        Transform targetTransform = PlayerManager.instance.GetPlayer().BodyTransform;
        Vector3 distance = targetTransform.position;
        distance = new Vector3(distance.x, transform.position.y + 1f, distance.z);
        transform.DOMove(distance, 0.1f).OnUpdate(() =>
        {
            Vector3 distance2 = targetTransform.position;
            distance = new Vector3(distance2.x, transform.position.y + 1f, distance2.z);
        }).OnComplete(() =>
        {
            MissionManager.instance.collectionHandler.AdvanceMission();
            AudioManager.instance.PlayTakeCollectionObjSound();
            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
        });
    }
}
