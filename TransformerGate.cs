using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    [Header("Debug")]
    public bool showGizmos = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRANSFORMER] Gate triggered by {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        // Also check if it's a collected object following the player
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null)
        {
            Debug.Log($"[TRANSFORMER] Found collectable {collectable.name}, isCollected: {collectable.isCollected}, Type: {collectable.type}");

            if (collectable.isCollected)
            {
                Debug.Log($"[TRANSFORMER] Transforming {collectable.name}");
                collectable.TryTransform();
            }
        }
        else
        {
            Debug.Log($"[TRANSFORMER] No Collectable component found");
        }
    }

    // Visual helper to see the gate's trigger area
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}