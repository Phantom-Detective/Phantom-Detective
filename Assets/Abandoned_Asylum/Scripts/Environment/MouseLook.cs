using UnityEngine;

/// <summary>
/// Temporary mouse look for the TestPlayer hierarchy.
/// Attach to CameraHolder for pitch, with the TestPlayer body assigned for yaw.
/// </summary>
[DisallowMultipleComponent]
public sealed class MouseLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Look")]
    [SerializeField, Min(0f)] private float mouseSensitivity = 2f;
    [SerializeField] private float minimumPitch = -80f;
    [SerializeField] private float maximumPitch = 80f;
    [SerializeField] private bool lockCursorOnStart = true;

    private float pitch;

    private void Reset()
    {
        CharacterController parentController = GetComponentInParent<CharacterController>();
        playerBody = parentController != null ? parentController.transform : transform.parent;
    }

    private void Awake()
    {
        if (playerBody == null)
        {
            CharacterController parentController = GetComponentInParent<CharacterController>();
            playerBody = parentController != null ? parentController.transform : transform.parent;
        }

        Vector3 localEuler = transform.localEulerAngles;
        pitch = NormalizeAngle(localEuler.x);
        pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        pitch = Mathf.Clamp(pitch - mouseY, minimumPitch, maximumPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnValidate()
    {
        if (maximumPitch < minimumPitch)
        {
            float oldMinimum = minimumPitch;
            minimumPitch = maximumPitch;
            maximumPitch = oldMinimum;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }
}
