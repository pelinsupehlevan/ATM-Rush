//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public enum CollectableType { Cash, Gold, Diamond }

//public class Collectable : MonoBehaviour
//{
//    public bool isCollected = false;
//    public float collectDistance = 1f;
//    public Transform followTarget;
//    public float followSpeed = 10f;
//    private bool isTip = false;
//    public PlayerController player;

//    public float value = 100f;
//    public CollectableType collectableType = CollectableType.Cash;

//    public GameObject goldPrefab;
//    public GameObject diamondPrefab;

//    private HashSet<Transform> passedTransformers = new HashSet<Transform>();
//    private HashSet<Transform> passedObstacles = new HashSet<Transform>();


//    private Transform lastTransformer = null;
//    private float lastTransformTime = -999f;
//    private float transformCooldown = 0.1f; 

//    private bool isFrozen= false;


//    void Update() => FollowTarget();

//    public void Collect(PlayerController player)
//    {
//        if (isCollected) return;

//        isCollected = true;
//        this.player = player;
//        gameObject.layer = LayerMask.NameToLayer("Collected");
//        player.Collect(this);
//    }

//    //public void FollowTarget()
//    //{
//    //    if (!isCollected || followTarget == null) return;

//    //    Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
//    //    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
//    //}
//    public void FollowTarget()
//    {
//        if (!isCollected || followTarget == null || isFrozen) return;

//        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
//        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
//    }


//    public void SetTip(bool value) => isTip = value;

//    //private void OnTriggerEnter(Collider other)
//    //{
//    //    if (!isCollected) return;

//    //    if (other.CompareTag("Transformer"))
//    //    {
//    //        if (Time.time - lastTransformTime < transformCooldown || passedTransformers.Contains(other.transform))
//    //            return;

//    //        passedTransformers.Add(other.transform);
//    //        lastTransformer = other.transform;
//    //        lastTransformTime = Time.time;

//    //        TransformToNext();
//    //        return;
//    //    }

//    //    if (isTip && other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
//    //    {
//    //        Collectable otherCollectable = other.GetComponent<Collectable>();
//    //        if (otherCollectable != null && !otherCollectable.isCollected)
//    //        {
//    //            otherCollectable.Collect(player);
//    //        }
//    //    }

//    //    if (isTip && other.CompareTag("Drop"))
//    //    {
//    //        player.DropCollectable(this);
//    //    }

//    //    if (isTip && other.CompareTag("Destroy"))
//    //    {
//    //        player.DestroyCollectable(this);
//    //    }

//    //    if (isTip && other.CompareTag("ATM"))
//    //    {
//    //        player.Deposit(this);
//    //    }
//    //}

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!isCollected) return;

//        // Transformer logic (unchanged, but works for any collectable that touches it)
//        if (other.CompareTag("Transformer"))
//        {
//            if (Time.time - lastTransformTime < transformCooldown || passedTransformers.Contains(other.transform))
//                return;

//            passedTransformers.Add(other.transform);
//            lastTransformer = other.transform;
//            lastTransformTime = Time.time;

//            TransformToNext();
//            return;
//        }

//        // Collect new collectables if THIS collectable is the tip
//        if (isTip && other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
//        {
//            Collectable otherCollectable = other.GetComponent<Collectable>();
//            if (otherCollectable != null && !otherCollectable.isCollected)
//            {
//                otherCollectable.Collect(player);
//            }
//        }
//        // Prevent re-triggering the same obstacle multiple times
//        if (other.CompareTag("Drop") || other.CompareTag("Destroy") || other.CompareTag("ATM"))
//        {
//            if (passedObstacles.Contains(other.transform))
//                return;

//            passedObstacles.Add(other.transform);
//        }

//        // Now obstacle handling — works for ANY collectable that touches
//        if (other.CompareTag("Drop"))
//        {
//            player.DropCollectable(this);
//            return; // ensure we don't also trigger other actions in same frame
//        }

//        if (other.CompareTag("Destroy"))
//        {
//            player.DestroyCollectable(this);
//            return;
//        }

//        if (other.CompareTag("ATM"))
//        {
//            player.Deposit(this);
//            return;
//        }
//    }

//    public void TransformToNext()
//    {
//        if (collectableType == CollectableType.Diamond) return;

//        CollectableType newType = (collectableType == CollectableType.Cash) ? CollectableType.Gold : CollectableType.Diamond;
//        GameObject prefab = (newType == CollectableType.Gold) ? goldPrefab : diamondPrefab;

//        //GameObject newObj = Instantiate(prefab, transform.position, Quaternion.identity);
//        GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);

//        Collectable newCollectable = newObj.GetComponent<Collectable>();

//        newCollectable.lastTransformer = this.lastTransformer;
//        newCollectable.lastTransformTime = Time.time;
//        newCollectable.isCollected = true;
//        newCollectable.player = this.player;
//        newCollectable.collectDistance = this.collectDistance;
//        newCollectable.SetTip(this.isTip);

//        int index = player.collectedList.IndexOf(this);
//        if (index >= 0)
//        {
//            player.collectedList[index] = newCollectable;
//        }

//        if (index == 0)
//        {
//            newCollectable.followTarget = player.transform;
//        }
//        else
//        {
//            newCollectable.followTarget = player.collectedList[index - 1].transform;
//        }

//        if (index + 1 < player.collectedList.Count)
//        {
//            player.collectedList[index + 1].followTarget = newCollectable.transform;
//        }

//        newCollectable.transform.position = newCollectable.followTarget.position - newCollectable.followTarget.forward * newCollectable.collectDistance;

//        Destroy(gameObject);
//    }
//}

using System.Collections.Generic;
using UnityEngine;

public enum CollectableType { Cash, Gold, Diamond }

public class Collectable : MonoBehaviour
{
    public bool isCollected = false;
    public float collectDistance = 1f;
    public Transform followTarget;
    public float followSpeed = 10f;
    private bool isTip = false;
    public PlayerController player;

    public float value = 100f;
    public CollectableType collectableType = CollectableType.Cash;

    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    private HashSet<Transform> passedTransformers = new HashSet<Transform>();
    private HashSet<Transform> passedObstacles = new HashSet<Transform>();

    private Transform lastTransformer = null;
    private float lastTransformTime = -999f;
    private float transformCooldown = 0.1f;

    private bool isFrozen = false;

    void Update() => FollowTarget();

    public void Collect(PlayerController player)
    {
        if (isCollected) return;

        isCollected = true;
        this.player = player;
        gameObject.layer = LayerMask.NameToLayer("Collected");
        player.Collect(this);
    }

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

        // Transformer logic
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

        // Tip logic for collecting new items
        if (isTip && other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable otherCollectable = other.GetComponent<Collectable>();
            if (otherCollectable != null && !otherCollectable.isCollected)
            {
                otherCollectable.Collect(player);
            }
        }

        // Obstacle logic — any collectable can trigger
        if (other.CompareTag("Drop") || other.CompareTag("Destroy") || other.CompareTag("ATM"))
        {
            // Prevent duplicate trigger with same obstacle
            if (passedObstacles.Contains(other.transform))
                return;

            passedObstacles.Add(other.transform);

            if (other.CompareTag("Drop"))
            {
                player.DropFromCollectable(this);
                return;
            }

            if (other.CompareTag("Destroy"))
            {
                player.DestroyFromCollectable(this);
                return;
            }

            if (other.CompareTag("ATM"))
            {
                player.DepositFromCollectable(this);
                return;
            }
        }
    }

    //public void TransformToNext()
    //{
    //    if (collectableType == CollectableType.Diamond) return;

    //    CollectableType newType = (collectableType == CollectableType.Cash) ? CollectableType.Gold : CollectableType.Diamond;
    //    GameObject prefab = (newType == CollectableType.Gold) ? goldPrefab : diamondPrefab;

    //    GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);
    //    Collectable newCollectable = newObj.GetComponent<Collectable>();

    //    newCollectable.lastTransformer = this.lastTransformer;
    //    newCollectable.lastTransformTime = Time.time;
    //    newCollectable.isCollected = true;
    //    newCollectable.player = this.player;
    //    newCollectable.collectDistance = this.collectDistance;
    //    newCollectable.SetTip(this.isTip);

    //    int index = player.collectedList.IndexOf(this);
    //    if (index >= 0)
    //    {
    //        player.collectedList[index] = newCollectable;
    //    }

    //    if (index == 0)
    //    {
    //        newCollectable.followTarget = player.transform;
    //    }
    //    else
    //    {
    //        newCollectable.followTarget = player.collectedList[index - 1].transform;
    //    }

    //    if (index + 1 < player.collectedList.Count)
    //    {
    //        player.collectedList[index + 1].followTarget = newCollectable.transform;
    //    }

    //    newCollectable.transform.position = newCollectable.followTarget.position - newCollectable.followTarget.forward * newCollectable.collectDistance;

    //    Destroy(gameObject);
    //}

    //public void TransformToNext()
    //{
    //    if (collectableType == CollectableType.Diamond) return;

    //    CollectableType newType = (collectableType == CollectableType.Cash)
    //        ? CollectableType.Gold
    //        : CollectableType.Diamond;
    //    GameObject prefab = (newType == CollectableType.Gold)
    //        ? goldPrefab
    //        : diamondPrefab;

    //    GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);
    //    Collectable newCollectable = newObj.GetComponent<Collectable>();

    //    // Copy transformer and obstacle history so it doesn't re-trigger instantly
    //    newCollectable.passedTransformers = new HashSet<Transform>(this.passedTransformers);
    //    newCollectable.passedObstacles = new HashSet<Transform>(this.passedObstacles);

    //    newCollectable.lastTransformer = this.lastTransformer;
    //    newCollectable.lastTransformTime = Time.time;
    //    newCollectable.isCollected = true;
    //    newCollectable.player = this.player;
    //    newCollectable.collectDistance = this.collectDistance;
    //    newCollectable.SetTip(this.isTip);

    //    int index = player.collectedList.IndexOf(this);
    //    if (index >= 0)
    //    {
    //        player.collectedList[index] = newCollectable;
    //    }

    //    if (index == 0)
    //    {
    //        newCollectable.followTarget = player.transform;
    //    }
    //    else
    //    {
    //        newCollectable.followTarget = player.collectedList[index - 1].transform;
    //    }

    //    if (index + 1 < player.collectedList.Count)
    //    {
    //        player.collectedList[index + 1].followTarget = newCollectable.transform;
    //    }

    //    newCollectable.transform.position = newCollectable.followTarget.position
    //        - newCollectable.followTarget.forward * newCollectable.collectDistance;

    //    Destroy(gameObject);
    //}
    public void TransformToNext()
    {
        if (collectableType == CollectableType.Diamond) return;

        CollectableType newType = (collectableType == CollectableType.Cash)
            ? CollectableType.Gold
            : CollectableType.Diamond;
        GameObject prefab = (newType == CollectableType.Gold)
            ? goldPrefab
            : diamondPrefab;

        GameObject newObj = Instantiate(prefab, transform.position, transform.rotation);
        Collectable newCollectable = newObj.GetComponent<Collectable>();

        // Copy transformer and obstacle history so it doesn't re-trigger instantly
        newCollectable.passedTransformers = new HashSet<Transform>(this.passedTransformers);
        newCollectable.passedObstacles = new HashSet<Transform>(this.passedObstacles);

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

        newCollectable.transform.position = newCollectable.followTarget.position
            - newCollectable.followTarget.forward * newCollectable.collectDistance;

        Destroy(gameObject);
    }

}



