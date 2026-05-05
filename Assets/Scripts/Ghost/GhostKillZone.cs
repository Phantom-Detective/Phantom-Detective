//using UnityEngine;

//public class GhostKillZone : MonoBehaviour
//{
//    private GhostAnimatorController animController;

//    void Start()
//    {
//        animController = GetComponentInParent<GhostAnimatorController>();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            // Check if player is hidden — if so ignore completely
//            PlayerHiding hiding = other.GetComponent<PlayerHiding>();
//            if (hiding != null && hiding.isHidden)
//            {
//                Debug.Log("Player is hidden — ghost can't attack!");
//                return;
//            }

//            // Play attack animation
//            if (animController != null)
//                animController.TriggerAttack();

//            // Deal damage
//            other.GetComponent<PlayerHealth>().TakeDamage();
//            Debug.Log("Ghost attacked player!");
//        }
//    }
//}

using UnityEngine;

public class GhostKillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Check if player is hidden
        PlayerHiding hiding = other.GetComponent<PlayerHiding>();
        if (hiding != null && hiding.isHidden)
        {
            Debug.Log("Player is hidden — ghost can't attack!");
            return;
        }

        // Deal damage only — animation handled by distance check
        other.GetComponent<PlayerHealth>().TakeDamage();
        Debug.Log("Ghost attacked player!");
    }
}