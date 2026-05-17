using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "SampleScene";

    [Header("Player")]
    public GameObject player;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject storyPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel;           

    [Header("Pause Button")]
    public Button pauseButton;

    [Header("Story UI Elements")]
    public Image storyImage;
    public TMP_Text storyText;
    public TMP_Text pageCounterText;

    [Header("Story Navigation Buttons")]
    public Button backButton;
    public Button nextButton;
    public Button playButton;

    [Header("Story Data")]
    public Sprite[] pageImages;
    [TextArea(3, 8)]
    public string[] pageTexts;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button storyButton;
    public Button manualButton;               

    private int currentPage = 0;
    private bool isPaused = false;
    public static bool gameStarted = false;
    [Header("Mission Text")]
    public GameObject missionTextPanel;

    void Start()
    {
        

    // Null-check everything critical
    if (player == null) Debug.LogError("PLAYER IS NULL");
    if (mainMenuPanel == null) Debug.LogError("MAIN MENU PANEL IS NULL");
    if (startButton == null) Debug.LogError("START BUTTON IS NULL");

    // Confirm listeners are actually wired
    if (startButton != null)
        Debug.Log("Start button listener count: " + 
                  startButton.onClick.GetPersistentEventCount());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.GetComponent<PlayerMovement>().enabled = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);    // ADD THIS
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);

        ShowMainMenu();
    }

    void Update()
    {
        // Only listen for Escape when game is started
        // and not in main menu or story panel
        if (!gameStarted) return;
        if (mainMenuPanel.activeSelf || storyPanel.activeSelf) return;

        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused)
                OnContinueClicked();
            else
                OnPauseButtonClicked();
        }
    }
    public void OnStartClicked()
    {
        gameStarted = true;
        mainMenuPanel.SetActive(false);
        player.GetComponent<PlayerMovement>().enabled = true;
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);

        // Show mission text
        if (missionTextPanel != null)
            StartCoroutine(ShowMissionText());
    }

    private IEnumerator ShowMissionText()
    {
        CanvasGroup cg = missionTextPanel.GetComponent<CanvasGroup>();
        missionTextPanel.SetActive(true);
        cg.alpha = 1f;

        yield return new WaitForSeconds(10f);   // visible for 10 seconds

        // Fade out over 1.5 seconds
        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        missionTextPanel.SetActive(false);
    }

    public void OnStoryClicked()
    {
        currentPage = 0;
        ShowStoryPanel();
        UpdateStoryPage();
    }

    //  called by the Manual button in the main menu
    public void OnManualClicked()
    {
        mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // called by the Back button inside the Controls panel
    public void OnControlsBackClicked()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        ShowMainMenu();
    }

    public void OnNextClicked()
    {
        if (currentPage < pageImages.Length - 1)
        {
            currentPage++;
            UpdateStoryPage();
        }
    }

    public void OnBackClicked()
    {
        if (currentPage == 0) ShowMainMenu();
        else { currentPage--; UpdateStoryPage(); }
    }

    public void OnPlayFromStoryClicked() => OnStartClicked();

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<PlayerMovement>().enabled = false;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    public void OnPauseButtonClicked()
    {
        PauseGame();
    }

    public void OnContinueClicked()
    {
        Debug.Log("=== CONTINUE CLICKED ===");
        Debug.Log("pausePanel active: " + pausePanel.activeSelf);
        Debug.Log("pausePanel name: " + pausePanel.name);
        Debug.Log("TimeScale before: " + Time.timeScale);

        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);

        Debug.Log("pausePanel active after: " + pausePanel.activeSelf);
        Debug.Log("TimeScale after: " + Time.timeScale);

        if (pauseButton != null) pauseButton.gameObject.SetActive(true);

        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = true;
            Debug.Log("PlayerMovement enabled: " + pm.enabled);
        }
        else Debug.LogError("PlayerMovement not found!");
    }

    public void OnSettingsClicked()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnSettingsBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void OnReturnToMainMenuClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.GetComponent<PlayerMovement>().enabled = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);

        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        storyPanel.SetActive(false);

        if (startButton != null) startButton.gameObject.SetActive(true);
        if (storyButton != null) storyButton.gameObject.SetActive(true);
        if (manualButton != null) manualButton.gameObject.SetActive(true);  // ADD THIS
    }

    private void ShowStoryPanel()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    private void UpdateStoryPage()
    {
        int total = pageImages != null ? pageImages.Length : 0;
        bool isLast = currentPage == total - 1;

        if (pageImages != null && currentPage < pageImages.Length)
            storyImage.sprite = pageImages[currentPage];

        if (pageTexts != null && currentPage < pageTexts.Length)
            storyText.text = pageTexts[currentPage];

        if (pageCounterText != null)
            pageCounterText.text = $"{currentPage + 1} / {total}";

        if (backButton != null) backButton.gameObject.SetActive(true);
        if (nextButton != null) nextButton.gameObject.SetActive(!isLast);
        if (playButton != null) playButton.gameObject.SetActive(isLast);
    }
}
