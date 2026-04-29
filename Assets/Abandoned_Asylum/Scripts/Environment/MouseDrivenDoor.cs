using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Mouse-driven first-person door interaction for hinge-parent door prefabs.
/// Attach this to the door mesh/collider object. The optional hinge root defaults to the parent.
/// </summary>
[DisallowMultipleComponent]
public sealed class MouseDrivenDoor : MonoBehaviour
{
    private const float LegacyMouseSensitivityDefault = 0.2f;
    private const float CurrentMouseSensitivityDefault = 1f;

    public enum HingeSide
    {
        Left,
        Right
    }

    [Header("Detection")]
    [SerializeField] private Camera interactionCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Hinge")]
    [SerializeField] private Transform hingeRoot;
    [SerializeField] private HingeSide hingeSide = HingeSide.Left;
    [SerializeField] private bool invertOpenDirection;
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private float startingAngle = 0f;

    [Header("Mouse")]
    [SerializeField, Min(0f)] private float mouseSensitivity = CurrentMouseSensitivityDefault;
    [SerializeField, Min(0f)] private float openSpeedMultiplier = 2f;
    [SerializeField, Range(0f, 1f)] private float verticalMouseInfluence = 0.35f;
    [SerializeField] private bool invertMouseDirection;
    [SerializeField] private bool flipSideDetection;

    [Header("Motion")]
    [SerializeField, Min(0.01f)] private float smoothTime = 0.08f;
    [SerializeField, Min(1f)] private float maxRotationSpeed = 540f;

    [Header("Optional Highlight")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Renderer[] highlightRenderers;

    private static MouseDrivenDoor grabbedDoor;

    private Quaternion closedLocalRotation;
    // Stable world-space forward of the door when CLOSED — computed once in Awake,
    // never updated again. This is the key fix: we must NOT use hingeRoot.forward
    // during gameplay because hingeRoot rotates as the door opens, which breaks
    // side-detection the moment the door is no longer at angle 0.
    private Vector3 stableClosedForwardXZ;

    private float currentAngle;
    private float targetAngle;
    private float angularVelocity;
    private bool isGrabbed;
    private bool isHighlighted;
    private Material[][] originalMaterials;

    public bool IsGrabbed => isGrabbed;
    public float CurrentAngle => currentAngle;

    private void Reset()
    {
        hingeRoot = transform.parent;
        highlightRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Awake()
    {
        if (hingeRoot == null)
        {
            hingeRoot = transform.parent != null ? transform.parent : transform;
        }

        if (highlightRenderers == null || highlightRenderers.Length == 0)
        {
            highlightRenderers = GetComponentsInChildren<Renderer>(true);
        }

        closedLocalRotation = hingeRoot.localRotation;

        // Bake the door's forward direction in world space while the hinge is at rest (closed).
        // We do this once here so it never changes regardless of door angle later.
        // Using the parent's transform to convert from local → world gives us a vector
        // that is stable even for doors with negative scale.
        stableClosedForwardXZ = ComputeStableClosedForwardXZ();

        targetAngle = Mathf.Clamp(startingAngle, minAngle, maxAngle);
        currentAngle = targetAngle;
        ApplyDoorRotation(currentAngle);
    }

    private Vector3 ComputeStableClosedForwardXZ()
    {
        // Convert the hinge's local-space forward (at rest) into world space via its PARENT,
        // so we bypass the hinge's own rotation (which changes as the door opens).
        Vector3 worldForward;
        Transform parent = hingeRoot.parent;
        if (parent != null)
        {
            // parent.TransformDirection respects parent's scale/rotation but not the hinge's
            worldForward = parent.TransformDirection(closedLocalRotation * Vector3.forward);
        }
        else
        {
            worldForward = closedLocalRotation * Vector3.forward;
        }

        // Flatten onto the XZ plane — doors rotate around Y, so we only care about
        // horizontal side (left/right of door plane). Vertical component causes jitter
        // when camera is at a different height than the hinge.
        worldForward.y = 0f;

        if (worldForward.sqrMagnitude < 0.001f)
        {
            // Fallback: door's forward is nearly straight up/down (very unusual).
            // Use world forward as a safe default.
            worldForward = Vector3.forward;
        }

        return worldForward.normalized;
    }

    private void Update()
    {
        bool canInteract = grabbedDoor == null || grabbedDoor == this;
        bool isPointedAt = canInteract && !isGrabbed && IsCameraPointingAtThisDoor();

        SetHighlighted(isPointedAt);

        if (isGrabbed)
        {
            if (WasLeftMouseReleasedThisFrame() || !IsLeftMouseHeld())
            {
                ReleaseDoor();
                return;
            }

            UpdateTargetAngleFromMouse();
            return;
        }

        if (isPointedAt && WasLeftMousePressedThisFrame())
        {
            GrabDoor();
        }
    }

    private void LateUpdate()
    {
        currentAngle = Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref angularVelocity,
            smoothTime,
            maxRotationSpeed,
            Time.deltaTime);

        ApplyDoorRotation(currentAngle);
    }

    private void OnDisable()
    {
        if (grabbedDoor == this)
        {
            grabbedDoor = null;
        }

        isGrabbed = false;
        SetHighlighted(false);
    }

    private void OnValidate()
    {
        interactionDistance = Mathf.Max(0.1f, interactionDistance);
        mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
        openSpeedMultiplier = Mathf.Max(0f, openSpeedMultiplier);
        smoothTime = Mathf.Max(0.01f, smoothTime);
        maxRotationSpeed = Mathf.Max(1f, maxRotationSpeed);

        if (maxAngle < minAngle)
        {
            float oldMin = minAngle;
            minAngle = maxAngle;
            maxAngle = oldMin;
        }

        startingAngle = Mathf.Clamp(startingAngle, minAngle, maxAngle);
    }

    private void GrabDoor()
    {
        grabbedDoor = this;
        isGrabbed = true;
        SetHighlighted(false);
    }

    private void ReleaseDoor()
    {
        if (grabbedDoor == this)
        {
            grabbedDoor = null;
        }

        isGrabbed = false;
    }

    private void UpdateTargetAngleFromMouse()
    {
        Vector2 mouseDelta = ReadMouseDelta();
        if (mouseDelta.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float dragAmount = mouseDelta.x + (mouseDelta.y * verticalMouseInfluence);

        if (invertMouseDirection)
        {
            dragAmount *= -1f;
        }

        // --- STABLE SIDE DETECTION ---
        // Determine which side of the door the player is on using the BAKED
        // closed-position forward vector. This never changes as the door opens,
        // so it is reliable from every angle and from both sides.
        float sideSign = ComputeSideSign();

        targetAngle = Mathf.Clamp(
            targetAngle + dragAmount * GetEffectiveMouseSensitivity() * sideSign,
            minAngle,
            maxAngle);
    }

    private float ComputeSideSign()
    {
        Camera cam = GetInteractionCamera();
        if (cam == null || hingeRoot == null)
        {
            return 1f;
        }

        // Vector from hinge to camera, flattened to XZ.
        Vector3 toCam = cam.transform.position - hingeRoot.position;
        toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.001f)
        {
            return 1f; // Camera is directly above/below hinge — rare edge case.
        }

        // Dot against the STABLE closed-door forward baked at Awake.
        // Positive dot  → player is in "front" of the door → sideSign = +1
        // Negative dot  → player is "behind" the door     → sideSign = -1
        // This makes mouse-right always feel like "pushing the door away" from
        // whichever side you are standing on.
        float dot = Vector3.Dot(toCam.normalized, stableClosedForwardXZ);
        float sign = dot >= 0f ? 1f : -1f;

        if (flipSideDetection)
        {
            sign *= -1f;
        }

        return sign;
    }

    private float GetEffectiveMouseSensitivity()
    {
        // Existing scene doors may still serialize the old 0.2 default.
        // Treat that legacy value as the new default at runtime so we don't
        // have to re-save every door in the scene.
        if (Mathf.Approximately(mouseSensitivity, LegacyMouseSensitivityDefault))
        {
            return CurrentMouseSensitivityDefault;
        }

        return mouseSensitivity;
    }

    private void ApplyDoorRotation(float angle)
    {
        if (hingeRoot == null)
        {
            return;
        }

        float signedAngle = angle * GetOpenDirectionSign();
        hingeRoot.localRotation = closedLocalRotation * Quaternion.Euler(0f, signedAngle, 0f);
    }

    private float GetOpenDirectionSign()
    {
        float sign = hingeSide == HingeSide.Left ? 1f : -1f;
        return invertOpenDirection ? -sign : sign;
    }

    private bool IsCameraPointingAtThisDoor()
    {
        Camera cameraToUse = GetInteractionCamera();
        if (cameraToUse == null)
        {
            return false;
        }

        Ray ray = cameraToUse.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, triggerInteraction))
        {
            return false;
        }

        MouseDrivenDoor hitDoor = hit.collider.GetComponentInParent<MouseDrivenDoor>();
        if (hitDoor == this)
        {
            return true;
        }

        Transform hitTransform = hit.collider.transform;
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }

    private Camera GetInteractionCamera()
    {
        if (interactionCamera != null)
        {
            return interactionCamera;
        }

        interactionCamera = Camera.main;
        return interactionCamera;
    }

    private void SetHighlighted(bool shouldHighlight)
    {
        if (highlightMaterial == null || highlightRenderers == null || highlightRenderers.Length == 0)
        {
            isHighlighted = false;
            return;
        }

        if (isHighlighted == shouldHighlight)
        {
            return;
        }

        if (shouldHighlight)
        {
            CacheOriginalMaterials();
            AddHighlightMaterial();
        }
        else
        {
            RestoreOriginalMaterials();
        }

        isHighlighted = shouldHighlight;
    }

    private void CacheOriginalMaterials()
    {
        if (originalMaterials != null && originalMaterials.Length == highlightRenderers.Length)
        {
            return;
        }

        originalMaterials = new Material[highlightRenderers.Length][];
        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            Renderer rendererToCache = highlightRenderers[i];
            originalMaterials[i] = rendererToCache != null ? rendererToCache.sharedMaterials : null;
        }
    }

    private void AddHighlightMaterial()
    {
        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            Renderer rendererToHighlight = highlightRenderers[i];
            Material[] baseMaterials = originalMaterials[i];

            if (rendererToHighlight == null || baseMaterials == null || ContainsMaterial(baseMaterials, highlightMaterial))
            {
                continue;
            }

            Material[] highlightedMaterials = new Material[baseMaterials.Length + 1];
            for (int materialIndex = 0; materialIndex < baseMaterials.Length; materialIndex++)
            {
                highlightedMaterials[materialIndex] = baseMaterials[materialIndex];
            }

            highlightedMaterials[highlightedMaterials.Length - 1] = highlightMaterial;
            rendererToHighlight.sharedMaterials = highlightedMaterials;
        }
    }

    private void RestoreOriginalMaterials()
    {
        if (originalMaterials == null)
        {
            return;
        }

        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            if (highlightRenderers[i] != null && originalMaterials[i] != null)
            {
                highlightRenderers[i].sharedMaterials = originalMaterials[i];
            }
        }
    }

    private static bool ContainsMaterial(Material[] materials, Material materialToFind)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == materialToFind)
            {
                return true;
            }
        }

        return false;
    }

    private static bool WasLeftMousePressedThisFrame()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        pressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        pressed = pressed || Input.GetMouseButtonDown(0);
#endif

        return pressed;
    }

    private static bool WasLeftMouseReleasedThisFrame()
    {
        bool released = false;

#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        released = mouse != null && mouse.leftButton.wasReleasedThisFrame;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        released = released || Input.GetMouseButtonUp(0);
#endif

        return released;
    }

    private static bool IsLeftMouseHeld()
    {
        bool held = false;

#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        held = mouse != null && mouse.leftButton.isPressed;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        held = held || Input.GetMouseButton(0);
#endif

        return held;
    }

    private static Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.delta.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#else
        return Vector2.zero;
#endif
    }
}