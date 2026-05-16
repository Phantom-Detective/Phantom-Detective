using UnityEngine;

public class PuzzleToMenuBridge : MonoBehaviour
{
    [Header("Puzzle UI")]
    public GameObject puzzleBoardCanvas;

    [Header("Main Menu")]
    public MainMenuController mainMenuController;

    [Header("Spawner (DRAG PuzzleManager HERE)")]
    public ZonePieceSpawner pieceSpawner;

    public void ReturnToMainMenu()
    {
        Debug.Log("=== RETURN TO MAIN MENU STARTED ===");

        // 1. Reset inventory
        if (PuzzleInventory.Instance != null)
        {
            PuzzleInventory.Instance.ResetInventory();
            Debug.Log("✓ Inventory reset");
        }
        else
            Debug.LogError("✗ PuzzleInventory.Instance is NULL!");

        // 2. Reset board
        PuzzleBoard board = puzzleBoardCanvas?.GetComponent<PuzzleBoard>();
        if (board != null)
        {
            board.ResetBoard();
            Debug.Log("✓ Board reset");
        }
        else
            Debug.LogError("✗ PuzzleBoard not found on canvas!");

        // 3. Hide puzzle canvas
        if (puzzleBoardCanvas != null)
        {
            puzzleBoardCanvas.SetActive(false);
            Debug.Log("✓ Canvas hidden");
        }

        // 4. Respawn world pieces
        if (pieceSpawner != null)
        {
            Debug.Log("✓ Spawner found, calling RespawnPieces...");
            pieceSpawner.RespawnPieces();
        }
        else
        {
            Debug.LogError("✗ pieceSpawner is NOT assigned in Inspector! Drag PuzzleManager here.");
        }

        // 5. Return to main menu
        if (mainMenuController != null)
        {
            mainMenuController.OnReturnToMainMenuClicked();
            Debug.Log("✓ Main menu called");
        }
        else
            Debug.LogError("✗ MainMenuController not assigned!");

        Debug.Log("=== RETURN TO MAIN MENU COMPLETE ===");
    }
}