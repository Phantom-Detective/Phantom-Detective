using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoryRevealManager : MonoBehaviour
{
    public static StoryRevealManager Instance;

    [Header("UI References")]
    public GameObject storyPanel;
    public Image storyImage;

    [Header("Story Images (Drag in order: Story_01, Story_02, ... Story_09)")]
    public Sprite[] storyImages; // Size 9

    [Header("Settings")]
    public float displayDuration = 1.5f;

    private int currentStoryIndex = 0; // Tracks how many pieces collected so far

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);
    }

    // Call this whenever ANY piece is collected
    public void ShowNextStoryImage()
    {
        if (storyPanel == null || storyImage == null) return;
        if (storyImages == null || storyImages.Length == 0) return;

        // Only show if we still have images left
        if (currentStoryIndex >= storyImages.Length)
        {
            Debug.Log("All story images already shown!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisplayImage(currentStoryIndex));

        // Increment for next collection
        currentStoryIndex++;
    }

    private IEnumerator DisplayImage(int index)
    {
        storyImage.sprite = storyImages[index];
        storyPanel.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        storyPanel.SetActive(false);
    }
}