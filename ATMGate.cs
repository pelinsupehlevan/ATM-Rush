using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATMGate : MonoBehaviour
{
    private HashSet<Collectable> depositedCollectables = new HashSet<Collectable>();

    public void DepositIndividualCollectable(Collectable collectable)
    {
        if (depositedCollectables.Contains(collectable)) return;

        depositedCollectables.Add(collectable);

        // Find the player
        PlayerController player = collectable.FindPlayerInChain();
        if (player != null)
        {
            StartCoroutine(DepositAndDestroy(collectable, player));
        }
    }

    private IEnumerator DepositAndDestroy(Collectable collectable, PlayerController player)
    {
        // Add money to player
        player.moneyAmount += collectable.value;
        Debug.Log($"Deposited ${collectable.value:F0}. Total Money: ${player.moneyAmount:F0}");

        // Remove from player's list and update chain
        player.RemoveCollectableFromChain(collectable);

        // Destroy the collectable (it gets "eaten" by the ATM)
        Destroy(collectable.gameObject);

        yield return null;
    }

    private void OnTriggerExit(Collider other)
    {
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && depositedCollectables.Contains(collectable))
        {
            depositedCollectables.Remove(collectable);
        }
    }
}