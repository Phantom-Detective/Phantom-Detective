using UnityEngine;

public class CelebrationManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject celebrationPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip celebrationSound;

    public void ShowCelebration()
    {
        // Hide win panel
        if (winPanel != null)
            winPanel.SetActive(false);

        // Show celebration panel
        if (celebrationPanel != null)
            celebrationPanel.SetActive(true);

        // Play celebration sound
        if (audioSource != null && celebrationSound != null)
            audioSource.PlayOneShot(celebrationSound);

        Debug.Log("Celebration shown!");
    }
}