using UnityEngine;

// ================================================================
// DoorTrigger.cs
// Attach to a SEPARATE Empty GameObject near each door.
// Do NOT attach to the DoorHinge itself — it needs to stay active
// even when the door is hidden.
// ================================================================
public class DoorTrigger : MonoBehaviour
{
    public ElevatorController elevator;

    [Tooltip("1 = Floor 1,  2 = Floor 2,  0 = Basement")]
    public int floorId = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            elevator.OnPlayerEnterDoor(floorId);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            elevator.OnPlayerExitDoor();
    }
}