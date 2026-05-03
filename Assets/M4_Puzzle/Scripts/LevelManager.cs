using UnityEngine;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI levelTransitionText;
    public GameObject levelCompletePanel;

    [Header("Settings")]
    public float messageDisplayTime = 3f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowNextLevelMessage(string message = "Next Level In Progress...")
    {
        StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        if (levelTransitionText != null)
        {
            levelTransitionText.text = message;
            levelTransitionText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageDisplayTime);

        if (levelTransitionText != null)
            levelTransitionText.gameObject.SetActive(false);
    }

    public void LoadNextLevel(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        ShowNextLevelMessage("Loading Next Level...");

        yield return new WaitForSeconds(2f);

        // Make sure cursor is locked before scene change
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}