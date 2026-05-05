


using UnityEngine;
using UnityEngine.AI;

public class GhostAnimatorController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDistance = 1.5f;

    private Animator animator;
    private GhostAI ghostAI;
    private NavMeshAgent agent;
    private Transform player;
    private float currentSpeed = 0f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ghostAI = GetComponent<GhostAI>();
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (animator == null)
            Debug.LogError("No Animator found on " + gameObject.name);
        if (ghostAI == null)
            Debug.LogError("No GhostAI found on " + gameObject.name);
        if (agent == null)
            Debug.LogError("No NavMeshAgent found on " + gameObject.name);
    }

    void Update()
    {
        if (animator == null || ghostAI == null || agent == null)
            return;

        // Smooth speed
        float targetSpeed = agent.velocity.magnitude;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
                                   Time.deltaTime * 10f);
        animator.SetFloat("Speed", currentSpeed);

        // Chasing or searching = run
        bool isChasing = ghostAI.currentState == GhostAI.GhostState.Chase
                      || ghostAI.currentState == GhostAI.GhostState.Search;
        animator.SetBool("IsChasing", isChasing);

        // Distance based attack
        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position, player.position);

            bool isAttacking = distance <= attackDistance
                             && ghostAI.currentState == GhostAI.GhostState.Chase;

            animator.SetBool("IsAttacking", isAttacking);
        }
    }

    // Draw attack range in Scene view
    void OnDrawGizmos()
    {
        // Black circle = attack range
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}