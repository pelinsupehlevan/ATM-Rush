using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public float conveyorSpeed = 3f;
    public Transform startPoint;
    public Transform endPoint;
    public float collectableSpacing = 1f;

    [Header("Counting")]
    public float countDelay = 0.5f;

    private bool isActive = false;
    private UIManager uiManager;
    private LevelManager levelManager;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !isActive && player.collectedList.Count > 0)
        {
            startPoint = player.transform;
            StartCoroutine(ProcessCollectables(player));
        }
    }

    private IEnumerator ProcessCollectables(PlayerController player)
    {
        isActive = true;

        // Stop player movement
        player.enabled = false;

        // Get all collectables and reverse order (process from tip backwards)
        List<Collectable> collectables = new List<Collectable>(player.collectedList);
        collectables.Reverse();

        float totalMoney = 0f;
        int totalCount = 0;

        foreach (Collectable collectable in collectables)
        {
            // Move collectable to conveyor start
            yield return StartCoroutine(MoveToConveyor(collectable));

            // Move along conveyor
            yield return StartCoroutine(MoveAlongConveyor(collectable));

            // Add to totals
            totalMoney += collectable.value;
            totalCount++;

            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateMoneyCount(totalMoney);
                uiManager.UpdateCollectableCount(totalCount);
            }

            // Destroy the collectable
            Destroy(collectable.gameObject);

            // Wait before processing next
            yield return new WaitForSeconds(countDelay);
        }

        // Clear player's list and add money
        player.collectedList.Clear();
        player.moneyAmount += totalMoney;

        // Complete the level
        if (levelManager != null)
        {
            levelManager.CompleteLevel(totalMoney, totalCount);
        }

        yield return new WaitForSeconds(1f);

        isActive = false;
    }

    private IEnumerator MoveToConveyor(Collectable collectable)
    {
        Vector3 startPos = collectable.transform.position;
        Vector3 targetPos = startPoint.position;

        // Rotate the collectable 90 degrees
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
        Vector3 startPos = startPoint.position;
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