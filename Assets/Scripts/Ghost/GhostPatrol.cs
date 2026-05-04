using UnityEngine;
using UnityEngine.AI;

public class GhostPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Patrol Settings")]
    public float waypointWaitTime = 1f;

    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isPatrolling = false;  // ← new flag

    // Stuck detection
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("No NavMeshAgent found on " + gameObject.name);
        lastPosition = transform.position;
    }

    public void StartPatrol()
    {
        if (agent == null)
        {
            Debug.LogError("Agent is null on " + gameObject.name);
            return;
        }
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints on " + gameObject.name);
            return;
        }

        isPatrolling = true;
        agent.isStopped = false;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
    public void StopPatrol()
    {
        isPatrolling = false;
        // DO NOT set agent.isStopped here
        // GhostAI controls the agent directly during chase
    }

    public void UpdatePatrol()
    {
        if (!isPatrolling) return;
        if (waypoints.Length == 0) return;

        // Waiting at waypoint
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waypointWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                GoToNextWaypoint();
            }
            return;
        }

        // Arrived at waypoint?
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaiting = true;
            return;
        }

        // Stuck detection
        stuckTimer += Time.deltaTime;
        if (stuckTimer > 2f)
        {
            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                Debug.Log("Ghost stuck — skipping waypoint");
                GoToNextWaypoint();
            }
            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    void GoToNextWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
}