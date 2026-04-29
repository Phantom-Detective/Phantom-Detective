using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    public int currentLives;
    private Vector3 respawnPoint;

    void Start()
    {
        currentLives = maxLives;
        respawnPoint = transform.position;
    }

    public void TakeDamage()
    {
        currentLives--;
        Debug.Log("Lives left: " + currentLives);

        if (currentLives <= 0)
            Debug.Log("GAME OVER");
        else
            Respawn();
    }

    void Respawn()
    {
        transform.position = respawnPoint;
        Debug.Log("Respawned!");
    }
}