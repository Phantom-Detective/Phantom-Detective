using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleBoard : MonoBehaviour
{
    public static PuzzleBoard Instance;

    [Header("UI References")]
    public GameObject puzzleBoardPanel;
    public Transform piecesParent; // Empty object inside canvas to hold loose pieces
    public List<PuzzleSlot> slots = new List<PuzzleSlot>();
    public GameObject puzzlePiecePrefab;

    [Header("Win Screen")]
    public GameObject winPanel;
    public Image fullPhotoImage;
    public Sprite completePhoto;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip placeSound;
    public AudioClip winSound;

    private bool isOpen = false;
    private List<PuzzlePiece> spawnedPieces = new List<PuzzlePiece>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        puzzleBoardPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void OpenBoard()
    {
        if (PuzzleInventory.Instance.GetCollectedCount() < 9)
        {
            Debug.Log("Collect all 9 pieces first!");
            return;
        }

        puzzleBoardPanel.SetActive(true);
        isOpen = true;

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Spawn pieces if not already spawned
        if (spawnedPieces.Count == 0)
        {
            SpawnPieces();
        }
    }

    public void CloseBoard()
    {
        puzzleBoardPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
        isOpen = false;

        // CRITICAL: Lock cursor back for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void SpawnPieces()
    {
        foreach (var collected in PuzzleInventory.Instance.collectedPieces)
        {
            GameObject pieceObj = Instantiate(puzzlePiecePrefab, piecesParent);
            PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
            piece.Setup(collected.pieceSprite, collected.correctSlotIndex);
            spawnedPieces.Add(piece);

            // Randomize starting position slightly so they don't stack perfectly
            RectTransform rt = pieceObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                Random.Range(-250f, 250f),
                Random.Range(-100f, 100f)
            );
        }
    }

    public void CheckWinCondition()
    {
        bool allCorrect = true;

        foreach (var slot in slots)
        {
            if (!slot.IsCorrectlyFilled())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            Debug.Log("PUZZLE COMPLETE!");
            StartCoroutine(ShowWinScreen());
        }
    }

    private System.Collections.IEnumerator ShowWinScreen()
    {
        yield return new WaitForSeconds(0.5f);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (fullPhotoImage != null && completePhoto != null)
                fullPhotoImage.sprite = completePhoto;
        }

        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBoard();
        }
    }
    public void ContinueToNextLevel()
    {
        CloseBoard();

        // Show the "Next Level" message
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ShowNextLevelMessage("Next Level In Progress...");
        }

        // actually load next scene in the future
        // LevelManager.Instance.LoadNextLevel("Level2");
    }

    private System.Collections.IEnumerator TransitionToNextLevel()
    {
        // Optional: fade out effect
        yield return new WaitForSeconds(1f);

        // Close the puzzle board
        CloseBoard();

        // Show "Next Level" message on screen
        ShowLevelTransitionMessage();

        // Optional: load next scene after delay
        // Uncomment the line below if you have a Scene 2
        // yield return new WaitForSeconds(3f);
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }

    private void ShowLevelTransitionMessage()
    {
        // You can create a simple UI text in your main canvas for this
        // For now, we'll just log it and you can expand
        Debug.Log("NEXT LEVEL IN PROGRESS...");

        // Optional: create a temporary on-screen message
        // This requires a TextMeshPro in your main game canvas
    }
    public void ResetBoard()
    {
        // Close board if it's open
        if (isOpen)
            CloseBoard();

        // Destroy all UI pieces that were created on the board
        foreach (var piece in spawnedPieces)
        {
            if (piece != null)
                Destroy(piece.gameObject);
        }
        spawnedPieces.Clear();

        // Hide win panel
        if (winPanel != null)
            winPanel.SetActive(false);

        // Make sure main panel is hidden
        if (puzzleBoardPanel != null)
            puzzleBoardPanel.SetActive(false);
    }
}