using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    // Track which collectables are currently being transformed to avoid duplicates
    private HashSet<Collectable> transformedCollectables = new HashSet<Collectable>();

    public void TransformIndividualCollectable(Collectable collectable)
    {
        // Already transformed? Skip
        if (transformedCollectables.Contains(collectable)) return;

        GameObject newPrefab = null;
        CollectableType newType = collectable.type;

        // Determine the next type
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
        else
        {
            // Diamond or no prefab available, nothing to transform
            return;
        }

        // Mark as transforming
        transformedCollectables.Add(collectable);

        // Start coroutine to replace collectable
        StartCoroutine(ReplaceCollectable(collectable, newPrefab, newType));
    }

    private IEnumerator ReplaceCollectable(Collectable oldCollectable, GameObject newPrefab, CollectableType newType)
    {
        PlayerController player = oldCollectable.FindPlayerInChain();
        if (player == null) yield break;

        // Save old position and rotation
        Vector3 oldPosition = oldCollectable.transform.position;
        Quaternion oldRotation = oldCollectable.transform.rotation;

        // Instantiate new collectable at same position and rotation
        GameObject newObj = Instantiate(newPrefab, oldPosition, oldRotation);
        Collectable newCollectable = newObj.GetComponent<Collectable>();

        if (newCollectable != null)
        {
            // Preserve chain properties
            newCollectable.isCollected = true;
            newCollectable.followTarget = oldCollectable.followTarget;
            newCollectable.collectDistance = oldCollectable.collectDistance;
            newCollectable.type = newType;
            newCollectable.gameObject.layer = LayerMask.NameToLayer("Default");

            // Replace in player's collected list
            int index = player.collectedList.IndexOf(oldCollectable);
            if (index >= 0)
            {
                player.collectedList[index] = newCollectable;

                // Update follow targets for collectables following the old one
                for (int i = 0; i < player.collectedList.Count; i++)
                {
                    if (player.collectedList[i].followTarget == oldCollectable.transform)
                    {
                        player.collectedList[i].followTarget = newCollectable.transform;
                    }
                }
            }
        }

        // Destroy old collectable
        Destroy(oldCollectable.gameObject);

        yield return null;

        // Remove from transformed set
        transformedCollectables.Remove(oldCollectable);
    }

    private void OnTriggerExit(Collider other)
    {
        // Just in case, remove from the set
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null)
        {
            transformedCollectables.Remove(collectable);
        }
    }
}
