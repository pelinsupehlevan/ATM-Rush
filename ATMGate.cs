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

        PlayerController player = collectable.FindPlayerInChain();
        if (player != null)
        {
            StartCoroutine(DepositAndDestroy(collectable, player));
        }
    }

    private IEnumerator DepositAndDestroy(Collectable collectable, PlayerController player)
    {
        player.moneyAmount += collectable.value;
        Debug.Log($"Deposited ${collectable.value:F0}. Total Money: ${player.moneyAmount:F0}");

        player.RemoveCollectableFromChain(collectable);

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