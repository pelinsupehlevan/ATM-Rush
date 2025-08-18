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
        //Vector3 targetPosition = followTarget.position - followTarget.forward * collectDistance;
        //targetPosition.y = transform.position.y;

        //transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        //// Rotate the collectable
        //transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        Vector3 targetPosition = followTarget.position + Vector3.forward * collectDistance;
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
}