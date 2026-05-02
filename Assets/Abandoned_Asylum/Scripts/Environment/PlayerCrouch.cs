using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toggle-crouch controller that works with the New Input System.
/// Attach this to the same GameObject as PlayerMovement and CharacterController.
/// Assign the cameraTransform (your camera or camera pivot) in the Inspector.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera transform (or camera pivot) that sits on the player.")]
    public Transform cameraTransform;

    [Header("Heights")]
    [Tooltip("Full standing height of the CharacterController.")]
    public float standHeight = 1.8f;
    [Tooltip("Crouched height of the CharacterController.")]
    public float crouchHeight = 0.9f;

    [Header("Camera Offset")]
    [Tooltip("How far the camera sits below the top of the collider when standing.")]
    public float cameraHeightOffset = 0.1f;

    [Header("Speed")]
    public float crouchSpeed = 2f;

    [Header("Smoothing")]
    [Tooltip("How fast the collider and camera transition. Higher = snappier.")]
    public float crouchSmoothSpeed = 12f;

    // ── State ──────────────────────────────────────────────────────────────────
    private CharacterController _controller;
    private PlayerMovement _movement;

    private bool _isCrouching;
    private bool _wantsToStand;

    private float _currentHeight;
    private float _targetHeight;

    private float _standCameraY;
    private float _targetCameraY;

    // ──────────────────────────────────────────────────────────────────────────

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _movement   = GetComponent<PlayerMovement>();

        // Initialise collider to standing height
        _controller.height = standHeight;
        _controller.center = new Vector3(0f, standHeight / 2f, 0f);
        _currentHeight     = standHeight;
        _targetHeight      = standHeight;

        // Remember the camera's standing Y so we can offset relative to it.
        // This works whether the camera is at eye height, mid-body, etc.
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        _standCameraY  = standHeight - cameraHeightOffset;
        _targetCameraY = _standCameraY;

        if (cameraTransform != null)
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x,
                _standCameraY,
                cameraTransform.localPosition.z);
    }

    void Update()
    {
        HandleInput();
        SmoothCollider();
        SmoothCamera();
        BroadcastSpeed();
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool crouchPressed =
            keyboard.leftCtrlKey.wasPressedThisFrame ||
            keyboard.cKey.wasPressedThisFrame;

        if (!crouchPressed) return;

        if (_isCrouching)
        {
            // Only stand up if nothing is blocking overhead
            _wantsToStand = true;
        }
        else
        {
            _isCrouching   = true;
            _wantsToStand  = false;
            _targetHeight  = crouchHeight;
            _targetCameraY = crouchHeight - cameraHeightOffset;
        }
    }

    // ── Collider ───────────────────────────────────────────────────────────────

    void SmoothCollider()
    {
        // If we want to stand, keep trying until the ceiling clears
        if (_wantsToStand && !IsCeilingBlocked())
        {
            _isCrouching   = false;
            _wantsToStand  = false;
            _targetHeight  = standHeight;
            _targetCameraY = _standCameraY;
        }

        _currentHeight = Mathf.Lerp(
            _currentHeight,
            _targetHeight,
            Time.deltaTime * crouchSmoothSpeed);

        _controller.height = _currentHeight;
        _controller.center = new Vector3(0f, _currentHeight / 2f, 0f);
    }

    // ── Camera ─────────────────────────────────────────────────────────────────

    void SmoothCamera()
    {
        if (cameraTransform == null) return;

        float newY = Mathf.Lerp(
            cameraTransform.localPosition.y,
            _targetCameraY,
            Time.deltaTime * crouchSmoothSpeed);

        cameraTransform.localPosition = new Vector3(
            cameraTransform.localPosition.x,
            newY,
            cameraTransform.localPosition.z);
    }

    // ── Speed ──────────────────────────────────────────────────────────────────

    void BroadcastSpeed()
    {
        if (_movement == null) return;

        // Let PlayerMovement know the current max speed
        // (only overrides when crouching; standing speed is owned by PlayerMovement)
        _movement.SetCrouchOverride(_isCrouching, crouchSpeed);
    }

    // ── Ceiling check ──────────────────────────────────────────────────────────

    bool IsCeilingBlocked()
    {
        // Cast from the top of the current (crouched) collider upward
        float topOfCollider = transform.position.y + _currentHeight;
        float checkDistance = standHeight - _currentHeight + 0.05f;

        return Physics.Raycast(
            new Vector3(transform.position.x, topOfCollider, transform.position.z),
            Vector3.up,
            checkDistance);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Returns true while the player is crouched (or transitioning into a crouch).</summary>
    public bool IsCrouching() => _isCrouching;
}
