using UnityEngine;
using UnityEngine.AI;

public class GhostAnimatorController : MonoBehaviour
{
    private Animator animator;
    private GhostAI ghostAI;
    private NavMeshAgent agent;
    private float currentSpeed = 0f;

    // Prevents attack spamming
    private float attackCooldown = 0f;
    public float attackCooldownTime = 1.5f;

    void Start()
    {
        // Animator is on the monster child object
        animator = GetComponentInChildren<Animator>();
        ghostAI = GetComponent<GhostAI>();
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            Debug.LogError("No Animator found on Ghost children!");
        if (ghostAI == null)
            Debug.LogError("No GhostAI found on Ghost!");
        if (agent == null)
            Debug.LogError("No NavMeshAgent found on Ghost!");
    }

    void Update()
    {
        if (animator == null || ghostAI == null || agent == null)
            return;

        // Smooth speed to avoid animation snapping
        float targetSpeed = agent.velocity.magnitude;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
                                   Time.deltaTime * 10f);
        animator.SetFloat("Speed", currentSpeed);

        // Chasing or searching = run animation
        bool isChasing = ghostAI.currentState == GhostAI.GhostState.Chase
                      || ghostAI.currentState == GhostAI.GhostState.Search;
        animator.SetBool("IsChasing", isChasing);

        // Count down attack cooldown
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;
    }

    // Called by GhostKillZone when ghost touches player
    public void TriggerAttack()
    {
        if (animator != null && attackCooldown <= 0f)
        {
            animator.SetTrigger("Attack");
            attackCooldown = attackCooldownTime;
            Debug.Log("Ghost attack triggered!");
        }
    }

    // Add this method to GhostAnimatorController.cs
    public void ResetAttack()
    {
        attackCooldown = 0f;
        if (animator != null)
            animator.ResetTrigger("Attack");
    }
}