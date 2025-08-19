using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    private HashSet<Collectable> transformedCollectables = new HashSet<Collectable>();

    public void TransformIndividualCollectable(Collectable collectable)
    {
        if (transformedCollectables.Contains(collectable)) return;

        GameObject newPrefab = null;
        CollectableType newType = collectable.type;

        // Determine what to transform to
        if (collectable.type == CollectableType.Cash && goldPrefab != null)
        {
            newPrefab = goldPrefab;
            newType = CollectableType.Gold;
        }
        else if (collectable.type == CollectableType.Gold && diamondPrefab != null)
        {
            newPrefab = diamondPrefab;
            newType = CollectableType.Diamond;
        }

        if (newPrefab != null)
        {
            StartCoroutine(ReplaceCollectable(collectable, newPrefab, newType));
        }
    }

    private IEnumerator ReplaceCollectable(Collectable oldCollectable, GameObject newPrefab, CollectableType newType)
    {
        // Find the player
        PlayerController player = oldCollectable.FindPlayerInChain();
        if (player == null) yield break;

        // Create new collectable
        GameObject newObj = Instantiate(newPrefab, oldCollectable.transform.position, oldCollectable.transform.rotation);
        Collectable newCollectable = newObj.GetComponent<Collectable>();

        if (newCollectable != null)
        {
            // Copy states
            newCollectable.isCollected = true;
            newCollectable.followTarget = oldCollectable.followTarget;
            newCollectable.collectDistance = oldCollectable.collectDistance;
            newCollectable.type = newType;
            newCollectable.gameObject.layer = LayerMask.NameToLayer("Default");

            // Handle tip indicator
            bool wasTip = oldCollectable.tipIndicator != null && oldCollectable.tipIndicator.activeInHierarchy;
            newCollectable.SetTip(wasTip);

            // Replace in player's list
            int index = player.collectedList.IndexOf(oldCollectable);
            if (index >= 0)
            {
                player.collectedList[index] = newCollectable;

                // Update follow targets for collectables behind this one
                for (int j = index + 1; j < player.collectedList.Count; j++)
                {
                    if (j == index + 1)
                    {
                        player.collectedList[j].followTarget = newCollectable.transform;
                    }
                }
            }

            transformedCollectables.Add(newCollectable);
        }

        // Destroy old collectable
        Destroy(oldCollectable.gameObject);

        yield return null;
    }

    private void OnTriggerExit(Collider other)
    {
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && transformedCollectables.Contains(collectable))
        {
            transformedCollectables.Remove(collectable);
        }
    }
}