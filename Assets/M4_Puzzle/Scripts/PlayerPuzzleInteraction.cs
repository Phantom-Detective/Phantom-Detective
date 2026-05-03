using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPuzzleInteraction : MonoBehaviour
{
    public float detectionRange = 3f;
    public LayerMask puzzleLayer;
    public GameObject interactionPrompt;

    private Camera playerCamera;
    private FragmentPickup currentTarget;

    void Start()
    {
        playerCamera = Camera.main;
        if (puzzleLayer == 0)
            puzzleLayer = LayerMask.GetMask("PuzzlePiece");
    }

    void Update()
    {
        DetectPuzzlePiece();
        HandleInput();

        // Show "Press TAB" prompt when all pieces collected
        if (PuzzleInventory.Instance.GetCollectedCount() >= 9)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                var tmpText = interactionPrompt.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = "Press TAB to solve puzzle";
            }
        }
    }

    void DetectPuzzlePiece()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        FragmentPickup previousTarget = currentTarget;
        currentTarget = null;

        if (Physics.Raycast(ray, out hit, detectionRange, puzzleLayer))
        {
            FragmentPickup fragment = hit.collider.GetComponent<FragmentPickup>();
            if (fragment != null)
                currentTarget = fragment;
        }

        if (previousTarget != null && previousTarget != currentTarget)
            previousTarget.Highlight(false);

        if (currentTarget != null)
        {
            currentTarget.Highlight(true);
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
        else
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame && currentTarget != null)
        {
            currentTarget.Collect();
            currentTarget = null;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
        // Open puzzle board when all pieces collected
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (PuzzleInventory.Instance.GetCollectedCount() >= 9)
            {
                PuzzleBoard.Instance.OpenBoard();
            }
        }
    }
}