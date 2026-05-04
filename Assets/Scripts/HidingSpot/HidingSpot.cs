using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    // When player enters the hiding spot trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHiding>().isHidden = true;
            Debug.Log("Player entered hiding spot — HIDDEN");
        }
    }

    // When player leaves the hiding spot trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHiding>().isHidden = false;
            Debug.Log("Player left hiding spot — VISIBLE");
        }
    }
}