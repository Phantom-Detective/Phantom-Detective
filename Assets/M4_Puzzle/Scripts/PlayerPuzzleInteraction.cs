using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPuzzleInteraction : MonoBehaviour
{
    public float detectionRange = 3f;
    public LayerMask puzzleLayer;
    public GameObject interactionPrompt;

    private Camera playerCamera;
    private FragmentPickup currentTarget;
    private bool tabPromptShown = false;

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
        ShowTabPromptIfAllCollected();
    }

    void DetectPuzzlePiece()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        FragmentPickup previousTarget = currentTarget;
        currentTarget = null;

        if (Physics.Raycast(ray, out hit, detectionRange, puzzleLayer))
        {
            FragmentPickup fragment = hit.collider.GetComponent <FragmentPickup > ();
            if (fragment != null)
                currentTarget = fragment;
        }

        if (previousTarget != null && previousTarget != currentTarget)
            previousTarget.Highlight(false);

        if (currentTarget != null)
        {
            currentTarget.Highlight(true);
            ShowPrompt("Press E to collect");
        }
        else
        {
            // Only hide prompt here if NOT showing the TAB prompt
            if (!tabPromptShown && interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    void ShowTabPromptIfAllCollected()
    {
        if (PuzzleInventory.Instance == null) return;

        if (PuzzleInventory.Instance.GetCollectedCount() >= 9 && !tabPromptShown)
        {
            tabPromptShown = true;
            ShowPrompt("Press TAB to solve puzzle");
            StartCoroutine(HidePromptAfterSeconds(3f));
        }
    }

    private System.Collections.IEnumerator HidePromptAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        // After hiding, we don't need to track it anymore
        // (player can still press TAB, they just won't see the reminder)
    }

    void ShowPrompt(string text)
    {
        if (interactionPrompt == null) return;

        var tmpText = interactionPrompt.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = text;

        interactionPrompt.SetActive(true);
    }

    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame && currentTarget != null)
        {
            currentTarget.Collect();
            currentTarget = null;
            if (interactionPrompt != null && !tabPromptShown)
                interactionPrompt.SetActive(false);
        }

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (PuzzleInventory.Instance != null && PuzzleInventory.Instance.GetCollectedCount() >= 9)
            {
                PuzzleBoard.Instance?.OpenBoard();
            }
        }
    }
}