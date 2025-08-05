using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformerTrigger : MonoBehaviour
{
    private bool hasActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) hasActivated = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            hasActivated = false;
        }
    }
}
