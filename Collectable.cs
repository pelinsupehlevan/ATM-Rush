using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    public CollectableType type = CollectableType.Cash;
    public float value = 1f;
    public bool isCollected = false;

    [Header("Mesh References")]
    public GameObject cashMesh;
    public GameObject goldMesh;
    public GameObject diamondMesh;

    [Header("Following Behavior")]
    public Transform followTarget;
    public float collectDistance = 2f;
    public float followSpeed = 8f;

    private Vector3 followOffset;
    private bool isProcessing = false;

    void Start()
    {
        UpdateCollectableType(type);
    }

    public void UpdateCollectableType(CollectableType newType)
    {
        type = newType;

        if (cashMesh != null) cashMesh.SetActive(false);
        if (cashMesh != null) cashMesh.SetActive(false);
        if (goldMesh != null) goldMesh.SetActive(false);
        if (diamondMesh != null) diamondMesh.SetActive(false);

        switch (type)
        {
            case CollectableType.Cash:
                value = 1f;
                if (cashMesh != null) cashMesh.SetActive(true);
                break;
            case CollectableType.Gold:
                value = 5f;
                if (goldMesh != null) goldMesh.SetActive(true);
                break;
            case CollectableType.Diamond:
                value = 10f;
                if (diamondMesh != null) diamondMesh.SetActive(true);
                break;
        }
    }

    // New method to handle transformation
    public void TryTransform()
    {
        if (type == CollectableType.Diamond) return;

        CollectableType newType = type == CollectableType.Cash ?
            CollectableType.Gold : CollectableType.Diamond;

        UpdateCollectableType(newType);
    }

    void Update()
    {
        if (isCollected && followTarget != null)
        {
            FollowTarget();
        }
    }

    private void FollowTarget()
    {
        if (followTarget == null) return;

        Vector3 targetPosition = followTarget.position + followTarget.forward * collectDistance;
        targetPosition.y = followTarget.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = followTarget.rotation;
    }

    public void Collect(PlayerController player)
    {
        if (isCollected || isProcessing) return;

        isProcessing = true;
        isCollected = true;
        gameObject.layer = LayerMask.NameToLayer("Collected");

        // Add to player's collection - using your existing method
        player.Collect(this);

        isProcessing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected || isProcessing) return;

        // Only the tip collectable can collect new money
        PlayerController player = FindPlayerInChain();
        if (player != null && player.collectedList.Count > 0 && this == player.collectedList[0])
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
            {
                Collectable newCollectable = other.GetComponent<Collectable>();
                if (newCollectable != null && !newCollectable.isCollected && newCollectable != this)
                {
                    newCollectable.Collect(player);
                }
            }
        }

        // Handle transformer gate
        if (other.CompareTag("Transformer"))
        {
            TryTransform();
        }

        // Handle ATM gate - using your existing method name
        if (other.CompareTag("ATM"))
        {
            ATMGate atm = other.GetComponent<ATMGate>();
            if (atm != null)
            {
                atm.DepositIndividualCollectable(this);
            }
        }

        // Handle other interactions using your existing methods
        if (player != null)
        {
            switch (other.tag)
            {
                case "Drop":
                    player.DropFromCollectable(this);
                    break;
                case "Destroy":
                    player.DestroyFromCollectable(this);
                    break;
                case "Deposit":
                    player.DepositFromCollectable(this);
                    break;
            }
        }
    }

    // Your existing method
    public PlayerController FindPlayerInChain()
    {
        Transform current = followTarget;
        while (current != null)
        {
            PlayerController player = current.GetComponent<PlayerController>();
            if (player != null) return player;

            Collectable collectable = current.GetComponent<Collectable>();
            if (collectable != null)
            {
                current = collectable.followTarget;
            }
            else break;
        }
        return null;
    }
}