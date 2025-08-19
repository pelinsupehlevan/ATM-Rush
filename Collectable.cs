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

    [Header("Visual Elements")]
    public GameObject tipIndicator;

    private Vector3 followOffset;

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

    private void FollowTarget()
    {
        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void Collect(PlayerController player)
    {
        if (isCollected) return;

        isCollected = true;
        gameObject.layer = LayerMask.NameToLayer("Default");
        player.Collect(this);
    }

    public void SetTip(bool isTip)
    {
        if (tipIndicator != null)
        {
            tipIndicator.SetActive(isTip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the tip collectable should collect new items
        if (isCollected && IsTip())
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
            {
                Collectable newCollectable = other.GetComponent<Collectable>();
                if (newCollectable != null && !newCollectable.isCollected)
                {
                    // Find the player through the chain
                    PlayerController player = FindPlayerInChain();
                    if (player != null)
                    {
                        newCollectable.Collect(player);
                    }
                }
            }
        }

        // Handle individual transformation
        if (isCollected && other.CompareTag("Transformer"))
        {
            TransformerGate gate = other.GetComponent<TransformerGate>();
            if (gate != null)
            {
                gate.TransformIndividualCollectable(this);
            }
        }

        // Handle individual deposit
        if (isCollected && other.CompareTag("ATM"))
        {
            ATMGate atm = other.GetComponent<ATMGate>();
            if (atm != null)
            {
                atm.DepositIndividualCollectable(this);
            }
        }
    }

    private bool IsTip()
    {
        return tipIndicator != null && tipIndicator.activeInHierarchy;
    }

    public PlayerController FindPlayerInChain()
    {
        Transform current = followTarget;
        while (current != null)
        {
            PlayerController player = current.GetComponent<PlayerController>();
            if (player != null)
            {
                return player;
            }

            Collectable collectable = current.GetComponent<Collectable>();
            if (collectable != null)
            {
                current = collectable.followTarget;
            }
            else
            {
                break;
            }
        }
        return null;
    }
}