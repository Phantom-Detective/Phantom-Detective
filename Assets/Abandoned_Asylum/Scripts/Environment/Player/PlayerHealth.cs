using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;          // Changed from 1 → 3
    public int currentLives;
    private Vector3 respawnPoint;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Health Bar UI")]
    public HealthBarUI healthBarUI;   // Drag your HealthBarUI GameObject here

    [Header("Invincibility")]
    public float invincibilityTime = 2f;
    private bool isInvincible = false;
    private bool isGameOver = false;

    [Header("Audio")]
    public AudioSource loseSound;    // ghost eating
    public AudioSource painSound;    // pain when hit

    [Header("Camera Reset")]
    public Transform playerCamera;

    private Quaternion startPlayerRotation;
    private Vector3 startCameraPosition;
    private Quaternion startCameraRotation;

    void Start()
    {
        startPlayerRotation = transform.rotation;

        if (playerCamera != null)
        {
            startCameraPosition = playerCamera.position;
            startCameraRotation = playerCamera.rotation;
        }

        currentLives = maxLives;
        respawnPoint = transform.position;
        Debug.Log("Player start!" + respawnPoint.x + " , " + respawnPoint.y + " , " + respawnPoint.z);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Initialize health bar
        if (healthBarUI != null)
            healthBarUI.SetLives(currentLives, maxLives);
    }

    public void TakeDamage()
    {
        if (isGameOver) return;
        if (isInvincible) return;

        currentLives--;
        Debug.Log("Lives left: " + currentLives);

        // Play pain sound
        if (painSound != null) painSound.Play();

        // Update health bar
        if (healthBarUI != null)
            healthBarUI.SetLives(currentLives, maxLives);

        if (currentLives <= 0)
        {
            isGameOver = true;
            Debug.Log("GAME OVER");
            ShowGameOver();
        }
        else
            Respawn();
    }

    void Respawn()
    {
        //transform.position = respawnPoint;
        isInvincible = true;
        Invoke("EndInvincibility", invincibilityTime);
        Debug.Log("Respawned!");
    }

    void EndInvincibility()
    {
        isInvincible = false;
    }

    void ShowGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Stop all ghosts
        GhostAI[] ghosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None);
        foreach (GhostAI ghost in ghosts)
            ghost.Disappear();

        // Play lose sound
        if (loseSound != null)
            loseSound.Play();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        //// Lock cursor back
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        // Stop sounds
        if (loseSound != null) loseSound.Stop();

        // Reset all ghosts
        GhostAI[] ghosts = FindObjectsByType<GhostAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log("Number of ghosts!" + ghosts.Length);

        foreach (GhostAI ghost in ghosts)
            ghost.ResetGhost();

        if (PuzzleInventory.Instance != null)
            PuzzleInventory.Instance.ResetInventory();

        if (PuzzleBoard.Instance != null)
            PuzzleBoard.Instance.ResetBoard();

        ZonePieceSpawner spawner = FindFirstObjectByType<ZonePieceSpawner>();
        if (spawner != null)
            spawner.RespawnPieces();

        // Reset player
        ResetPlayer();
    }


    public void RestartGameWihoutPuzzles()
    {
        // Reset all ghosts
        GhostAI[] ghosts = FindObjectsByType<GhostAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log("Number of ghosts!" + ghosts.Length);

        foreach (GhostAI ghost in ghosts)
            ghost.ResetGhost();

        // Reset player
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        currentLives = maxLives;
        isGameOver = false;
        isInvincible = false;

        if (healthBarUI != null)
            healthBarUI.SetLives(currentLives, maxLives);

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Move player first
        transform.position = respawnPoint;
        transform.rotation = startPlayerRotation;

        // Reset camera AFTER player moves
        if (playerCamera != null)
        {
            playerCamera.localPosition = new Vector3(0, 1.7f, 0);
            playerCamera.localRotation = Quaternion.identity;
        }

        if (cc != null) cc.enabled = true;

        // Also reset the rotationX in PlayerMovement
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
            pm.ResetCameraRotation();
    }

}
