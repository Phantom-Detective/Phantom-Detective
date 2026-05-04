using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public enum GhostState { Patrol, Chase, Search, Disappear }

    [Header("State Debug")]
    public GhostState currentState = GhostState.Patrol;

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;
    public float patrolSpeed = 3.5f;

    [Header("Search Settings")]
    public float searchDuration = 5f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPosition;

    private NavMeshAgent agent;
    private GhostPatrol patrol;
    private GhostSenses senses;
    private Transform player;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {

        startPosition = transform.position;
        startRotation = transform.rotation;

        agent = GetComponent<NavMeshAgent>();
        patrol = GetComponent<GhostPatrol>();
        senses = GetComponent<GhostSenses>();

        // Cache player once
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        EnterPatrol();
    }

    void Update()
    {
        // Check if all shards collected
        if (PuzzleInventory.Instance != null)
        {
            if (PuzzleInventory.Instance.GetCollectedCount() >=
                PuzzleInventory.Instance.totalPieces)
            {
                Disappear();
                return;
            }
        }

        switch (currentState)
        {
            case GhostState.Patrol: HandlePatrol(); break;
            case GhostState.Chase: HandleChase(); break;
            case GhostState.Search: HandleSearch(); break;
            case GhostState.Disappear: HandleDisappear(); break;
        }
    }

    // ─── PATROL ───────────────────────────────────────────
    void EnterPatrol()
    {
        currentState = GhostState.Patrol;
        agent.speed = patrolSpeed;
        agent.isStopped = false;        // ← make sure agent is running
        patrol.StartPatrol();
        Debug.Log("Ghost: PATROL");
    }

    void HandlePatrol()
    {
        patrol.UpdatePatrol();

        if (senses.IsPlayerDetectable())
            EnterChase();
    }

    // ─── CHASE ────────────────────────────────────────────
    void EnterChase()
    {
        currentState = GhostState.Chase;
        agent.speed = chaseSpeed;
        agent.isStopped = false;        // ← critical: must be false to move
        patrol.StopPatrol();            // stops waypoint logic only
        Debug.Log("Ghost: CHASE");
    }

    void HandleChase()
    {
        if (player == null) return;

        if (senses.IsPlayerDetectable())
        {
            lastKnownPosition = player.position;
            agent.isStopped = false;
            agent.SetDestination(lastKnownPosition);
        }
        else
        {
            // Player hidden or out of range
            EnterSearch();
        }
    }

    // ─── SEARCH ───────────────────────────────────────────
    void EnterSearch()
    {
        currentState = GhostState.Search;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        searchTimer = 0f;

        // Go to last known position first
        agent.SetDestination(lastKnownPosition);
        Debug.Log("Ghost: SEARCH");
    }

    void HandleSearch()
    {
        searchTimer += Time.deltaTime;

        // Player found again while searching
        if (senses.IsPlayerDetectable())
        {
            EnterChase();
            return;
        }

        // Arrived at last known position — wander nearby
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Pick a random nearby point to wander to
            Vector3 randomPoint = lastKnownPosition +
                                  Random.insideUnitSphere * 4f;
            randomPoint.y = transform.position.y;

            // Make sure random point is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(
                    randomPoint, out hit, 4f,
                    UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        // Give up after search duration
        if (searchTimer >= searchDuration)
        {
            Debug.Log("Ghost: giving up, back to PATROL");
            EnterPatrol();
        }
    }

    // ─── DISAPPEAR ────────────────────────────────────────
    void HandleDisappear()
    {
        gameObject.SetActive(false);
    }

    public void Disappear()
    {
        currentState = GhostState.Disappear;
    }

    public void ResetGhost()
    {
        gameObject.SetActive(true);
        transform.position = startPosition;
        transform.rotation = startRotation;
        agent.enabled = false;
        agent.enabled = true;
        EnterPatrol();
        Debug.Log(gameObject.name + " reset!");
    }

}