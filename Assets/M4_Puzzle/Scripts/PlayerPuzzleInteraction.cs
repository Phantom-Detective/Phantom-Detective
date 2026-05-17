using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerPuzzleInteraction : MonoBehaviour
{
    public float detectionRange = 3f;
    public LayerMask puzzleLayer;
    public GameObject interactionPrompt;

    private Camera playerCamera;
    private FragmentPickup currentTarget;
    private bool hasShownTabPrompt = false;   

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
            FragmentPickup fragment = hit.collider.GetComponent <FragmentPickup> ();
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
            // Hide prompt when not looking at anything
            // (Don't hide if it's the TAB prompt — let the coroutine handle that)
            if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                var tmp = interactionPrompt.GetComponent<TextMeshProUGUI>();
                if (tmp != null && tmp.text == "Press E to collect")
                {
                    interactionPrompt.SetActive(false);
                }
            }
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

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (PuzzleInventory.Instance != null && PuzzleInventory.Instance.GetCollectedCount() >= 9)
            {
                PuzzleBoard.Instance?.OpenBoard();
            }
        }
    }

    void ShowTabPromptIfAllCollected()
    {
        if (PuzzleInventory.Instance == null) return;
        if (interactionPrompt == null) return;

        int count = PuzzleInventory.Instance.GetCollectedCount();

        // RESET flag when inventory is cleared (game restarted)
        if (count < 9)
        {
            hasShownTabPrompt = false;
            return;
        }

        // Show tab prompt ONCE when reaching 9
        if (count >= 9 && !hasShownTabPrompt)
        {
            hasShownTabPrompt = true;
            ShowPrompt("Press TAB to solve puzzle");
            StartCoroutine(HidePromptAfterSeconds(2f));
        }
    }

    private System.Collections.IEnumerator HidePromptAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void ShowPrompt(string text)
    {
        if (interactionPrompt == null) return;
        var tmpText = interactionPrompt.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = text;
        interactionPrompt.SetActive(true);
    }
}
