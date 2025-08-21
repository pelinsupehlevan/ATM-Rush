//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TransformerGate : MonoBehaviour
//{
//    [Header("Prefab References")]
//    public GameObject goldPrefab;
//    public GameObject diamondPrefab;

//    // Track which collectables are currently being transformed to avoid duplicates
//    private HashSet<Collectable> transformedCollectables = new HashSet<Collectable>();

//    public void TransformIndividualCollectable(Collectable collectable)
//    {
//        // Already transformed? Skip
//        if (transformedCollectables.Contains(collectable)) return;

//        GameObject newPrefab = null;
//        CollectableType newType = collectable.type;

//        // Determine the next type
//        if (collectable.type == CollectableType.Cash && goldPrefab != null)
//        {
//            newPrefab = goldPrefab;
//            newType = CollectableType.Gold;
//        }
//        else if (collectable.type == CollectableType.Gold && diamondPrefab != null)
//        {
//            newPrefab = diamondPrefab;
//            newType = CollectableType.Diamond;
//        }
//        else
//        {
//            // Diamond or no prefab available, nothing to transform
//            return;
//        }

//        // Mark as transforming
//        transformedCollectables.Add(collectable);

//        // Start coroutine to replace collectable
//        StartCoroutine(ReplaceCollectable(collectable, newPrefab, newType));
//    }

//    //private IEnumerator ReplaceCollectable(Collectable oldCollectable, GameObject newPrefab, CollectableType newType)
//    //{
//    //    PlayerController player = oldCollectable.FindPlayerInChain();
//    //    if (player == null) yield break;

//    //    // Save old position and rotation
//    //    Vector3 oldPosition = oldCollectable.transform.position;
//    //    Quaternion oldRotation = oldCollectable.transform.rotation;

//    //    // Instantiate new collectable at same position and rotation
//    //    GameObject newObj = Instantiate(newPrefab, oldPosition, oldRotation);
//    //    Collectable newCollectable = newObj.GetComponent<Collectable>();

//    //    if (newCollectable != null)
//    //    {
//    //        // Preserve chain properties
//    //        newCollectable.isCollected = true;
//    //        newCollectable.followTarget = oldCollectable.followTarget;
//    //        newCollectable.collectDistance = oldCollectable.collectDistance;
//    //        newCollectable.type = newType;
//    //        newCollectable.gameObject.layer = LayerMask.NameToLayer("Default");

//    //        // Replace in player's collected list
//    //        int index = player.collectedList.IndexOf(oldCollectable);
//    //        if (index >= 0)
//    //        {
//    //            player.collectedList[index] = newCollectable;

//    //            // Update follow targets for collectables following the old one
//    //            for (int i = 0; i < player.collectedList.Count; i++)
//    //            {
//    //                if (player.collectedList[i].followTarget == oldCollectable.transform)
//    //                {
//    //                    player.collectedList[i].followTarget = newCollectable.transform;
//    //                }
//    //            }
//    //        }
//    //    }

//    //    // Destroy old collectable
//    //    Destroy(oldCollectable.gameObject);

//    //    yield return null;

//    //    // Remove from transformed set
//    //    transformedCollectables.Remove(oldCollectable);
//    //}

//    private IEnumerator ReplaceCollectable(Collectable oldCollectable, GameObject newPrefab, CollectableType newType)
//    {
//        PlayerController player = oldCollectable.FindPlayerInChain();
//        if (player == null) yield break;

//        // Save old position and rotation
//        Vector3 oldPosition = oldCollectable.transform.position;
//        Quaternion oldRotation = oldCollectable.transform.rotation;

//        // Instantiate new collectable at same position and rotation
//        GameObject newObj = Instantiate(newPrefab, oldPosition, oldRotation);
//        Collectable newCollectable = newObj.GetComponent<Collectable>();

//        if (newCollectable != null)
//        {
//            // Preserve chain properties
//            newCollectable.isCollected = true;
//            newCollectable.followTarget = oldCollectable.followTarget;
//            newCollectable.collectDistance = oldCollectable.collectDistance;
//            newCollectable.type = newType;

//            // keep as Collectable layer so it still works in chain
//            newCollectable.gameObject.layer = LayerMask.NameToLayer("Collectable");

//            // Replace in player's collected list
//            int index = player.collectedList.IndexOf(oldCollectable);
//            if (index >= 0)
//            {
//                player.collectedList[index] = newCollectable;

//                // Update follow targets for collectables following the old one
//                for (int i = 0; i < player.collectedList.Count; i++)
//                {
//                    if (player.collectedList[i].followTarget == oldCollectable.transform)
//                    {
//                        player.collectedList[i].followTarget = newCollectable.transform;
//                    }
//                }
//            }
//        }

//        //  Destroy old immediately
//        Destroy(oldCollectable.gameObject);

//        //  Remove from transformed set immediately
//        transformedCollectables.Remove(oldCollectable);

//        yield break;
//    }


//    private void OnTriggerExit(Collider other)
//    {
//        // Just in case, remove from the set
//        Collectable collectable = other.GetComponent<Collectable>();
//        if (collectable != null)
//        {
//            transformedCollectables.Remove(collectable);
//        }
//    }
//}

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
        if (transformedCollectables.Contains(collectable))
        {
            Debug.Log($"[Gate:{name}] Skipping {collectable.name}, already in transformation set.");
            return;
        }

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
        else
        {
            Debug.Log($"[Gate:{name}] {collectable.name} cannot transform (already Diamond or missing prefab).");
            return;
        }

        Debug.Log($"[Gate:{name}] Starting transformation: {collectable.name} ({collectable.type} → {newType})");

        transformedCollectables.Add(collectable);
        StartCoroutine(ReplaceCollectable(collectable, newPrefab, newType));
    }

    private IEnumerator ReplaceCollectable(Collectable oldCollectable, GameObject newPrefab, CollectableType newType)
    {
        PlayerController player = oldCollectable.FindPlayerInChain();
        if (player == null)
        {
            Debug.LogWarning($"[Gate:{name}] Player not found for {oldCollectable.name}, aborting transform.");
            yield break;
        }

        Vector3 oldPosition = oldCollectable.transform.position;
        Quaternion oldRotation = oldCollectable.transform.rotation;

        GameObject newObj = Instantiate(newPrefab, oldPosition, oldRotation);
        Collectable newCollectable = newObj.GetComponent<Collectable>();

        if (newCollectable != null)
        {
            newCollectable.isCollected = true;
            newCollectable.followTarget = oldCollectable.followTarget;
            newCollectable.collectDistance = oldCollectable.collectDistance;
            newCollectable.type = newType;
            newCollectable.gameObject.layer = LayerMask.NameToLayer("Collectable");

            int index = player.collectedList.IndexOf(oldCollectable);
            if (index >= 0)
            {
                player.collectedList[index] = newCollectable;

                for (int i = 0; i < player.collectedList.Count; i++)
                {
                    if (player.collectedList[i].followTarget == oldCollectable.transform)
                    {
                        player.collectedList[i].followTarget = newCollectable.transform;
                        Debug.Log($"[Gate:{name}] Updated follow target for {player.collectedList[i].name} → {newCollectable.name}");
                    }
                }
            }

            Debug.Log($"[Gate:{name}] Finished replacing {oldCollectable.name} with {newCollectable.name} ({newType})");
        }
        else
        {
            Debug.LogError($"[Gate:{name}] Spawned prefab {newPrefab.name} has no Collectable script!");
        }

        Destroy(oldCollectable.gameObject);
        transformedCollectables.Remove(oldCollectable);
        Debug.Log($"[Gate:{name}] Destroyed {oldCollectable.name}, cleared from set.");
        yield break;
    }

    private void OnTriggerExit(Collider other)
    {
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && transformedCollectables.Contains(collectable))
        {
            transformedCollectables.Remove(collectable);
            Debug.Log($"[Gate:{name}] {collectable.name} exited gate, removed from transform set.");
        }
    }
}

