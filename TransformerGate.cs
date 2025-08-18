using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    [Header("Gate Settings")]
    public float transformDelay = 0.1f;

    [Header("Prefab References")]
    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    private HashSet<Collectable> transformedCollectables = new HashSet<Collectable>();
    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.collectedList.Count > 0)
        {
            StartCoroutine(TransformCollectables(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            transformedCollectables.Clear();
            isProcessing = false;
        }
    }

    private IEnumerator TransformCollectables(PlayerController player)
    {
        isProcessing = true;

        List<Collectable> collectablesToTransform = new List<Collectable>(player.collectedList);

        for (int i = 0; i < collectablesToTransform.Count; i++)
        {
            Collectable collectable = collectablesToTransform[i];

            if (transformedCollectables.Contains(collectable)) continue;

            GameObject newPrefab = null;
            CollectableType newType = collectable.type;

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
                GameObject newObj = Instantiate(newPrefab, collectable.transform.position, collectable.transform.rotation);
                Collectable newCollectable = newObj.GetComponent<Collectable>();

                if (newCollectable != null)
                {
                    newCollectable.isCollected = true;
                    newCollectable.followTarget = collectable.followTarget;
                    newCollectable.collectDistance = collectable.collectDistance;
                    newCollectable.type = newType;
                    newCollectable.gameObject.layer = LayerMask.NameToLayer("Default");

                    bool wasTip = collectable.tipIndicator != null && collectable.tipIndicator.activeInHierarchy;
                    newCollectable.SetTip(wasTip);

                    int index = player.collectedList.IndexOf(collectable);
                    if (index >= 0)
                    {
                        player.collectedList[index] = newCollectable;

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

                Destroy(collectable.gameObject);

                yield return new WaitForSeconds(transformDelay);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isProcessing = false;
    }
}