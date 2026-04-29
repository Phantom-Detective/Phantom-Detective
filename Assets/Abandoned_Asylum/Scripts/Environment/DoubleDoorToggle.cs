using UnityEngine;

/// <summary>
/// F-key toggle interaction for double-door leaves. Attach this to one door leaf only.
/// The leaf should be a child of its own DoorHinge object.
/// </summary>
[DisallowMultipleComponent]
public sealed class DoubleDoorToggle : MonoBehaviour
{
    private const float InteractionDistance = 2.5f;

    [SerializeField] private Transform hingeRoot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private bool startOpen;

    private Quaternion closedLocalRotation;
    private float currentAngle;
    private float animationStartAngle;
    private float animationTargetAngle;
    private float animationElapsed;
    private float openDirectionSign = 1f;
    private bool isOpen;
    private bool isAnimating;

    private void Reset()
    {
        hingeRoot = transform.parent;
    }

    private void Awake()
    {
        if (hingeRoot == null)
        {
            hingeRoot = transform.parent != null ? transform.parent : transform;
        }

        closedLocalRotation = hingeRoot.localRotation;
        openDirectionSign = GetDefaultOpenDirectionSign();
        isOpen = startOpen;
        currentAngle = isOpen ? openAngle * openDirectionSign : 0f;
        ApplyAngle(currentAngle);
    }

    private void Update()
    {
        UpdateAnimation();

        if (!isAnimating && IsCameraPointingAtThisDoor() && Input.GetKeyDown(KeyCode.F))
        {
            ToggleDoor();
        }
    }

    private void OnValidate()
    {
        openAngle = Mathf.Max(0f, openAngle);
        animationDuration = Mathf.Max(0.01f, animationDuration);
    }

    private void ToggleDoor()
    {
        animationStartAngle = currentAngle;

        if (isOpen)
        {
            animationTargetAngle = 0f;
            isOpen = false;
        }
        else
        {
            openDirectionSign = ChooseOpenDirectionAwayFromPlayer();
            animationTargetAngle = openAngle * openDirectionSign;
            isOpen = true;
        }

        animationElapsed = 0f;
        isAnimating = true;
    }

    private void UpdateAnimation()
    {
        if (!isAnimating)
        {
            return;
        }

        animationElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(animationElapsed / animationDuration);

        // Use SmoothStep for a more natural ease-in/ease-out feel
        float smoothT = t * t * (3f - 2f * t);
        currentAngle = Mathf.LerpAngle(animationStartAngle, animationTargetAngle, smoothT);
        ApplyAngle(currentAngle);

        if (t >= 1f)
        {
            currentAngle = animationTargetAngle;
            ApplyAngle(currentAngle);
            isAnimating = false;
        }
    }

    private void ApplyAngle(float angle)
    {
        if (hingeRoot == null)
        {
            return;
        }

        hingeRoot.localRotation = closedLocalRotation * Quaternion.Euler(0f, angle, 0f);
    }

    private float ChooseOpenDirectionAwayFromPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null || hingeRoot == null)
        {
            return GetDefaultOpenDirectionSign();
        }

        float defaultSign = GetDefaultOpenDirectionSign();
        Vector3 doorCenter = GetDoorCenter();
        Vector3 hingeToDoor = doorCenter - hingeRoot.position;

        // FIX: Always use Vector3.up (world up) as the rotation axis, NOT hingeRoot.up.
        // hingeRoot.up changes as the hinge rotates and as parent objects have arbitrary
        // orientations — this caused incorrect open-direction choices on many doors.
        // Doors always rotate around the world Y axis in a grounded scene.
        Vector3 swingAxis = Vector3.up;

        Vector3 openedCenterDefault = hingeRoot.position + Quaternion.AngleAxis(defaultSign * openAngle, swingAxis) * hingeToDoor;
        Vector3 openedCenterOpposite = hingeRoot.position + Quaternion.AngleAxis(-defaultSign * openAngle, swingAxis) * hingeToDoor;

        Vector3 camPos = cam.transform.position;
        float distDefault = Vector3.SqrMagnitude(openedCenterDefault - camPos);
        float distOpposite = Vector3.SqrMagnitude(openedCenterOpposite - camPos);

        // Pick the direction that moves the door FURTHEST from the player —
        // i.e., the door swings away from you, which is the natural push behaviour.
        if (distOpposite > distDefault)
        {
            return -defaultSign;
        }

        return defaultSign;
    }

    private float GetDefaultOpenDirectionSign()
    {
        // Right-named doors open clockwise (positive Y), Left counter-clockwise.
        // This is a safe starting guess; ChooseOpenDirectionAwayFromPlayer overrides
        // it dynamically when the player actually presses F to open.
        return IsRightDoor() ? 1f : -1f;
    }

    private bool IsRightDoor()
    {
        return name.Contains("Right");
    }

    private Vector3 GetDoorCenter()
    {
        if (TryGetComponent(out Renderer ownRenderer))
        {
            return ownRenderer.bounds.center;
        }

        Renderer childRenderer = GetComponentInChildren<Renderer>(true);
        if (childRenderer != null)
        {
            return childRenderer.bounds.center;
        }

        return transform.position;
    }

    private bool IsCameraPointingAtThisDoor()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, InteractionDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        DoubleDoorToggle hitDoor = hit.collider.GetComponentInParent<DoubleDoorToggle>();
        if (hitDoor == this)
        {
            return true;
        }

        Transform hitTransform = hit.collider.transform;
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }
}
