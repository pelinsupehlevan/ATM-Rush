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

    [Header("Money System")]
    public float permanentMoney = 0f;    
    public float inGameMoney = 0f;       
    public float depositedThisLevel = 0f; 

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
        UpdateInGameMoney();
    }

    private void Update()
    {
        HandleSwipeInput();
        HandleMovement();
        UpdateInGameMoney(); 
    }

    private void UpdateInGameMoney()
    {
        float currentCollectionValue = 0f;
        foreach (Collectable collectable in collectedList)
        {
            currentCollectionValue += collectable.value;
        }
        inGameMoney = currentCollectionValue + depositedThisLevel;
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

        Debug.Log($"[PLAYER] OnTriggerEnter with {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, Tag: {other.tag}");

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") &&
            !other.CompareTag("ATM") && !other.CompareTag("Transformer"))
        {
            StartCoroutine(Recover());
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Collectable") && collectedList.Count == 0)
        {
            Collectable collectable = other.GetComponent<Collectable>();
            if (collectable != null && !collectable.isCollected)
            {
                Debug.Log($"[PLAYER] Player collecting {collectable.name} (empty chain)");
                collectable.Collect(this);
            }
        }
    }

    public void Collect(Collectable newCollectable)
    {
        if (collectedList.Contains(newCollectable))
        {
            Debug.LogWarning($"[PLAYER] Trying to collect {newCollectable.name} but it's already in the list!");
            return;
        }

        Debug.Log($"[PLAYER] Adding {newCollectable.name} to collection. List size before: {collectedList.Count}");

        collectedList.Add(newCollectable);

        Transform target;
        if (collectedList.Count == 1)
        {
            target = this.transform;
            newCollectable.collectDistance = 2f;
            Debug.Log($"[PLAYER] {newCollectable.name} is first in chain, following player");
        }
        else
        {
            target = collectedList[collectedList.Count - 2].transform;
            newCollectable.collectDistance = 1f;
            Debug.Log($"[PLAYER] {newCollectable.name} following {collectedList[collectedList.Count - 2].name}");
        }

        newCollectable.followTarget = target;

        Debug.Log($"[PLAYER] Collected {newCollectable.name} ({newCollectable.type}). Total collected: {collectedList.Count}");

        for (int i = 0; i < collectedList.Count; i++)
        {
            Debug.Log($"[PLAYER] Chain[{i}]: {collectedList[i].name} (ID: {collectedList[i].GetInstanceID()})");
        }
    }

    public void RemoveCollectableFromChain(Collectable collectable)
    {
        if (!collectedList.Contains(collectable)) return;

        int index = collectedList.IndexOf(collectable);
        Debug.Log($"[PLAYER] Removing {collectable.name} from chain at index {index}");

        if (index < collectedList.Count - 1)
        {
            Collectable nextCollectable = collectedList[index + 1];
            if (index > 0)
            {
                nextCollectable.followTarget = collectedList[index - 1].transform;
            }
            else
            {
                nextCollectable.followTarget = this.transform;
                nextCollectable.collectDistance = 2f;
            }
        }

        collectedList.RemoveAt(index);

        UpdateCollectableDistances();
    }

    private void UpdateCollectableDistances()
    {
        for (int i = 0; i < collectedList.Count; i++)
        {
            if (i == 0)
            {
                collectedList[i].collectDistance = 2f; 
            }
            else
            {
                collectedList[i].collectDistance = 1f; 
            }
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
            depositedThisLevel += col.value;
            Destroy(col.gameObject);
        }

        collectedList.RemoveRange(index, collectedList.Count - index);

        Debug.Log($"[PLAYER] Deposited money. InGame: ${inGameMoney}, DepositedThisLevel: ${depositedThisLevel}");
    }

    public void CompleteLevel()
    {
        permanentMoney += inGameMoney;
        inGameMoney = 0f;
        depositedThisLevel = 0f;

        Debug.Log($"[PLAYER] Level completed. New permanent money: ${permanentMoney}");
    }

    public void StartNewLevel()
    {
        inGameMoney = 0f;
        depositedThisLevel = 0f;
        collectedList.Clear();
    }
}