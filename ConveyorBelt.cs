using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public float conveyorSpeed = 3f;
    public Transform endPoint; 
    public float collectableSpacing = 1f;

    [Header("Counting")]
    public float countDelay = 0.5f;

    [Header("Player Positioning")]
    public Transform playerTargetPosition; 
    public float playerMoveSpeed = 2f;

    private bool isActive = false;
    private UIManager uiManager;
    private LevelManager levelManager;
    private Vector3 dynamicStartPoint;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CONVEYOR] OnTriggerEnter with {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        Collectable tipCollectable = other.GetComponent<Collectable>();
        if (tipCollectable != null && tipCollectable.isCollected && !isActive)
        {
            PlayerController player = tipCollectable.FindPlayerInChain();
            if (player != null && player.collectedList.Count > 0 &&
                tipCollectable == player.collectedList[player.collectedList.Count - 1])
            {
                Debug.Log($"[CONVEYOR] Chain tip {tipCollectable.name} hit conveyor, starting processing");
                dynamicStartPoint = tipCollectable.transform.position;
                StartCoroutine(ProcessCollectables(player));
            }
            else
            {
                Debug.Log($"[CONVEYOR] Collectable {tipCollectable.name} is not chain tip or no player found");
            }
        }
        else
        {
            Debug.Log($"[CONVEYOR] Not a valid collectable for processing");
        }
    }

    private IEnumerator ProcessCollectables(PlayerController player)
    {
        isActive = true;

        player.enabled = false;

        if (playerTargetPosition != null)
        {
            StartCoroutine(MovePlayerToCenter(player));
        }

        // Get all collectables and reverse order (process from tip backwards)
        List<Collectable> collectables = new List<Collectable>(player.collectedList);
        collectables.Reverse();

        float totalLevelMoney = 0f;
        int totalCount = 0;

        foreach (Collectable collectable in collectables)
        {
            yield return StartCoroutine(MoveToConveyor(collectable));

            yield return StartCoroutine(MoveAlongConveyor(collectable));

            totalLevelMoney += collectable.value;
            totalCount++;

            if (uiManager != null)
            {
                uiManager.UpdateLevelMoney(totalLevelMoney);
                uiManager.UpdateCollectableCount(totalCount);
            }

            player.RemoveCollectableFromChain(collectable);

            Destroy(collectable.gameObject);

            //yield return new WaitForSeconds(countDelay);
        }

        player.CompleteLevel();

        if (levelManager != null)
        {
            levelManager.CompleteLevel(totalLevelMoney, totalCount);
        }

        yield return new WaitForSeconds(1f);

        isActive = false;
    }

    private IEnumerator MovePlayerToCenter(PlayerController player)
    {
        if (playerTargetPosition == null) yield break;

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = new Vector3(playerTargetPosition.position.x, startPos.y, startPos.z);

        float moveTime = Vector3.Distance(startPos, targetPos) / playerMoveSpeed;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        player.transform.position = targetPos;
    }

    private IEnumerator MoveToConveyor(Collectable collectable)
    {
        Vector3 startPos = collectable.transform.position;
        Vector3 targetPos = dynamicStartPoint;

        Quaternion startRot = collectable.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 90, 0);

        float moveTime = 1f;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            collectable.transform.position = Vector3.Lerp(startPos, targetPos, t);
            collectable.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        collectable.transform.position = targetPos;
        collectable.transform.rotation = targetRot;
    }

    private IEnumerator MoveAlongConveyor(Collectable collectable)
    {
        Vector3 startPos = dynamicStartPoint;
        Vector3 endPos = endPoint.position;

        float distance = Vector3.Distance(startPos, endPos);
        float moveTime = distance / conveyorSpeed;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            collectable.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        collectable.transform.position = endPos;
    }
}