using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum CollectableType
{
    Cash = 0,
    Gold = 1,
    Diamond = 2
}
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

    [Header("Chain Tip Collection")]
    public float chainTipCollectionRadius = 1.5f; 

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

    public void TryTransform()
    {
        Debug.Log($"[COLLECTABLE] TryTransform called on {gameObject.name}. Current type: {type}");

        if (type == CollectableType.Diamond)
        {
            Debug.Log($"[COLLECTABLE] {gameObject.name} is already Diamond, no transformation");
            return; 
        }

        CollectableType newType = type == CollectableType.Cash ?
            CollectableType.Gold : CollectableType.Diamond;

        Debug.Log($"[COLLECTABLE] Transforming {gameObject.name} from {type} to {newType}");
        UpdateCollectableType(newType);

        Debug.Log($"[COLLECTABLE] {gameObject.name} transformed to {newType}. New value: {value}");
    }

    public bool IsCashActive()
    {
        return cashMesh != null && cashMesh.activeSelf;
    }

    public bool IsGoldActive()
    {
        return goldMesh != null && goldMesh.activeSelf;
    }

    public bool IsDiamondActive()
    {
        return diamondMesh != null && diamondMesh.activeSelf;
    }

    public void Initialize(CollectableType startType)
    {
        UpdateCollectableType(startType);
    }

    void Update()
    {
        if (isCollected && followTarget != null)
        {
            FollowTarget();

            if (IsChainTip())
            {
                CheckAndCollectNearby();
            }
        }
    }

    private bool IsChainTip()
    {
        PlayerController player = FindPlayerInChain();
        return player != null && player.collectedList.Count > 0 &&
               this == player.collectedList[player.collectedList.Count - 1];
    }

    private void CheckAndCollectNearby()
    {
        LayerMask collectableMask = LayerMask.GetMask("Collectable");
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, chainTipCollectionRadius, collectableMask);

        foreach (Collider col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue; 

            Collectable nearbyCollectable = col.GetComponent<Collectable>();
            if (nearbyCollectable != null && !nearbyCollectable.isCollected && !nearbyCollectable.isProcessing)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Debug.Log($"[CHAIN TIP] {gameObject.name} found collectible {nearbyCollectable.name} at distance: {distance}");

                if (distance <= chainTipCollectionRadius)
                {
                    PlayerController player = FindPlayerInChain();
                    if (player != null)
                    {
                        Debug.Log($"[CHAIN TIP] {gameObject.name} collecting {nearbyCollectable.name}");
                        nearbyCollectable.Collect(player);
                    }
                }
            }
        }
    }

    private void FollowTarget()
    {
        if (followTarget == null) return;

        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void Collect(PlayerController player, Transform target)
    {
        if (isCollected || isProcessing) return;

        isProcessing = true;
        isCollected = true;

        gameObject.layer = LayerMask.NameToLayer("Collected");

        followTarget = target;

        isProcessing = false;

        Debug.Log($"{gameObject.name} collected by player. Type: {type}, Value: {value}");
    }

    public void Collect(PlayerController player)
    {
        if (isCollected || isProcessing) return;

        isProcessing = true;
        isCollected = true;
        gameObject.layer = LayerMask.NameToLayer("Collected");

        player.Collect(this);

        isProcessing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[COLLECTABLE] {gameObject.name} OnTriggerEnter with {other.gameObject.name}, IsCollected: {isCollected}, IsProcessing: {isProcessing}");

        if (!isCollected && !isProcessing)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.collectedList.Count == 0)
            {
                Debug.Log($"[COLLECTABLE] {gameObject.name} being collected by player (empty chain)");
                return; 
            }

            Collectable tipCollectable = other.GetComponent<Collectable>();
            if (tipCollectable != null && tipCollectable.isCollected)
            {
                PlayerController foundPlayer = tipCollectable.FindPlayerInChain();
                if (foundPlayer != null && foundPlayer.collectedList.Count > 0 &&
                    tipCollectable == foundPlayer.collectedList[foundPlayer.collectedList.Count - 1])
                {
                    Debug.Log($"[COLLECTABLE] {gameObject.name} being collected by chain tip {tipCollectable.name}");
                    this.Collect(foundPlayer);
                    return;
                }
            }
        }

        if (isCollected && !isProcessing)
        {
            // if (other.CompareTag("Transformer"))
            // {
            //     Debug.Log($"[COLLECTABLE] {gameObject.name} hit transformer gate. Current type: {type}");
            //     TryTransform();
            //     return;
            // }

            if (other.CompareTag("ATM"))
            {
                Debug.Log($"[COLLECTABLE] {gameObject.name} hit ATM gate");
                ATMGate atm = other.GetComponent<ATMGate>();
                if (atm != null)
                {
                    atm.DepositIndividualCollectable(this);
                }
                return;
            }

            PlayerController chainPlayer = FindPlayerInChain();
            if (chainPlayer != null)
            {
                switch (other.tag)
                {
                    case "Drop":
                        Debug.Log($"[COLLECTABLE] {gameObject.name} hit drop zone");
                        chainPlayer.DropFromCollectable(this);
                        break;
                    case "Destroy":
                        Debug.Log($"[COLLECTABLE] {gameObject.name} hit destroy zone");
                        chainPlayer.DestroyFromCollectable(this);
                        break;
                    case "Deposit":
                        Debug.Log($"[COLLECTABLE] {gameObject.name} hit deposit zone");
                        chainPlayer.DepositFromCollectable(this);
                        break;
                }
            }
        }
    }

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