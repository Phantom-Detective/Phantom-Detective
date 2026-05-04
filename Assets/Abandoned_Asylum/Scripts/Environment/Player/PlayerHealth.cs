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
    public int maxLives = 3;
    public int currentLives;
    private Vector3 respawnPoint;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Invincibility")]
    public float invincibilityTime = 2f;
    private bool isInvincible = false;
    private bool isGameOver = false;

    void Start()
    {
        currentLives = maxLives;
        respawnPoint = transform.position;

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
        transform.position = respawnPoint;
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

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    public void RestartGame()
    {
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset player
        ResetPlayer();

        // Reset all ghosts
        GhostAI[] ghosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None);
        foreach (GhostAI ghost in ghosts)
            ghost.ResetGhost();
    }

    public void ResetPlayer()
    {
        currentLives = maxLives;
        isGameOver = false;
        isInvincible = false;
        transform.position = respawnPoint;
        Debug.Log("Player reset!");
    }
}