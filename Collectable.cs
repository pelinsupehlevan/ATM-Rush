using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{

    public bool isCollected = false;

    public float collectDistance = 1f;
    public Transform followTarget;
    public float followSpeed = 7f;
    private bool isTip = false;
    private PlayerController player;

    public float value = 100f;
    public GameObject[] collectables; // 0 coin 1 gold 2 diamond


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        if (!isCollected || !isTip) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
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

        if (isTip && other.CompareTag("Transformer"))
        {
            Transform();
        }

    }

    public void Transform()
    {
        if (CompareTag("Cash")) {
            GameObject newCollectable = Instantiate(collectables[1], transform.position, Quaternion.identity);
            newCollectable.transform.localScale = transform.localScale;
            Destroy(gameObject);
            player.Collect(newCollectable.GetComponent<Collectable>());
        }

        if (CompareTag("Gold")) {
            GameObject newCollectable = Instantiate(collectables[2], transform.position, Quaternion.identity);
            newCollectable.transform.localScale = transform.localScale;
            Destroy(gameObject);
            player.Collect(newCollectable.GetComponent<Collectable>());

        }

        if (CompareTag("Diamond")) return;
    }


}

