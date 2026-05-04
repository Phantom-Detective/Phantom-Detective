using UnityEngine;
using UnityEngine.AI;

public class DemonAnimatorController : MonoBehaviour
{
    private Animator animator;
    private GhostAI ghostAI;
    private NavMeshAgent agent;

    private float attackCooldown = 0f;
    public float attackCooldownTime = 1.5f;
    private float previousSpeed = 0f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ghostAI = GetComponent<GhostAI>();
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            Debug.LogError("No Animator found on Demon!");
    }

    void Update()
    {
        if (animator == null || ghostAI == null || agent == null)
            return;

        float speed = agent.velocity.magnitude;

        // Trigger walk when starting to move
        if (speed > 0.1f && previousSpeed <= 0.1f)
            animator.SetTrigger("walk");

        previousSpeed = speed;

        // Cooldown timer
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;
    }

    public void TriggerAttack()
    {
        if (animator != null && attackCooldown <= 0f)
        {
            // Randomly use attak1 or attak2
            int rand = Random.Range(0, 2);
            animator.SetTrigger(rand == 0 ? "attak1" : "attak2");
            attackCooldown = attackCooldownTime;
            Debug.Log("Demon attacked!");
        }
    }

    // Called when all memory shards collected
    public void TriggerDeath()
    {
        if (animator != null)
            animator.SetTrigger("die");
    }
}