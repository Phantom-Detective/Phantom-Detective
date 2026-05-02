using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CrouchController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot; // الكاميرا (أو نقطة الكاميرا)

    [Header("Heights")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;

    [Header("Speed")]
    public float standSpeed = 5f;
    public float crouchSpeed = 2.5f;

    [Header("Smooth")]
    public float smoothTime = 10f;

    private CharacterController controller;
    private Vector3 cameraStartPos;

    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // حفظ مكان الكاميرا الأصلي
        cameraStartPos = cameraPivot.localPosition;

        // تأكيد الارتفاع الطبيعي
        controller.height = standHeight;
    }

    void Update()
    {
        HandleCrouch();
        AdjustCamera();
    }

    void HandleCrouch()
    {
        // Toggle crouch بزر Ctrl
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isCrouching)
            {
                // نحاول نقف
                if (!IsBlockedAbove())
                    isCrouching = false;
            }
            else
            {
                // ننزل
                isCrouching = true;
            }
        }

        // تغيير ارتفاع الكوليدر
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * smoothTime);

        // ضبط مركز الكوليدر
        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    void AdjustCamera()
    {
        // نزول الكاميرا مع crouch
        float targetY = isCrouching ? crouchHeight : standHeight;

        Vector3 targetPos = new Vector3(
            cameraStartPos.x,
            targetY,
            cameraStartPos.z
        );

        cameraPivot.localPosition = Vector3.Lerp(
            cameraPivot.localPosition,
            targetPos,
            Time.deltaTime * smoothTime
        );
    }

    bool IsBlockedAbove()
    {
        // منع الوقوف لو في سقف فوقك
        Ray ray = new Ray(transform.position, Vector3.up);
        float checkDistance = standHeight;

        return Physics.Raycast(ray, checkDistance);
    }

    // 👇 ممكن تستخدمها في Ghost AI
    public bool IsCrouching()
    {
        return isCrouching;
    }
}