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

    [Header("Audio")]
    public AudioSource winSound;     // ghost dying

    private NavMeshAgent agent;
    private GhostPatrol patrol;
    private GhostSenses senses;
    private Transform player;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isPaused = true; // start paused

    void Start()
    {

        startPosition = transform.position;
        startRotation = transform.rotation;

        Debug.Log("ghost start!" + startPosition.x + " , " + startPosition.y + " , " + startPosition.z);

        agent = GetComponent<NavMeshAgent>();
        patrol = GetComponent<GhostPatrol>();
        senses = GetComponent<GhostSenses>();

        // Cache player once
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        EnterPatrol();
        PauseGhost();
    }

    void Update()
    {

        if (isPaused)  return;

        // Check if all shards collected
        if (PuzzleInventory.Instance != null)
        {
            if (PuzzleInventory.Instance.GetCollectedCount() >=
                PuzzleInventory.Instance.totalPieces)
            {
                if (currentState != GhostState.Disappear)
                {
                    Disappear();
                    Debug.Log("Number of shards collected!");
                    // Play win sound
                    if (winSound != null)
                        AudioSource.PlayClipAtPoint(winSound.clip, transform.position);
                }

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
    public void PauseGhost()
    {
        isPaused = true;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    public void ResumeGhost()
    {
        isPaused = false;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
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
        // Reactivate if was disabled
        gameObject.SetActive(true);

        // Re-fetch components in case they were lost
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (patrol == null) patrol = GetComponent<GhostPatrol>();
        if (senses == null) senses = GetComponent<GhostSenses>();

        // Disable agent before teleporting
        agent.enabled = false;
        Debug.Log(gameObject.name + " reset to: " + startPosition);

        // Teleport to start
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Re-enable agent
        agent.enabled = true;

        // Reset state
        currentState = GhostState.Patrol;

        // Start patrol
        EnterPatrol();
        PauseGhost();

        Debug.Log(gameObject.name + " reset to: " + startPosition);
    }

}