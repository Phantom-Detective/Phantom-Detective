//using UnityEngine;

//public class PlayerHealth : MonoBehaviour
//{
//    public int maxLives = 3;
//    public int currentLives;
//    private Vector3 respawnPoint;

//    void Start()
//    {
//        currentLives = maxLives;
//        respawnPoint = transform.position;
//    }

//    public void TakeDamage()
//    {
//        currentLives--;
//        Debug.Log("Lives left: " + currentLives);

//        if (currentLives <= 0)
//            Debug.Log("GAME OVER");
//        else
//            Respawn();
//    }

//    void Respawn()
//    {
//        transform.position = respawnPoint;
//        Debug.Log("Respawned!");
//    }
//}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 1;
    public int currentLives;
    private Vector3 respawnPoint;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Invincibility")]
    public float invincibilityTime = 2f;
    private bool isInvincible = false;
    private bool isGameOver = false;

    [Header("Audio")]
    public AudioSource loseSound;    // ghost eating
    public AudioSource winSound;     // ghost dying

    void Start()
    {
        currentLives = maxLives;
        respawnPoint = transform.position;
        Debug.Log("Player start!"+ respawnPoint.x+" , "+ respawnPoint.y+" , "+ respawnPoint.z);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TakeDamage()
    {
        if (isGameOver) return;
        if (isInvincible) return;

        currentLives--;
        Debug.Log("Lives left: " + currentLives);

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

    // Called when all shards collected
    public void TriggerWin()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Play win sound
        if (winSound != null)
            winSound.Play();
    }

    public void RestartGame()
    {
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Stop sounds
        if (loseSound != null) loseSound.Stop();
        if (winSound != null) winSound.Stop();

        // Reset all ghosts
        GhostAI[] ghosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None);
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
        transform.position = respawnPoint;
        Debug.Log("Player respawn!" + respawnPoint.x + " , " + respawnPoint.y + " , " + respawnPoint.z);
    }
}