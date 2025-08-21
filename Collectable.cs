using System.Collections;
using UnityEngine;

public enum CollectableType
{
    Cash,
    Gold,
    Diamond
}

public class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    public CollectableType type = CollectableType.Cash;
    public float value = 1f;
    public bool isCollected = false;

    [Header("Following Behavior")]
    public Transform followTarget;
    public float collectDistance = 2f;
    public float followSpeed = 8f;
    public float rotationSpeed = 90f;

    private Vector3 followOffset;
    public bool transformLock = false;

    void Start()
    {
        // Set initial values based on type
        switch (type)
        {
            case CollectableType.Cash:
                value = 1f;
                break;
            case CollectableType.Gold:
                value = 5f;
                break;
            case CollectableType.Diamond:
                value = 10f;
                break;
        }
    }

    void Update()
    {
        if (isCollected && followTarget != null)
        {
            FollowTarget();
        }
    }

    //private void FollowTarget()
    //{
    //    //Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
    //    //targetPosition.y = transform.position.y;

    //    //transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

    //    Vector3 targetPosition = followTarget.position + followTarget.forward * collectDistance;
    //    targetPosition.y = followTarget.position.y; // Keep same height as target
    //    transform.position = targetPosition;

    //    // Optional: match rotation exactly
    //}

    private void FollowTarget()
    {
        if (followTarget == null) return;

        // Snap directly in front along world Z axis, ignoring X/Y rotation
        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
        targetPosition.y = followTarget.position.y; // Keep same height as target
        transform.position = targetPosition;

        // Match rotation exactly (optional)
        transform.rotation = followTarget.rotation;
    }


    public void Collect(PlayerController player)
    {
        if (isCollected) return;

        isCollected = true;
        gameObject.layer = LayerMask.NameToLayer("Default");
        player.Collect(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected) return;

        PlayerController player = FindPlayerInChain();
        if (player == null) return;

        // --- Collect new money if this is the tip ---
        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable newCollectable = other.GetComponent<Collectable>();
            if (newCollectable != null && !newCollectable.isCollected)
            {
                newCollectable.Collect(player);
            }
        }

        //// --- Handle Transformer gate ---
        //if (other.CompareTag("Transformer"))
        //{
        //    TransformerGate gate = other.GetComponent<TransformerGate>();
        //    if (gate != null)
        //    {
        //        gate.TransformIndividualCollectable(this);
        //    }
        //}

        // --- Handle Transformer gate ---
        if (other.CompareTag("Transformer"))
        {
            var gate = other.GetComponent<TransformerGate>();
            if (gate != null)
            {
                // DEBUG
                Debug.Log($"[Collectable:{name} id={GetInstanceID()} type={type} t={Time.time:F3}s] Calling Transform on gate {gate.name}.");
                gate.TransformIndividualCollectable(this);
            }
            else
            {
                Debug.LogWarning($"[Collectable:{name}] Transformer tag found but no TransformerGate component on {other.name}.");
            }
        }


        // --- Handle ATM gate ---
        if (other.CompareTag("ATM"))
        {
            ATMGate atm = other.GetComponent<ATMGate>();
            if (atm != null)
            {
                atm.DepositIndividualCollectable(this);
            }
        }

        // --- Handle obstacles ---
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






    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Transformer"))
        {
            transformLock = false; // reset lock when leaving transformer
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
