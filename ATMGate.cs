using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATMGate : MonoBehaviour
{
    [Header("ATM Settings")]
    public float depositDelay = 0.1f;

    private HashSet<Collectable> depositedCollectables = new HashSet<Collectable>();
    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.collectedList.Count > 0)
        {
            StartCoroutine(DepositCollectables(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            depositedCollectables.Clear();
            isProcessing = false;
        }
    }

    private IEnumerator DepositCollectables(PlayerController player)
    {
        isProcessing = true;

        List<Collectable> collectablesToDeposit = new List<Collectable>(player.collectedList);

        float totalDeposited = 0f;
        int countDeposited = 0;

        foreach (Collectable collectable in collectablesToDeposit)
        {
            if (depositedCollectables.Contains(collectable)) continue;

            totalDeposited += collectable.value;
            countDeposited++;
            depositedCollectables.Add(collectable);

            player.DepositFromCollectable(collectable);

            yield return new WaitForSeconds(depositDelay);
        }

        if (countDeposited > 0)
        {
            Debug.Log($"Deposited {countDeposited} items worth ${totalDeposited:F0}");
        }

        yield return new WaitForSeconds(0.5f);
        isProcessing = false;
    }
}