using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public int correctSlotIndex;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Find canvas properly
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
    }

    public void Setup(Sprite sprite, int correctIndex)
    {
        GetComponent<Image>().sprite = sprite;
        correctSlotIndex = correctIndex;

        // Make sure raycast is on
        GetComponent<Image>().raycastTarget = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Move to top of hierarchy so it renders above everything
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Try to find a slot - check the object we dropped on, or its parents
        PuzzleSlot slot = null;

        if (eventData.pointerEnter != null)
        {
            // Check if we hit the slot directly
            slot = eventData.pointerEnter.GetComponent<PuzzleSlot>();

            // If not, check if we hit a piece that's inside a slot (check parent)
            if (slot == null)
            {
                slot = eventData.pointerEnter.GetComponentInParent<PuzzleSlot>();
            }
        }

        if (slot != null)
        {
            // If slot already has a piece, swap them
            if (slot.transform.childCount > 0)
            {
                PuzzlePiece existingPiece = slot.transform.GetChild(0).GetComponent<PuzzlePiece>();
                if (existingPiece != null && existingPiece != this)
                {
                    // Move existing piece back to where we came from
                    existingPiece.transform.SetParent(originalParent);
                    existingPiece.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    // Update the slot reference for the piece we just moved out
                    PuzzleSlot oldSlot = originalParent.GetComponent<PuzzleSlot>();
                    if (oldSlot != null)
                    {
                        oldSlot.PlacePiece(existingPiece);
                    }
                }
            }

            // Place dragged piece into the new slot
            slot.PlacePiece(this);

            // Play sound if available
            if (PuzzleBoard.Instance != null && PuzzleBoard.Instance.audioSource != null
                && PuzzleBoard.Instance.placeSound != null)
            {
                PuzzleBoard.Instance.audioSource.PlayOneShot(PuzzleBoard.Instance.placeSound);
            }

            PuzzleBoard.Instance?.CheckWinCondition();
            return;
        }

        // Return to original position if not dropped on slot
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}