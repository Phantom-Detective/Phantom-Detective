//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerMovement : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    public float walkSpeed = 4f;
//    public float runSpeed = 7f;
//    public float gravity = -20f;
//    public float mouseSensitivity = 2f;

//    public Transform cameraTransform;

//    private CharacterController controller;
//    private float verticalVelocity;
//    private float rotationX = 0f;

//    private bool _crouchOverride;
//    private float _crouchSpeed;

//    void Start()
//    {
//        controller = GetComponent<CharacterController>();
//        if (cameraTransform == null)
//            cameraTransform = Camera.main.transform;

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    void Update()
//    {
//        HandleMovement();
//        HandleMouseLook();
//    }

//    void HandleMovement()
//    {
//        var keyboard = Keyboard.current;
//        if (keyboard == null) return;

//        float x = 0f, z = 0f;

//        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
//        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
//        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
//        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;

//        bool isRunning = keyboard.leftShiftKey.isPressed && !_crouchOverride;
//        float speed = _crouchOverride ? _crouchSpeed : (isRunning ? runSpeed : walkSpeed);

//        Vector3 move = transform.right * x + transform.forward * z;

//        if (controller.isGrounded)
//            verticalVelocity = -2f;
//        else
//            verticalVelocity += gravity * Time.deltaTime;

//        move.y = verticalVelocity;
//        controller.Move(move * speed * Time.deltaTime);
//    }

//    /// <summary>Called each frame by PlayerCrouch to apply crouch speed.</summary>
//    public void SetCrouchOverride(bool isCrouching, float speed)
//    {
//        _crouchOverride = isCrouching;
//        _crouchSpeed    = speed;
//    }

//    void HandleMouseLook()
//    {
//        var mouse = Mouse.current;
//        if (mouse == null) return;

//        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
//        float mouseY = mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;

//        transform.Rotate(Vector3.up * mouseX);

//        rotationX -= mouseY;
//        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
//        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
//    }
//}


using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -20f;
    public float mouseSensitivity = 2f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;

    [Header("Torch Settings")]
    public Light torchLight;            // assign a Light component here
    public float torchIntensity = 2f;   // brightness when on
    public Transform torchTransform; // drag Torch GameObject here

    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;
    private float rotationX = 0f;
    private bool torchOn = false;

    private bool _crouchOverride;
    private float _crouchSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Torch off by default
        if (torchLight != null)
            torchLight.enabled = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleTorch();
    }

    void HandleMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f, z = 0f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;

        bool isRunning = keyboard.leftShiftKey.isPressed && !_crouchOverride;
        float speed = _crouchOverride ? _crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        Vector3 move = transform.right * x + transform.forward * z;

        // Jump
        if (controller.isGrounded)
        {
            verticalVelocity = -2f;

            if (keyboard.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleTorch()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.fKey.wasPressedThisFrame)
        {
            torchOn = !torchOn;

            if (torchLight != null)
                torchLight.enabled = torchOn;

            // Play torch click sound
            PlayerAudio audio = GetComponent<PlayerAudio>();
            if (audio != null)
                audio.PlayTorchSound();

            Debug.Log("Torch: " + (torchOn ? "ON" : "OFF"));
        }
    }


    public void SetCrouchOverride(bool isCrouching, float speed)
    {
        _crouchOverride = isCrouching;
        _crouchSpeed = speed;
    }

    void HandleMouseLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float mouseX = mouse.delta.x.ReadValue() * mouseSensitivity * 0.1f;
        float mouseY = mouse.delta.y.ReadValue() * mouseSensitivity * 0.1f;

        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Torch follows camera up/down
        if (torchTransform != null)
            torchTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

    }
    public void ResetCameraRotation()
    {
        rotationX = 0f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.identity;
    }
}