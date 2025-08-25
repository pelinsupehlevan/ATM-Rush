using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    [Header("Debug")]
    public bool showGizmos = true;
    public bool verboseDebug = true;

    private void OnTriggerEnter(Collider other)
    {
        if (verboseDebug)
        {
            Debug.Log($"[TRANSFORMER] Gate triggered by {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
            Debug.Log($"[TRANSFORMER] Other collider isTrigger: {other.isTrigger}, enabled: {other.enabled}");
        }

        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null)
        {
            if (verboseDebug)
            {
                Debug.Log($"[TRANSFORMER] Found collectable {collectable.name}, isCollected: {collectable.isCollected}, Type: {collectable.type}");
            }

            if (collectable.isCollected)
            {
                Debug.Log($"[TRANSFORMER] Transforming {collectable.name} from {collectable.type}");
                collectable.TryTransform();
                Debug.Log($"[TRANSFORMER] After transformation: {collectable.name} is now {collectable.type}");
            }
            else
            {
                Debug.Log($"[TRANSFORMER] Collectable {collectable.name} is not collected yet, ignoring");
            }
        }
        else
        {
            if (verboseDebug)
            {
                Debug.Log($"[TRANSFORMER] No Collectable component found on {other.gameObject.name}");

                Collectable childCollectable = other.GetComponentInChildren<Collectable>();
                if (childCollectable != null)
                {
                    Debug.Log($"[TRANSFORMER] Found collectable in children: {childCollectable.name}");
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && collectable.isCollected)
        {
            Debug.Log($"[TRANSFORMER] Item {collectable.name} is staying in trigger zone");
        }
    }

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