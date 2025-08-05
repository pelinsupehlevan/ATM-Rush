using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Collectable : MonoBehaviour
//{

//    public bool isCollected = false;

//    public float collectDistance = 1f;
//    public Transform followTarget;
//    public float followSpeed = 7f;
//    private bool isTip = false;
//    private PlayerController player;

//    public float value = 100f;
//    public GameObject[] collectables; // 0 coin 1 gold 2 diamond
//    private bool hasTransformed = false;


//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        FollowTarget();
//    }

//    public void Collect(PlayerController player)
//    {
//        if (isCollected) return;

//        isCollected = true;
//        this.player = player;

//        gameObject.layer = LayerMask.NameToLayer("Collected");

//        player.Collect(this);
//    }


//    public void FollowTarget()
//    {
//        if (!isCollected || followTarget == null) return;

//        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
//        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
//    }

//    public void SetTip(bool value)
//    {
//        isTip = value;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!isCollected) return;

//        if (isTip && other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
//        {
//            Collectable otherCollectable = other.GetComponent<Collectable>();
//            if (otherCollectable != null && !otherCollectable.isCollected)
//            {
//                otherCollectable.Collect(player);
//            }
//        }

//        if (isTip && other.CompareTag("Drop"))
//        {
//            player.DropCollectable(this);
//        }

//        if (isTip && other.CompareTag("Destroy"))
//        {
//            player.DestroyCollectable(this);
//        }

//        if (isTip && other.CompareTag("ATM"))
//        {
//            player.Deposit(this);
//        }

//        if (other.CompareTag("Transformer") && !hasTransformed)
//        {
//            hasTransformed = true;
//            Transform();
//        }

//    }
//    public void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Transformer"))
//        {
//            hasTransformed = false;
//        }

//    }

//    public void Transform()
//    {
//        //if (CompareTag("Cash")) {
//        //    //GameObject newCollectable = Instantiate(collectables[1], transform.position, Quaternion.identity);
//        //    //newCollectable.transform.localScale = transform.localScale;
//        //    //Destroy(gameObject);
//        //    //newCollectable.tag = "Gold";
//        //    //player.Collect(newCollectable.GetComponent<Collectable>());

//        //    int indexInList = player.collectedList.IndexOf(this);
//        //    if (indexInList == -1) return; 

//        //    GameObject newObject = Instantiate(collectables[1], transform.position, transform.rotation);
//        //    newObject.transform.localScale = transform.localScale;
//        //    newObject.tag = "Gold";

//        //    Collectable newCollectable = newObject.GetComponent<Collectable>();
//        //    newCollectable.isCollected = true;
//        //    newCollectable.player = player;
//        //    newCollectable.followTarget = this.followTarget;
//        //    newCollectable.collectDistance = this.collectDistance;
//        //    newCollectable.SetTip(this.isTip);
//        //    newObject.layer = LayerMask.NameToLayer("Collected");

//        //    player.collectedList[indexInList] = newCollectable;

//        //    Destroy(gameObject);

//        //}

//        //if (CompareTag("Gold")) {
//        //    //GameObject newCollectable = Instantiate(collectables[2], transform.position, Quaternion.identity);
//        //    //newCollectable.transform.localScale = transform.localScale;
//        //    //Destroy(gameObject);
//        //    //newCollectable.tag = "Diamond";
//        //    //player.Collect(newCollectable.GetComponent<Collectable>());

//        //    int indexInList = player.collectedList.IndexOf(this);
//        //    if (indexInList == -1) return;

//        //    GameObject newObject = Instantiate(collectables[2], transform.position, transform.rotation);
//        //    newObject.transform.localScale = transform.localScale;
//        //    newObject.tag = "Diamond";

//        //    Collectable newCollectable = newObject.GetComponent<Collectable>();
//        //    newCollectable.isCollected = true;
//        //    newCollectable.player = player;
//        //    newCollectable.followTarget = this.followTarget;
//        //    newCollectable.collectDistance = this.collectDistance;
//        //    newCollectable.SetTip(this.isTip);
//        //    newObject.layer = LayerMask.NameToLayer("Collected");

//        //    player.collectedList[indexInList] = newCollectable;

//        //    Destroy(gameObject);

//        //}

//        //if (CompareTag("Diamond")) return;





//        int indexInList = player.collectedList.IndexOf(this);
//        if (indexInList == -1) return;

//        GameObject newObject = null;

//        if (CompareTag("Cash"))
//            newObject = Instantiate(collectables[1], transform.position, transform.rotation);
//        else if (CompareTag("Gold"))
//            newObject = Instantiate(collectables[2], transform.position, transform.rotation);
//        else if (CompareTag("Diamond"))
//            return;

//        if (newObject == null) return;

//        newObject.transform.localScale = transform.localScale;
//        newObject.tag = CompareTag("Cash") ? "Gold" : "Diamond";
//        newObject.layer = LayerMask.NameToLayer("Collected");

//        Collectable newCollectable = newObject.GetComponent<Collectable>();
//        newCollectable.isCollected = true;
//        newCollectable.player = player;
//        newCollectable.followTarget = this.followTarget;
//        newCollectable.collectDistance = this.collectDistance;
//        newCollectable.SetTip(this.isTip);

//        player.collectedList[indexInList] = newCollectable;

//        Destroy(gameObject);
//    }


//}



public class Collectable : MonoBehaviour
{
    public bool isCollected = false;
    public float collectDistance = 1f;
    public Transform followTarget;
    public float followSpeed = 7f;
    private bool isTip = false;
    public PlayerController player;

    public float value = 100f;
    public GameObject[] collectables; // 0 cash 1 gold 2 diamond
    private bool hasTransformed = false;

    void Update()
    {
        FollowTarget();
    }

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
        if (!isCollected || followTarget == null) return;

        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void SetTip(bool value)
    {
        isTip = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected) return;

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

        if (isTip && other.CompareTag("Transformer") && !hasTransformed)
        {
            hasTransformed = true;
            Transform(this);        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Transformer"))
        {
            hasTransformed = false;
        }
    }

    public void Transform(Collectable collectable)
    {
        int indexInList = player.collectedList.IndexOf(this);
        if (indexInList == -1) return;

        GameObject newObject = null;

        if (CompareTag("Cash"))
            newObject = Instantiate(collectables[1], transform.position, transform.rotation);
        else if (CompareTag("Gold"))
            newObject = Instantiate(collectables[2], transform.position, transform.rotation);
        else if (CompareTag("Diamond"))
            return;

        if (newObject == null) return;

        newObject.transform.localScale = transform.localScale;
        newObject.tag = CompareTag("Cash") ? "Gold" : "Diamond";
        newObject.layer = LayerMask.NameToLayer("Collected");

        Collectable newCollectable = newObject.GetComponent<Collectable>();
        newCollectable.isCollected = true;
        newCollectable.player = player;
        newCollectable.followTarget = this.followTarget;
        newCollectable.collectDistance = this.collectDistance;
        newCollectable.SetTip(this.isTip);

        player.collectedList[indexInList] = newCollectable;

        Destroy(gameObject);
    }
}



