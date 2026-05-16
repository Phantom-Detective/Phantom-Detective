using UnityEngine;

// ================================================================
// ElevatorFloorTrigger.cs
// Attach to an Empty GameObject that is a CHILD of the elevator cabin.
// Add a Box Collider (Is Trigger = true) sized to cover the elevator floor.
// ================================================================
public class ElevatorFloorTrigger : MonoBehaviour
{
    public ElevatorController elevator;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered elevator: " + other.name);
        if (other.CompareTag("Player"))
            elevator.OnPlayerEnterElevator();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            elevator.OnPlayerExitElevator();
    }
}