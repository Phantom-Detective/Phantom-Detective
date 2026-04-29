using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -20f;
    public float mouseSensitivity = 2f;

    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;
    private float rotationX = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
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

        bool isRunning = keyboard.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;
        controller.Move(move * speed * Time.deltaTime);
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
    }
}