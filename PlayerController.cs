using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float minX = -2.8f;
    public float maxX = 2.8f;
    public float sensitivity = 0.01f;
    public float smoothingFactor = 0.1f;

    [Header("Recovery")]
    public float recoveryTime = 0.2f;
    public float backDistance = 3f;
    public LayerMask obstacleLayer;

    [Header("Collections")]
    public List<Collectable> collectedList = new List<Collectable>();
    public float moneyAmount = 0;

    // Private variables for movement
    private Vector2 startPosition;
    private Vector2 lastPosition;
    private Vector2 currentPosition;
    private float currentX;
    private float targetX;
    private float deltaX;

    // State variables
    private bool isRunning = false;
    private bool isSliding = false;
    private bool isRecovering = false;

    void Start()
    {
        currentX = transform.position.x;
        targetX = transform.position.x;
    }

    private void Update()
    {
        HandleSwipeInput();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!isRunning || isRecovering) { return; }

        // Forward movement
        Vector3 forward = transform.forward * speed;

        // Horizontal sliding
        if (isSliding)
        {
            currentX = Mathf.Lerp(currentX, targetX, smoothingFactor);
        }
        else
        {
            currentX = transform.position.x;
            targetX = transform.position.x;
        }

        float offset = currentX - transform.position.x;
        Vector3 horizontal = Vector3.right * offset;

        Vector3 totalMovement = forward + horizontal;
        transform.position += totalMovement * Time.deltaTime;
    }

    private void HandleSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            lastPosition = startPosition;
            isSliding = true;
        }

        if (Input.GetMouseButton(0) && isSliding)
        {
            currentPosition = Input.mousePosition;
            deltaX = currentPosition.x - lastPosition.x;

            float horizontalMovement = deltaX * sensitivity;
            targetX += horizontalMovement;
            targetX = Mathf.Clamp(targetX, minX, maxX);

            lastPosition = currentPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSliding = false;
        }
    }

    private IEnumerator Recover()
    {
        isRecovering = true;

        Vector3 target = transform.position - transform.forward * backDistance;
        Vector3 start = transform.position;

        float bounceTime = 0.5f;
        float timer = 0f;

        while (timer < bounceTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, timer / bounceTime);
            yield return null;
        }

        transform.position = target;
        yield return new WaitForSeconds(recoveryTime);
        isRecovering = false;
    }

    public void Run()
    {
        isRunning = true;
    }

    public void StopRunning()
    {
        isRunning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRecovering) return;

        // Handle obstacles (but not ATM and Transformer gates)
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") &&
            !other.CompareTag("ATM") && !other.CompareTag("Transformer"))
        {
            StartCoroutine(Recover());
        }

        // Player can also collect directly (fallback)
        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable collectable = other.GetComponent<Collectable>();
            if (collectable != null && !collectable.isCollected)
            {
                collectable.Collect(this);
            }
        }
    }

    public void Collect(Collectable newCollectable)
    {
        // Remove tip from previous last collectable
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(false);
        }

        collectedList.Add(newCollectable);

        // Set follow target and distance
        Transform target;
        if (collectedList.Count == 1)
        {
            target = this.transform;
            newCollectable.collectDistance = 2f;
        }
        else
        {
            target = collectedList[collectedList.Count - 2].transform;
            newCollectable.collectDistance = 1f;
        }

        newCollectable.followTarget = target;
        newCollectable.SetTip(true); // New collectable becomes the tip
    }

    public void RemoveCollectableFromChain(Collectable collectable)
    {
        if (!collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);

        // If removing from the middle or start of chain, update follow targets
        if (index < collectedList.Count - 1)
        {
            Collectable nextCollectable = collectedList[index + 1];
            nextCollectable.followTarget = collectable.followTarget;
        }

        // Remove from list
        collectedList.RemoveAt(index);

        // Update tip indicator
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
    }

    public void DropFromCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        List<Collectable> toDrop = collectedList.GetRange(index, collectedList.Count - index);

        foreach (Collectable col in toDrop)
        {
            col.isCollected = false;
            col.followTarget = null;
            col.SetTip(false);
            col.gameObject.layer = LayerMask.NameToLayer("Collectable");

            Vector3 dropOffset = new Vector3(Random.Range(minX, maxX),
                                             transform.position.y,
                                             transform.position.z + Random.Range(8f, 10f));
            col.transform.position = dropOffset;
        }

        collectedList.RemoveRange(index, collectedList.Count - index);

        // Update tip
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
    }

    public void DestroyFromCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        List<Collectable> toDestroy = collectedList.GetRange(index, collectedList.Count - index);

        foreach (Collectable col in toDestroy)
        {
            col.SetTip(false);
            Destroy(col.gameObject);
        }

        collectedList.RemoveRange(index, collectedList.Count - index);

        // Update tip
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }
    }

    public void DepositFromCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        List<Collectable> toDeposit = collectedList.GetRange(index, collectedList.Count - index);

        foreach (Collectable col in toDeposit)
        {
            col.SetTip(false);
            moneyAmount += col.value;
            Destroy(col.gameObject);
        }

        collectedList.RemoveRange(index, collectedList.Count - index);

        // Update tip
        if (collectedList.Count > 0)
        {
            collectedList[collectedList.Count - 1].SetTip(true);
        }

        Debug.Log("Current Money: " + moneyAmount);
    }
}