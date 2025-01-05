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
        Vector3 distance = transform.position - PlayerManager.instance.GetPlayer().BodyTransform.position;
        transform.DOMove(distance, 2f).OnComplete(() =>
        {
            MissionManager.instance.collectionHandler.AdvanceMission();
            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
        });
    }
}
