using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableType { Cash, Gold, Diamond }

public class Collectable : MonoBehaviour
{
    public bool isCollected = false;
    public float collectDistance = 1f;
    public Transform followTarget;
    public float followSpeed = 7f;
    private bool isTip = false;
    public PlayerController player;

    public float value = 100f;
    public CollectableType collectableType = CollectableType.Cash;

    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    private HashSet<Transform> passedTransformers = new HashSet<Transform>();

    private Transform lastTransformer = null;
    private float lastTransformTime = -999f;
    private float transformCooldown = 0.1f; 

    private bool isFrozen= false;


    void Update() => FollowTarget();

    public void Collect(PlayerController player)
    {
        if (isCollected) return;

        isCollected = true;
        this.player = player;
        gameObject.layer = LayerMask.NameToLayer("Collected");
        player.Collect(this);
    }

    //public void FollowTarget()
    //{
    //    if (!isCollected || followTarget == null) return;

    //    Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
    //    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    //}
    public void FollowTarget()
    {
        if (!isCollected || followTarget == null || isFrozen) return;

        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }


    public void SetTip(bool value) => isTip = value;

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected) return;

        if (other.CompareTag("Transformer"))
        {
            if (Time.time - lastTransformTime < transformCooldown || passedTransformers.Contains(other.transform))
                return;

            passedTransformers.Add(other.transform);
            lastTransformer = other.transform;
            lastTransformTime = Time.time;

            TransformToNext();
            return;
        }

        if (isTip && other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable otherCollectable = other.GetComponent<Collectable>();
            if (otherCollectable != null && !otherCollectable.isCollected)
            {
                otherCollectable.Collect(player);
            }
        }

        if (isTip && other.CompareTag("Drop"))
        {
            player.DropCollectable(this);
        }

        if (isTip && other.CompareTag("Destroy"))
        {
            player.DestroyCollectable(this);
        }

        if (isTip && other.CompareTag("ATM"))
        {
            player.Deposit(this);
        }
    }


    public void TransformToNext()
    {
        if (collectableType == CollectableType.Diamond) return;

        CollectableType newType = (collectableType == CollectableType.Cash) ? CollectableType.Gold : CollectableType.Diamond;
        GameObject prefab = (newType == CollectableType.Gold) ? goldPrefab : diamondPrefab;

        //GameObject newObj = Instantiate(prefab, transform.position, Quaternion.identity);
        GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);

        Collectable newCollectable = newObj.GetComponent<Collectable>();

        newCollectable.lastTransformer = this.lastTransformer;
        newCollectable.lastTransformTime = Time.time;
        newCollectable.isCollected = true;
        newCollectable.player = this.player;
        newCollectable.collectDistance = this.collectDistance;
        newCollectable.SetTip(this.isTip);

        int index = player.collectedList.IndexOf(this);
        if (index >= 0)
        {
            player.collectedList[index] = newCollectable;
        }

        if (index == 0)
        {
            newCollectable.followTarget = player.transform;
        }
        else
        {
            newCollectable.followTarget = player.collectedList[index - 1].transform;
        }

        if (index + 1 < player.collectedList.Count)
        {
            player.collectedList[index + 1].followTarget = newCollectable.transform;
        }

        newCollectable.transform.position = newCollectable.followTarget.position - newCollectable.followTarget.forward * newCollectable.collectDistance;

        Destroy(gameObject);
    }
}




