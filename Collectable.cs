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
    public float chainTipCollectionRadius = 1.5f; // Increased radius for better detection

    private Vector3 followOffset;
    private bool isProcessing = false;

    void Start()
    {
        UpdateCollectableType(type);
    }

    public void UpdateCollectableType(CollectableType newType)
    {
        type = newType;

        // Deactivate all meshes first
        if (cashMesh != null) cashMesh.SetActive(false);
        if (goldMesh != null) goldMesh.SetActive(false);
        if (diamondMesh != null) diamondMesh.SetActive(false);

        // Activate the correct mesh and set value
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

    // Handle transformation through gates
    public void TryTransform()
    {
        Debug.Log($"[COLLECTABLE] TryTransform called on {gameObject.name}. Current type: {type}");

        if (type == CollectableType.Diamond)
        {
            Debug.Log($"[COLLECTABLE] {gameObject.name} is already Diamond, no transformation");
            return; // Already max level
        }

        CollectableType newType = type == CollectableType.Cash ?
            CollectableType.Gold : CollectableType.Diamond;

        Debug.Log($"[COLLECTABLE] Transforming {gameObject.name} from {type} to {newType}");
        UpdateCollectableType(newType);

        Debug.Log($"[COLLECTABLE] {gameObject.name} transformed to {newType}. New value: {value}");
    }

    // Helper method for level building - check what's currently active
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

    // Method to set initial type when spawning in level
    public void Initialize(CollectableType startType)
    {
        UpdateCollectableType(startType);
    }

    void Update()
    {
        if (isCollected && followTarget != null)
        {
            FollowTarget();

            // Chain tip collection logic
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
        // Use a larger radius and check both Collectable and Collected layers
        LayerMask collectableMask = LayerMask.GetMask("Collectable");
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, chainTipCollectionRadius, collectableMask);

        foreach (Collider col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue; // Skip self

            Collectable nearbyCollectable = col.GetComponent<Collectable>();
            if (nearbyCollectable != null && !nearbyCollectable.isCollected && !nearbyCollectable.isProcessing)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Debug.Log($"[CHAIN TIP] {gameObject.name} found collectible {nearbyCollectable.name} at distance: {distance}");

                // Collect if close enough
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

        // Change layer to prevent re-collection
        gameObject.layer = LayerMask.NameToLayer("Collected");

        // Set follow target
        followTarget = target;

        isProcessing = false;

        Debug.Log($"{gameObject.name} collected by player. Type: {type}, Value: {value}");
    }

    // Overload for backward compatibility with your existing PlayerController
    public void Collect(PlayerController player)
    {
        if (isCollected || isProcessing) return;

        isProcessing = true;
        isCollected = true;
        gameObject.layer = LayerMask.NameToLayer("Collected");

        // Add to player's collection
        player.Collect(this);

        isProcessing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[COLLECTABLE] {gameObject.name} OnTriggerEnter with {other.gameObject.name}, IsCollected: {isCollected}, IsProcessing: {isProcessing}");

        // Handle collection logic for uncollected items
        if (!isCollected && !isProcessing)
        {
            // Check if it's the player directly (only if player has empty chain)
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.collectedList.Count == 0)
            {
                Debug.Log($"[COLLECTABLE] {gameObject.name} being collected by player (empty chain)");
                return; // Let PlayerController handle this
            }

            // Check if it's a chain tip collectable
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

        // Handle gate interactions (ONLY for collected items)
        if (isCollected && !isProcessing)
        {
            // Handle transformer gate
            if (other.CompareTag("Transformer"))
            {
                Debug.Log($"[COLLECTABLE] {gameObject.name} hit transformer gate. Current type: {type}");
                TryTransform();
                return;
            }

            // Handle ATM gate
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

            // Handle other interactions
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

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (isCollected && IsChainTip())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, chainTipCollectionRadius);
        }
    }
}