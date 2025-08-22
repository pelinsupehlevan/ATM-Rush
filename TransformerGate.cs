using UnityEngine;

public class TransformerGate : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a Collectable component
        Collectable collectable = other.GetComponent<Collectable>();
        if (collectable != null && collectable.isCollected)
        {
            // Call the method that exists in your Collectable script
            collectable.TryTransform();
        }
    }
}