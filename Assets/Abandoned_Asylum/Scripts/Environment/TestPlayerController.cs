using UnityEngine;

/// <summary>
/// Temporary first-person test rig controller for environment and door testing.
/// Uses the legacy Input Manager axes: Horizontal, Vertical, Mouse X, and Mouse Y.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public sealed class TestPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraHolder;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float walkSpeed = 3f;
    [SerializeField, Min(0f)] private float sprintSpeed = 6f;
    [SerializeField, Min(0f)] private float crouchSpeed = 1.5f;

    [Header("Crouch")]
    [SerializeField, Min(0.1f)] private float standingHeight = 1.8f;
    [SerializeField, Min(0.1f)] private float crouchingHeight = 1.2f;
    [SerializeField, Min(0f)] private float standingCameraHeight = 1.65f;
    [SerializeField, Min(0f)] private float crouchingCameraHeight = 1.05f;
    [SerializeField, Min(0.01f)] private float crouchSmoothTime = 0.08f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStickForce = -2f;

    private float verticalVelocity;
    private float cameraHeightVelocity;
    private float controllerHeightVelocity;

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        cameraHolder = transform.Find("CameraHolder");
        ApplyControllerShape(standingHeight);
    }

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (cameraHolder == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>(true);
            cameraHolder = childCamera != null && childCamera.transform.parent != null
                ? childCamera.transform.parent
                : transform;
        }

        ApplyControllerShape(standingHeight);
        SetCameraHolderHeight(standingCameraHeight);
    }

    private void Update()
    {
        UpdateCrouch();
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

        float speed = GetCurrentMoveSpeed();
        Vector3 move = (transform.right * input.x) + (transform.forward * input.y);
        characterController.Move(move * speed * Time.deltaTime);
    }

    private float GetCurrentMoveSpeed()
    {
        if (IsCrouchingInputHeld())
        {
            return crouchSpeed;
        }

        return Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
    }

    private void UpdateCrouch()
    {
        bool wantsCrouch = IsCrouchingInputHeld();
        float targetControllerHeight = wantsCrouch ? crouchingHeight : standingHeight;
        float targetCameraHeight = wantsCrouch ? crouchingCameraHeight : standingCameraHeight;

        float newControllerHeight = Mathf.SmoothDamp(
            characterController.height,
            targetControllerHeight,
            ref controllerHeightVelocity,
            crouchSmoothTime);

        ApplyControllerShape(newControllerHeight);

        if (cameraHolder != null)
        {
            Vector3 localPosition = cameraHolder.localPosition;
            localPosition.y = Mathf.SmoothDamp(
                localPosition.y,
                targetCameraHeight,
                ref cameraHeightVelocity,
                crouchSmoothTime);

            cameraHolder.localPosition = localPosition;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void ApplyControllerShape(float height)
    {
        if (characterController == null)
        {
            return;
        }

        height = Mathf.Max(height, characterController.radius * 2f);
        characterController.height = height;
        characterController.center = new Vector3(0f, height * 0.5f, 0f);
    }

    private void SetCameraHolderHeight(float height)
    {
        if (cameraHolder == null)
        {
            return;
        }

        cameraHolder.localPosition = new Vector3(0f, height, 0f);
    }

    private static bool IsCrouchingInputHeld()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
    }
}
