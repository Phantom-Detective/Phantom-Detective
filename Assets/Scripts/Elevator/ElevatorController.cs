using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Cabin")]
    public Transform elevatorCabin;

    [Header("Floor Waypoints")]
    public Transform waypointFloor1;
    public Transform waypointFloor2;
    public Transform waypointBasement;

    [Header("Door Hinges")]
    public GameObject doorHinge1;
    public GameObject doorHinge2;
    public GameObject doorHingeBasement;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    [Header("Sounds")]
    public AudioSource elevatorMovingSound;   // looping sound while moving
    public AudioSource elevatorArrivedSound;  // ding when elevator arrives
    public AudioSource basementCrashSound;    // crash/malfunction sound for basement
    public AudioSource neonFlickerSound;      // electrical flicker sound for neon

    [Header("Basement Neon Light")]
    public Renderer neonRenderer;   // drag NeonLift here instead of a Light
    public float neonFlickerDuration = 3f;

    // ── Internal state ────────────────────────────────────────────
    [HideInInspector] public bool playerInsideElevator = false;
    private bool isMoving = false;
    private bool insideElevator = false;
    private bool playerNearDoor = false;
    private bool showingTemporaryMessage = false;
    private int currentFloor = 1;
    private int nearDoorFloorId = -1;

    private GameObject playerObj;
    private CharacterController playerCC;

    void Start()
    {
        HidePrompt();
        SetAllDoors(true);

        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerCC = playerObj.GetComponent<CharacterController>();

        // Make sure moving sound doesn't play at start
        if (elevatorMovingSound != null)
        {
            elevatorMovingSound.loop = true;
            elevatorMovingSound.Stop();
        }
    }

    void Update()
    {
        if (isMoving) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!insideElevator && playerNearDoor)
        {
            ShowPrompt("Press (Y) to call the elevator");

            if (keyboard.yKey.wasPressedThisFrame)
            {
                playerNearDoor = false;
                HidePrompt();
                StartCoroutine(CallElevatorRoutine(nearDoorFloorId));
            }
        }

        if (insideElevator && !isMoving && !showingTemporaryMessage)
        {
            ShowPrompt("[1] Floor 1\n[2] Floor 2\n[3] Basement");

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
                StartCoroutine(RideToFloor(1));

            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
                StartCoroutine(RideToFloor(2));

            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
                StartCoroutine(RideToFloor(0));
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Called by ElevatorFloorTrigger
    // ─────────────────────────────────────────────────────────────
    public void OnPlayerEnterElevator()
    {
        Debug.Log("Player entered elevator cabin!");
        insideElevator = true;
        playerInsideElevator = true;
        HidePrompt();

        GameObject hinge = GetHinge(currentFloor);
        if (hinge) SetDoorOpen(hinge, false);
    }

    public void OnPlayerExitElevator()
    {
        Debug.Log("Player left elevator cabin!");
        insideElevator = false;
        playerInsideElevator = false;
        HidePrompt();
    }

    // ─────────────────────────────────────────────────────────────
    // Called by DoorTrigger
    // ─────────────────────────────────────────────────────────────
    public void OnPlayerEnterDoor(int floorId)
    {
        Debug.Log("Player entered door zone, floor: " + floorId);
        if (isMoving || insideElevator) return;
        playerNearDoor = true;
        nearDoorFloorId = floorId;
    }

    public void OnPlayerExitDoor()
    {
        playerNearDoor = false;
        nearDoorFloorId = -1;
        if (!insideElevator) HidePrompt();
    }

    // ─────────────────────────────────────────────────────────────
    // Call elevator to a floor
    // ─────────────────────────────────────────────────────────────
    IEnumerator CallElevatorRoutine(int targetFloorId)
    {
        isMoving = true;

        yield return StartCoroutine(MoveCabinToFloor(targetFloorId, false));
        currentFloor = targetFloorId;

        // Arrived sound
        PlaySound(elevatorArrivedSound);

        // Open door sound + open door
        GameObject hinge = GetHinge(targetFloorId);
        if (hinge)
        {
            SetDoorOpen(hinge, true);
        }

        isMoving = false;
        ShowPrompt("Walk inside the elevator");

        StartCoroutine(CloseDoorIfPlayerDoesntEnter(hinge));
    }

    IEnumerator CloseDoorIfPlayerDoesntEnter(GameObject hinge)
    {
        yield return new WaitForSeconds(5f);

        if (!insideElevator && hinge != null)
        {
            SetDoorOpen(hinge, false);
            HidePrompt();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Ride to chosen floor
    // ─────────────────────────────────────────────────────────────
    IEnumerator RideToFloor(int targetFloorId)
    {
        if (targetFloorId == currentFloor)
        {
            showingTemporaryMessage = true;
            ShowPrompt("You are already on this floor!");
            yield return new WaitForSeconds(2f);
            showingTemporaryMessage = false;
            yield break;
        }

        isMoving = true;
        HidePrompt();

        if (playerCC != null) playerCC.enabled = false;

        yield return StartCoroutine(MoveCabinToFloor(targetFloorId, true));
        currentFloor = targetFloorId;

        if (playerCC != null) playerCC.enabled = true;

        // ── Basement special event ────────────────────────────────
        if (targetFloorId == 0)
        {
            yield return StartCoroutine(BasementMalfunctionEvent());
            isMoving = false;
            insideElevator = true; // player is still inside
            yield break;
        }

        // ── Normal floor arrival ──────────────────────────────────
        PlaySound(elevatorArrivedSound);

        GameObject hinge = GetHinge(targetFloorId);
        if (hinge)
        {
            SetDoorOpen(hinge, true);
        }

        isMoving = false;
        insideElevator = false;
        playerInsideElevator = true;

        ShowPrompt("Walk out — door closes behind you.");

        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !playerInsideElevator);

        yield return new WaitForSeconds(1f);
        if (hinge) SetDoorOpen(hinge, false);
        HidePrompt();
    }

    // ─────────────────────────────────────────────────────────────
    // Basement malfunction sequence
    // ─────────────────────────────────────────────────────────────
    IEnumerator BasementMalfunctionEvent()
    {
        // Play crash sound
        PlaySound(basementCrashSound);

        // Show error message
        ShowPrompt("ERROR: Door malfunction — cannot open.");

        // Flicker the neon light
        yield return StartCoroutine(FlickerNeon(neonFlickerDuration));

        // After flickering, leave light on and keep message for a moment
        yield return new WaitForSeconds(2f);

        HidePrompt();

        // Player stays inside — they can pick another floor
        showingTemporaryMessage = false;
    }

    // ─────────────────────────────────────────────────────────────
    // Neon flicker effect
    // ─────────────────────────────────────────────────────────────
    IEnumerator FlickerNeon(float duration)
    {
        if (neonRenderer == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float flickerWait = Random.Range(0.05f, 0.2f);

            // Toggle the renderer on/off to simulate flicker
            neonRenderer.enabled = !neonRenderer.enabled;
            PlaySound(neonFlickerSound);

            yield return new WaitForSeconds(flickerWait);
            elapsed += flickerWait;
        }

        // Leave it on after flickering
        neonRenderer.enabled = true;
    }

    // ─────────────────────────────────────────────────────────────
    // Smooth movement — movePlayer carries the player along
    // ─────────────────────────────────────────────────────────────
    IEnumerator MoveCabinToFloor(int floorId, bool movePlayer)
    {
        Transform waypoint = GetWaypoint(floorId);
        if (waypoint == null) yield break;

        Vector3 start = elevatorCabin.position;
        Vector3 end = waypoint.position;
        float dist = Vector3.Distance(start, end);
        float dur = dist / moveSpeed;
        float elapsed = 0f;

        // Start moving sound
        if (elevatorMovingSound != null) elevatorMovingSound.Play();

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);

            Vector3 newCabinPos = Vector3.Lerp(start, end, t);
            Vector3 delta = newCabinPos - elevatorCabin.position;

            elevatorCabin.position = newCabinPos;

            if (movePlayer && playerObj != null)
                playerObj.transform.position += delta;

            yield return null;
        }

        elevatorCabin.position = end;

        // Stop moving sound
        if (elevatorMovingSound != null) elevatorMovingSound.Stop();
    }

    // ─────────────────────────────────────────────────────────────
    // Door open/close
    // ─────────────────────────────────────────────────────────────
    //void SetDoorOpen(GameObject door, bool open)  using no msh colliders
    //{
    //    if (door == null) return;

    //    Collider[] colliders = door.GetComponentsInChildren<Collider>();
    //    foreach (Collider col in colliders)
    //        if (!col.isTrigger) col.enabled = !open;

    //    Renderer[] renderers = door.GetComponentsInChildren<Renderer>();
    //    foreach (Renderer rend in renderers)
    //        rend.enabled = !open;
    //}

    void SetDoorOpen(GameObject door, bool open)
    {
        if (door == null) return;
        door.SetActive(!open);
    }
    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────
    void PlaySound(AudioSource source)
    {
        if (source != null) source.Play();
    }

    Transform GetWaypoint(int id)
    {
        switch (id)
        {
            case 1: return waypointFloor1;
            case 2: return waypointFloor2;
            case 0: return waypointBasement;
            default: return null;
        }
    }

    GameObject GetHinge(int id)
    {
        switch (id)
        {
            case 1: return doorHinge1;
            case 2: return doorHinge2;
            case 0: return doorHingeBasement;
            default: return null;
        }
    }

    void SetAllDoors(bool closed)
    {
        if (doorHinge1) SetDoorOpen(doorHinge1, !closed);
        if (doorHinge2) SetDoorOpen(doorHinge2, !closed);
        if (doorHingeBasement) SetDoorOpen(doorHingeBasement, !closed);
    }

    void ShowPrompt(string msg)
    {
        if (promptText) { promptText.text = msg; promptText.enabled = true; }
    }

    void HidePrompt()
    {
        if (promptText) promptText.enabled = false;
    }
}