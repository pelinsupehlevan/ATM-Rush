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
    private Queue<Collectable> collectablesOnBelt = new Queue<Collectable>();
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

        player.enabled = false;

        List<Collectable> collectables = new List<Collectable>(player.collectedList);
        collectables.Reverse();

        float totalMoney = 0f;
        int totalCount = 0;

        foreach (Collectable collectable in collectables)
        {
            yield return StartCoroutine(MoveToConveyor(collectable));

            yield return StartCoroutine(MoveAlongConveyor(collectable));

            totalMoney += collectable.value;
            totalCount++;

            if (uiManager != null)
            {
                uiManager.UpdateMoneyCount(totalMoney);
                uiManager.UpdateCollectableCount(totalCount);
            }

            Destroy(collectable.gameObject);

            yield return new WaitForSeconds(countDelay);
        }

        player.collectedList.Clear();

        player.moneyAmount += totalMoney;

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
        collectable.transform.rotation *= Quaternion.Euler(0, 90, 0); 

        float moveTime = 1f;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            collectable.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        collectable.transform.position = targetPos;
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