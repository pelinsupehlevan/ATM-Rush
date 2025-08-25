using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    public float conveyorSpeed = 3f;
    public Transform endPoint;
    public float collectableSpacing = 1f;
    public Transform playerTargetPosition;
    public float playerMoveSpeed = 2f;

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
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && collectable.isCollected && !isActive)
        {
            PlayerController player = collectable.FindPlayerInChain();
            if (player != null && player.collectedList.Contains(collectable))
            {
                StartCoroutine(ProcessCollectablesFlow(player));
            }
        }
    }

    private IEnumerator ProcessCollectablesFlow(PlayerController player)
    {
        isActive = true;
        player.enabled = false;

        List<Collectable> collectables = new List<Collectable>(player.collectedList);
        collectables.Reverse();

        float totalLevelMoney = 0f;
        int totalCount = 0;

        List<Coroutine> conveyorCoroutines = new List<Coroutine>();

        for (int i = 0; i < collectables.Count; i++)
        {
            Collectable c = collectables[i];

            Vector3 targetPos = endPoint.position + Vector3.right * i * collectableSpacing;

            yield return StartCoroutine(MoveToConveyor(c, targetPos));

            totalLevelMoney += c.value;
            totalCount++;

            conveyorCoroutines.Add(StartCoroutine(MoveAlongConveyor(c, targetPos)));

            yield return new WaitForSeconds(0.05f);
        }

        if (playerTargetPosition != null)
            yield return StartCoroutine(MovePlayerToCenter(player));

        foreach (var c in conveyorCoroutines)
            yield return c;

        levelManager?.LevelFinished(totalLevelMoney, totalCount);
        isActive = false;
    }

    private IEnumerator MoveToConveyor(Collectable c, Vector3 target)
    {
        Vector3 start = c.transform.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            c.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        c.transform.position = target;
    }

    private IEnumerator MoveAlongConveyor(Collectable c, Vector3 endPos)
    {
        Vector3 start = c.transform.position;
        float distance = Vector3.Distance(start, endPos);
        float duration = distance / conveyorSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            c.transform.position = Vector3.Lerp(start, endPos, t);
            yield return null;
        }

        c.transform.position = endPos;

        PlayerController player = c.FindPlayerInChain();
        if (player != null)
            player.RemoveCollectableFromChain(c);

        Destroy(c.gameObject);
    }

    private IEnumerator MovePlayerToCenter(PlayerController player)
    {
        Vector3 start = player.transform.position;
        Vector3 target = playerTargetPosition.position;

        float duration = Vector3.Distance(start, target) / playerMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            player.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        player.transform.position = target;
    }
}
