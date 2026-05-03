using UnityEngine;
using TMPro;

public class PuzzleTrackerUI : MonoBehaviour
{
    public TextMeshProUGUI trackerText;
    public string format = "Pieces: {0}/{1}";

    void Start()
    {
        UpdateTracker(0, 9);
    }

    void OnEnable()
    {
        PuzzleInventory.OnPieceCollected += OnPieceCollected;
    }

    void OnDisable()
    {
        PuzzleInventory.OnPieceCollected -= OnPieceCollected;
    }

    void OnPieceCollected(int current, int total)
    {
        UpdateTracker(current, total);
    }

    void UpdateTracker(int current, int total)
    {
        if (trackerText != null)
            trackerText.text = string.Format(format, current, total);
    }
}