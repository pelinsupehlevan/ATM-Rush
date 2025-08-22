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

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private Vector2 currentPosition;
    private float currentX;
    private float targetX;
    private float deltaX;

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

        Vector3 forward = transform.forward * speed;

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

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") &&
            !other.CompareTag("ATM") && !other.CompareTag("Transformer"))
        {
            StartCoroutine(Recover());
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            Collectable collectable = other.GetComponent<Collectable>();
            if (collectable != null && !collectable.isCollected)
            {
                collectable.Collect(this);
            }
        }
    }


    //public void Collect(Collectable newCollectable)
    //{
    //    if (collectedList.Contains(newCollectable))
    //    {
    //        Debug.LogWarning($"[Player] Trying to collect {newCollectable.name} but it's already in the list!");
    //        return;
    //    }

    //    Debug.Log($"[Player] Collecting {newCollectable.name}. List size before: {collectedList.Count}");

    //    collectedList.Add(newCollectable);

    //    Transform target;
    //    if (collectedList.Count == 1)
    //    {
    //        target = this.transform;
    //        newCollectable.collectDistance = 2f;
    //        Debug.Log($"[Player] {newCollectable.name} is first in chain, following player");
    //    }
    //    else
    //    {
    //        target = collectedList[collectedList.Count - 2].transform;
    //        newCollectable.collectDistance = 1f;
    //        Debug.Log($"[Player] {newCollectable.name} following {collectedList[collectedList.Count - 2].name}");
    //    }

    //    newCollectable.followTarget = target;

    //    Debug.Log($"[Player] List size after: {collectedList.Count}");

    //    for (int i = 0; i < collectedList.Count; i++)
    //    {
    //        Debug.Log($"[Player] Index {i}: {collectedList[i].name} (ID: {collectedList[i].GetInstanceID()})");
    //    }
    //}

    public void RemoveCollectableFromChain(Collectable collectable)
    {
        if (!collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);

        if (index < collectedList.Count - 1)
        {
            Collectable nextCollectable = collectedList[index + 1];
            nextCollectable.followTarget = collectable.followTarget;
        }

        collectedList.RemoveAt(index);
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
            col.gameObject.layer = LayerMask.NameToLayer("Collectable");

            Vector3 dropOffset = new Vector3(Random.Range(minX, maxX),
                                             transform.position.y,
                                             transform.position.z + Random.Range(8f, 10f));
            col.transform.position = dropOffset;
        }

        collectedList.RemoveRange(index, collectedList.Count - index);
    }

    public void DestroyFromCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        List<Collectable> toDestroy = collectedList.GetRange(index, collectedList.Count - index);

        foreach (Collectable col in toDestroy)
        {
            Destroy(col.gameObject);
        }

        collectedList.RemoveRange(index, collectedList.Count - index);
    }

    public void DepositFromCollectable(Collectable collectable)
    {
        if (collectable == null || !collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        List<Collectable> toDeposit = collectedList.GetRange(index, collectedList.Count - index);

        foreach (Collectable col in toDeposit)
        {
            moneyAmount += col.value;
            Destroy(col.gameObject);
        }

        collectedList.RemoveRange(index, collectedList.Count - index);

        Debug.Log("Current Money: " + moneyAmount);
    }

    // In your PlayerController script, add these methods:

    public void Collect(Collectable collectable)
    {
        if (collectedList.Count == 0)
        {
            // First collectable follows player directly
            collectable.Collect(this, transform);
        }
        else
        {
            // Subsequent collectables follow the last collected item
            Collectable lastCollectable = collectedList[collectedList.Count - 1];
            collectable.Collect(this, lastCollectable.transform);
        }

        collectedList.Add(collectable);
    }

    // Add methods to handle dropping, destroying, and depositing collectables
    public void DropCollectable(Collectable collectable)
    {
        if (collectedList.Contains(collectable))
        {
            collectedList.Remove(collectable);
            // Reset the collectable
            collectable.isCollected = false;
            collectable.followTarget = null;
            collectable.gameObject.layer = LayerMask.NameToLayer("Collectable");

            // Update the following targets for remaining collectables
            UpdateCollectableChain();
        }
    }

    private void UpdateCollectableChain()
    {
        if (collectedList.Count == 0) return;

        // First collectable follows player
        collectedList[0].followTarget = transform;

        // Subsequent collectables follow the previous one
        for (int i = 1; i < collectedList.Count; i++)
        {
            collectedList[i].followTarget = collectedList[i - 1].transform;
        }
    }
}