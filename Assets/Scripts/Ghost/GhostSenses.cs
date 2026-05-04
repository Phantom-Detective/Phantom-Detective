using UnityEngine;

public class GhostSenses : MonoBehaviour
{
    [Header("Sight Settings")]
    public float sightRadius = 10f;
    public float fieldOfView = 90f;
    public LayerMask obstacleMask;

    [Header("Hearing Settings")]
    public float hearingRadius = 6f;

    private Transform player;
    private PlayerNoise playerNoise;
    private PlayerHiding playerHiding;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerNoise = playerObj.GetComponent<PlayerNoise>();
            playerHiding = playerObj.GetComponent<PlayerHiding>();
        }
    }

    public bool IsPlayerDetectable()
    {
        if (player == null) return false;
        if (playerHiding != null && playerHiding.isHidden) return false;
        return CanSeePlayer() || CanHearPlayer();
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;

        if (dirToPlayer.magnitude > sightRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfView / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer.normalized,
                            dirToPlayer.magnitude, obstacleMask))
            return false;

        return true;
    }

    bool CanHearPlayer()
    {
        if (playerNoise == null) return false;

        float noise = playerNoise.currentNoiseLevel;
        if (noise <= 0f) return false;

        // ← NEW: walls block hearing too
        Vector3 dirToPlayer = player.position - transform.position;
        if (Physics.Raycast(transform.position, dirToPlayer.normalized,
                            dirToPlayer.magnitude, obstacleMask))
            return false;

        float effectiveRadius = hearingRadius * noise;
        return Vector3.Distance(transform.position, player.position)
               < effectiveRadius;
    }

    void OnDrawGizmos()
    {
        // Sight radius — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        // Hearing radius — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        // ← NEW: FOV cone — green lines
        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0)
                                * transform.forward * sightRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0)
                                * transform.forward * sightRadius;

        Gizmos.DrawLine(transform.position,
                        transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position,
                        transform.position + rightBoundary);

        // ← NEW: Center forward line — blue
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position,
                        transform.position + transform.forward * sightRadius);
    }
}